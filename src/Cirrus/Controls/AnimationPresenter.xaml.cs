using Cirrus.Behaviors;
using Cirrus.Helpers.Impressionist;
using Cirrus.Helpers.Impressionist.Impressionist.Implementations;
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

    private Vector3[]? displayColors;
    private PixelShaderEffect<Isolation>? shaderEffect = new();
    private Vector2 canvasSize;

    public AnimationPresenter()
    {
        InitializeComponent();
        this.AttachEventTriggerBehaviorToCommand<FrameworkElement, UnloadedEventTriggerBehavior>(new RelayCommand(() =>
        {
            Interlocked.Exchange(ref BackgroundPresenter, null)?.RemoveFromVisualTree();
            Interlocked.Exchange(ref shaderEffect, null)?.Dispose();
        }));
        this.AttachEventTriggerBehaviorToCommand<FrameworkElement, SizeChangedEventTriggerBehavior>(new RelayCommand<SizeChangedEventArgs>(args =>
        {
            canvasSize = new Vector2(
                BackgroundPresenter.ConvertDipsToPixels((float)ActualWidth, CanvasDpiRounding.Round),
                BackgroundPresenter.ConvertDipsToPixels((float)ActualHeight, CanvasDpiRounding.Round));
        }));
    }
    private static async void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not AnimationPresenter background) return;
        if (e.NewValue is not IRandomAccessStreamReference reference) return;
        using var stream = await reference.OpenReadAsync();
        var decoder = await BitmapDecoder.CreateAsync(stream);
        var colors = await ImageColorHelper.GetPixelColor(decoder);
        var palette = await AutoPaletteGenerator.CreatePalette(colors, 4, false, false, true);
        background.displayColors = [.. palette.Palette.Select(t => t / 255)];
    }

    private static async void OnIsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not AnimationPresenter background) return;
        background.BackgroundPresenter.Paused = !(bool)e.NewValue;
    }

    private void BackgroundPresenter_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
    {
        shaderEffect?.ConstantBuffer = new Isolation(
            canvasSize,
            (float)(args?.Timing.TotalTime.TotalSeconds ?? 0),
            displayColors == null ? Vector3.Zero : displayColors[0],
            displayColors == null ? Vector3.Zero : displayColors[1],
            displayColors == null ? Vector3.Zero : displayColors[2],
            displayColors == null ? Vector3.Zero : displayColors[3],
            0,
            0,
            0,
            false,
            true);
    }

    private void BackgroundPresenter_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
    {
        if (args == null) return;
        using var session = args.DrawingSession;
        if (shaderEffect is not null)
            session.DrawImage(shaderEffect);
    }

    private void BackgroundPresenter_CreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
    {
        canvasSize = new Vector2(
             BackgroundPresenter.ConvertDipsToPixels((float)ActualWidth, CanvasDpiRounding.Round),
             BackgroundPresenter.ConvertDipsToPixels((float)ActualHeight, CanvasDpiRounding.Round));
    }
}