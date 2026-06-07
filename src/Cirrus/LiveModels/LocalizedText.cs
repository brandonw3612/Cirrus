using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;

namespace Cirrus.LiveModels;

public partial class LocalizedText : ObservableObject
{
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Localized))]
    public partial string? ResourceKey { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Localized))]
    public partial string? ResourcePath { get; set; }

    [ObservableProperty] public partial object? Attachment { get; set; }

    public string Localized =>
        ResourceKey switch
        {
            null => "{ResourceKey unspecified}",
            _ when ResourcePath is null => ResourceKey.GetLocalized() ?? "{Invalid ResourceKey}",
            _ => ResourceKey.GetLocalized(ResourcePath) ?? "{Invalid ResourceKey/Path}"
        };

    public LocalizedText()
    {
        // Empty constructor.
    }
    
    public LocalizedText(string resourceKey)
    {
        ResourceKey = resourceKey;
    }

    public LocalizedText(string resourceKey, string resourcePath)
    {
        ResourceKey = resourceKey;
        ResourcePath = resourcePath;
    }
}

public partial class LocalizedText<T> : LocalizedText
{
    public new T? Attachment
    {
        get => base.Attachment is T tValue ? tValue : default;
        set => base.Attachment = value;
    }

    public LocalizedText()
    {
        // Empty constructor.
    }
    
    public LocalizedText(string resourceKey) : base(resourceKey)
    {
        // No further actions.
    }

    public LocalizedText(string resourceKey, string resourcePath) : base(resourceKey, resourcePath)
    {
        // No further actions.
    }
}