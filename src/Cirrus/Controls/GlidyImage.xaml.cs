using Cirrus.Behaviors;
using Cirrus.Composition;
using Cirrus.Extensions;
using Cirrus.Primitives;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;

namespace Cirrus.Controls;

public sealed partial class GlidyImage
{
    private readonly ThemeAwareResource<CompositionColorBrush> _strokeBrush;
    public BorderComposition Dimensions { get; }

    public static readonly DependencyProperty ImageUriSourceProperty = DependencyProperty.Register(
        nameof(ImageUriSource), typeof(Uri), typeof(GlidyImage), new(null, OnImageSourceChanged));
    public Uri ImageUriSource
    {
        get => (Uri)GetValue(ImageUriSourceProperty);
        set => SetValue(ImageUriSourceProperty, value);
    }

    public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(
        nameof(Stretch), typeof(Stretch), typeof(GlidyImage), new PropertyMetadata(Stretch.UniformToFill, OnStretchChanged));

    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }
    
    private CompositionBrush? _imageBrush;
    
    public GlidyImage()
    {
        InitializeComponent();
        this.AttachEventTriggerBehaviorToCommand<FrameworkElement, LoadedEventTriggerBehavior>(LoadedCommand);
        var compositor = this.GetVisual().Compositor;
        _strokeBrush = compositor.BuildThemeAwareCardStrokeBrush();
        Dimensions = new(
            compositor,
            0f, 0f, 0f, 0f,
            [compositor.CreateColorBrush(Colors.Transparent)],
            [_strokeBrush.Resource]
        );
        Dimensions.InnerVisuals[0].Opacity = 1f;
        ElementCompositionPreview.SetElementChildVisual(this, Dimensions.Visual);
    }

    [RelayCommand]
    private void OnLoaded()
    {
        _strokeBrush.ApplyTheme(ActualTheme);
    }

    private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not GlidyImage glidyImage) return;
        var compositor = glidyImage.GetVisual().Compositor;
        CompositionBrush newBrush;
        if (e.NewValue is not Uri newUri)
        {
            newBrush = compositor.CreateColorBrush(Colors.Transparent);
            glidyImage._imageBrush = null;
        }
        else
        {
            var surface = LoadedImageSurface.StartLoadFromUri(newUri);
            var surfaceBrush = compositor.CreateSurfaceBrush(surface);
            surfaceBrush.Stretch = glidyImage.Stretch switch
            {
                Stretch.None => CompositionStretch.None,
                Stretch.Fill => CompositionStretch.Fill,
                Stretch.Uniform => CompositionStretch.Uniform,
                _ => CompositionStretch.UniformToFill
            };
            newBrush = surfaceBrush;
            glidyImage._imageBrush = surfaceBrush;
        }
        glidyImage.Dimensions.UpdateSingleBackgroundBrush(newBrush);
    }
    
    private static void OnStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not GlidyImage { _imageBrush: CompositionSurfaceBrush surfaceBrush } ||
            e.NewValue is not Stretch stretch) return;
        surfaceBrush.Stretch = stretch switch
        {
            Stretch.None => CompositionStretch.None,
            Stretch.Fill => CompositionStretch.Fill,
            Stretch.Uniform => CompositionStretch.Uniform,
            _ => CompositionStretch.UniformToFill
        };
    }
}