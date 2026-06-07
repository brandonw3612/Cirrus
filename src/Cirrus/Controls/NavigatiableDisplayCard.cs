using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using WinRT;

namespace Cirrus.Controls;

[GeneratedBindableCustomProperty([
    nameof(IsExplicit)
], [])]
[ContentProperty(Name = nameof(CustomContent))]
public partial class NavigatiableDisplayCard : Control
{
    public ImageSource? Image
    {
        get => (ImageSource?)GetValue(ImageProperty);
        set => SetValue(ImageProperty, value);
    }
    public static readonly DependencyProperty ImageProperty =
        DependencyProperty.Register(nameof(Image), typeof(ImageSource), typeof(NavigatiableDisplayCard),
            new PropertyMetadata(null));

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(NavigatiableDisplayCard),
            new PropertyMetadata(null, OnTextChanged));
    
    public bool IsExplicit
    {
        get => (bool)GetValue(IsExplicitProperty);
        set => SetValue(IsExplicitProperty, value);
    }
    public static readonly DependencyProperty IsExplicitProperty =
        DependencyProperty.Register(nameof(IsExplicit), typeof(bool), typeof(NavigatiableDisplayCard),
            new PropertyMetadata(false));

    public string? Subtitle
    {
        get => (string?)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }
    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(NavigatiableDisplayCard),
            new PropertyMetadata(null, OnTextChanged));

    public object? CustomContent
    {
        get => GetValue(CustomContentProperty);
        set => SetValue(CustomContentProperty, value);
    }
    public static readonly DependencyProperty CustomContentProperty =
        DependencyProperty.Register(nameof(CustomContent), typeof(object), typeof(NavigatiableDisplayCard),
            new PropertyMetadata(null));

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NavigatiableDisplayCard card) return;
        VisualStateManager.GoToState(card, card.Title is { Length: > 0 } ? "ShowTitle" : "HideTitle", false);
        VisualStateManager.GoToState(card, card.Subtitle is { Length: > 0 } ? "ShowSubtitle" : "HideSubtitle", false);
    }

    public NavigatiableDisplayCard()
    {
        DefaultStyleKey = typeof(NavigatiableDisplayCard);
    }
}