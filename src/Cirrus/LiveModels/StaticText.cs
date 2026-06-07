using CommunityToolkit.Mvvm.ComponentModel;

namespace Cirrus.LiveModels;

public partial class StaticText : ObservableObject
{
    [ObservableProperty] public partial string DisplayText { get; set; } = string.Empty;

    [ObservableProperty] public partial object? Attachment { get; set; }

    public StaticText()
    {
        // Empty constructor.
    }

    public StaticText(string displayText)
    {
        DisplayText = displayText;
    }
}

public partial class StaticText<T> : StaticText
{
    public new T? Attachment
    {
        get => base.Attachment is T tValue ? tValue : default;
        set => base.Attachment = value;
    }

    public StaticText()
    {
        // No further actions.
    }
    
    public StaticText(string displayText) : base(displayText)
    {
        // No further actions.
    }
}