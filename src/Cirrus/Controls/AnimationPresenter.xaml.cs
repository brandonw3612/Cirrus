using Cirrus.Behaviors;
using Cirrus.Resources;
using CommunityToolkit.Mvvm.Input;
using ComputeSharp.D2D1.WinUI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml;
using System.Numerics;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Cirrus.Extensions;
using Cirrus.Utilities.Impressionist.PaletteExtractors;
using Wacton.Unicolour;

namespace Cirrus.Controls;

public sealed partial class AnimationPresenter
{
    public IRandomAccessStreamReference Source
    {
        get => (IRandomAccessStreamReference)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(nameof(Source), typeof(IRandomAccessStreamReference), typeof(AnimationPresenter),
            new PropertyMetadata(null, OnSourceChanged));

    public bool IsPlaying
    {
        get => (bool)GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }

    public static readonly DependencyProperty IsPlayingProperty =
        DependencyProperty.Register(nameof(IsPlaying), typeof(bool), typeof(AnimationPresenter),
            new PropertyMetadata(false, OnIsPlayingChanged));

    private sealed record ColorPalette4(Unicolour C0, Unicolour C1, Unicolour C2, Unicolour C3);

    private static readonly int[,] Permutations4 =
    {
        { 0, 1, 2, 3 }, { 0, 1, 3, 2 }, { 0, 2, 1, 3 }, { 0, 2, 3, 1 }, { 0, 3, 1, 2 }, { 0, 3, 2, 1 },
        { 1, 0, 2, 3 }, { 1, 0, 3, 2 }, { 1, 2, 0, 3 }, { 1, 2, 3, 0 }, { 1, 3, 0, 2 }, { 1, 3, 2, 0 },
        { 2, 0, 1, 3 }, { 2, 0, 3, 1 }, { 2, 1, 0, 3 }, { 2, 1, 3, 0 }, { 2, 3, 0, 1 }, { 2, 3, 1, 0 },
        { 3, 0, 1, 2 }, { 3, 0, 2, 1 }, { 3, 1, 0, 2 }, { 3, 1, 2, 0 }, { 3, 2, 0, 1 }, { 3, 2, 1, 0 },
    };

    private const float ColorTransitionSpeed = 4.5f;
    private const float SettleEpsilonSq = 1e-6f;

    private readonly Lock _stateLock = new();
    private readonly Lock _effectLock = new();

    private static Unicolour Black => field ??= new(ColourSpace.Rgb, 0, 0, 0);

    private readonly Unicolour[] _currentColors = Enumerable.Repeat<Unicolour>(new(ColourSpace.Rgb, 0, 0, 0), 4).ToArray();
    private readonly Unicolour[] _targetColors = new Unicolour[4];
    private ColorPalette4? _pendingTargetColors;
    private bool _hasReceivedPalette;
    private bool _isPlaying;
    private bool _isTransitioning;
    private double _animationTime;
    private CancellationTokenSource? _sourceLoadCts;

    private PixelShaderEffect<Isolation>? _shaderEffect = new();
    private Vector2 _canvasSize;

    public AnimationPresenter()
    {
        InitializeComponent();
        this.AttachEventTriggerBehaviorToCommand<FrameworkElement, UnloadedEventTriggerBehavior>(new RelayCommand(() =>
        {
            var oldCts = Interlocked.Exchange(ref _sourceLoadCts, null);
            oldCts?.Cancel();
            oldCts?.Dispose();

            Interlocked.Exchange(ref BackgroundPresenter, null)?.RemoveFromVisualTree();

            lock (_effectLock)
            {
                Interlocked.Exchange(ref _shaderEffect, null)?.Dispose();
            }
        }));
        this.AttachEventTriggerBehaviorToCommand<FrameworkElement, SizeChangedEventTriggerBehavior>(new RelayCommand<SizeChangedEventArgs>(_ =>
        {
            if (BackgroundPresenter is not { } presenter) return;
            _canvasSize = new Vector2(
                presenter.ConvertDipsToPixels((float)ActualWidth, CanvasDpiRounding.Round),
                presenter.ConvertDipsToPixels((float)ActualHeight, CanvasDpiRounding.Round));
        }));
    }

    private static async void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not AnimationPresenter background) return;

        var newCts = new CancellationTokenSource();
        var oldCts = Interlocked.Exchange(ref background._sourceLoadCts, newCts);
        oldCts?.Cancel();
        oldCts?.Dispose();
        var token = newCts.Token;

        if (e.NewValue is not IRandomAccessStreamReference reference) return;

        try
        {
            using var stream = await reference.OpenReadAsync().AsTask(token);
            var decoder = await BitmapDecoder.CreateAsync(stream).AsTask(token);
            var colors = await decoder.GetColorMap();
            if (token.IsCancellationRequested) return;

            var palette = Mondrian.Default.ExtractPalette(colors, 4, new());
            if (token.IsCancellationRequested || palette.Palette.Count == 0) return;

            var labColors = new Unicolour[4];
            for (int i = 0; i < 4; i++)
            {
                labColors[i] = palette.Palette[i % palette.Palette.Count];
            }

            var nextPalette = new ColorPalette4(labColors[0], labColors[1], labColors[2], labColors[3]);
            lock (background._stateLock)
            {
                background._pendingTargetColors = nextPalette;
                background._isTransitioning = true;
                if (background.BackgroundPresenter is { } presenter)
                {
                    presenter.Paused = false;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch
        {
        }
    }

    private static void OnIsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not AnimationPresenter background) return;
        lock (background._stateLock)
        {
            background._isPlaying = (bool)e.NewValue;
            if (background.BackgroundPresenter is { } presenter)
            {
                presenter.Paused = !(background._isPlaying || background._isTransitioning);
            }
        }
    }

    private void BackgroundPresenter_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
    {
        var dt = Math.Clamp((float)args.Timing.ElapsedTime.TotalSeconds, 0f, 0.1f);

        ColorPalette4? incoming;
        bool playing;
        bool transitioning;
        lock (_stateLock)
        {
            incoming = _pendingTargetColors;
            _pendingTargetColors = null;
            playing = _isPlaying;
            transitioning = _isTransitioning;
        }

        if (playing)
        {
            _animationTime += dt;
        }

        if (incoming is not null)
        {
            ReadOnlySpan<Unicolour> candidates = [incoming.C0, incoming.C1, incoming.C2, incoming.C3];
            if (_hasReceivedPalette)
            {
                MatchPaletteToCurrent(candidates);
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    _targetColors[i] = candidates[i];
                }
                _hasReceivedPalette = true;
            }
            transitioning = true;
        }

        if (transitioning && dt > 0f)
        {
            float factor = 1f - MathF.Exp(-ColorTransitionSpeed * dt);
            bool allSettled = true;

            for (int i = 0; i < 4; i++)
            {
                _currentColors[i] = _currentColors[i].Mix(_targetColors[i], ColourSpace.Oklab, factor);
                if (_currentColors[i].Difference(_targetColors[i], DeltaE.Ok) > SettleEpsilonSq)
                {
                    allSettled = false;
                }
                else
                {
                    _currentColors[i] = _targetColors[i];
                }
            }

            if (allSettled)
            {
                lock (_stateLock)
                {
                    if (_pendingTargetColors is null)
                    {
                        _isTransitioning = false;
                    }
                    if (!_isPlaying && !_isTransitioning)
                    {
                        sender.Paused = true;
                    }
                }
            }
        }

        lock (_effectLock)
        {
            if (_shaderEffect is null) return;
            _shaderEffect.ConstantBuffer = new Isolation(
                _canvasSize,
                (float)_animationTime,
                _currentColors[0].ToSrgbVector3(),
                _currentColors[1].ToSrgbVector3(),
                _currentColors[2].ToSrgbVector3(),
                _currentColors[3].ToSrgbVector3(),
                0,
                0,
                0,
                false,
                true);
        }
    }

    private void BackgroundPresenter_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
    {
        using var session = args.DrawingSession;
        lock (_effectLock)
        {
            if (_shaderEffect is not null)
                session.DrawImage(_shaderEffect);
        }
    }

    private void BackgroundPresenter_CreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
    {
        _canvasSize = new Vector2(
             sender.ConvertDipsToPixels((float)ActualWidth, CanvasDpiRounding.Round),
             sender.ConvertDipsToPixels((float)ActualHeight, CanvasDpiRounding.Round));

        lock (_stateLock)
        {
            sender.Paused = !(_isPlaying || _isTransitioning);
        }
    }

    private void MatchPaletteToCurrent(ReadOnlySpan<Unicolour> candidates)
    {
        int bestRow = 0;
        var bestDistance = double.PositiveInfinity;

        for (int row = 0; row < 24; row++)
        {
            var dist =
                _currentColors[0].Difference(candidates[Permutations4[row, 0]], DeltaE.Ok) +
                _currentColors[1].Difference(candidates[Permutations4[row, 1]], DeltaE.Ok) +
                _currentColors[2].Difference(candidates[Permutations4[row, 2]], DeltaE.Ok) +
                _currentColors[3].Difference(candidates[Permutations4[row, 3]], DeltaE.Ok);

            if (dist < bestDistance)
            {
                bestDistance = dist;
                bestRow = row;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            _targetColors[i] = candidates[Permutations4[bestRow, i]];
        }
    }
}