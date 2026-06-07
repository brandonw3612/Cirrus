using Cirrus.Behaviors;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using QRCoder;
using Image = Microsoft.UI.Xaml.Controls.Image;

namespace Cirrus.Controls;

public partial class QrCode : Control
{
    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }
    public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(nameof(Stretch),
        typeof(Stretch), typeof(QrCode), new PropertyMetadata(Stretch.Uniform));

    public string? CodeSource
    {
        get => (string)GetValue(CodeSourceProperty);
        set => SetValue(CodeSourceProperty, value);
    }
    public static readonly DependencyProperty CodeSourceProperty = DependencyProperty.Register(
        nameof(CodeSource), typeof(string), typeof(QrCode), new PropertyMetadata(null, OnCodeSourceChanged));
    
    private Image? _image;
    
    public QrCode()
    {
        DefaultStyleKey = typeof(QrCode);
        this.AttachEventTriggerBehaviorToCommand<FrameworkElement, ActualThemeChangedEventTriggerBehavior>
            (new AsyncRelayCommand(UpdateImageAsync));
    }

    protected override async void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _image = GetTemplateChild("Image") as Image;
        await UpdateImageAsync();
    }
    
    private static async void OnCodeSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not QrCode qrCode) return;
        await qrCode.UpdateImageAsync();
    }
    
    private async Task UpdateImageAsync()
    {
        if (_image is null) return;
        if (CodeSource is not { Length: > 0 } source) return;
        
        using var generator = new QRCodeGenerator();
        using var codeData = generator.CreateQrCode(source, QRCodeGenerator.ECCLevel.H);
        using var qrCode = new PngByteQRCode(codeData);
        var backgroundColorBytes = ActualTheme is ElementTheme.Light
            ? new byte[] { 0xff, 0xff, 0xff }
            : new byte[] { 0x2b, 0x2b, 0x2b };
        var foregroundColorBytes = ActualTheme is ElementTheme.Light
            ? new byte[] { 0x40, 0x40, 0x40 }
            : new byte[] { 0xcf, 0xcf, 0xcf };
        var qrGraphic = qrCode.GetGraphic(20, foregroundColorBytes, backgroundColorBytes, false);

        BitmapImage imageSource = new();
        await imageSource.SetSourceAsync(new MemoryStream(qrGraphic).AsRandomAccessStream());
        _image.Source = imageSource;
    }
}