using Cirrus.Behaviors;
using Cirrus.Helpers;
using Cirrus.Resources;
using CommunityToolkit.Mvvm.Input;
using ComputeSharp.D2D1.WinUI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Numerics;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace Cirrus.Controls;

public sealed partial class FluidBackground
{
    public IRandomAccessStreamReference Source
    {
        get => (IRandomAccessStreamReference)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(nameof(Source), typeof(IRandomAccessStreamReference), typeof(FluidBackground),
            new PropertyMetadata(null, OnSourceChanged));

    public bool IsPlaying
    {
        get => (bool)GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }

    public static readonly DependencyProperty IsPlayingProperty =
        DependencyProperty.Register(nameof(IsPlaying), typeof(bool), typeof(FluidBackground),
            new PropertyMetadata(false, OnIsPlayingChanged));

    private Vector3[]? displayColors;
    private readonly PixelShaderEffect<IsolationEffect> shaderEffect = new();
    private Vector2 canvasSize;

    public FluidBackground()
    {
        InitializeComponent();
        this.AttachEventTriggerBehaviorToCommand<FrameworkElement, UnloadedEventTriggerBehavior>(new RelayCommand(() =>
        {
            BackgroundPresenter.RemoveFromVisualTree();
            shaderEffect.Dispose();

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
        if (d is not FluidBackground background) return;
        if (e.NewValue is not IRandomAccessStreamReference reference) return;
        using var stream = await reference.OpenReadAsync();
        var result = await ColorHelper.ExtractPaletteFromStream(stream);
        background.displayColors = [.. result.Select(t => t / 255)];
    }

    private static async void OnIsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FluidBackground background) return;
        background.BackgroundPresenter.Paused = !(bool)e.NewValue;
    }

    private void BackgroundPresenter_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
    {
        shaderEffect?.ConstantBuffer = new IsolationEffect(
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