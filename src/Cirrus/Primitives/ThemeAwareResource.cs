using Cirrus.Constants;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;

namespace Cirrus.Primitives;

public class ThemeAwareResource<T>
{
    public T Resource { get; }

    public required Action<T> ToLight { get; init; }
    
    public required Action<T> ToDark { get; init; }
    
    public ThemeAwareResource(T resource)
    {
        Resource = resource;
        WeakReferenceMessenger.Default.Register<ThemeAwareResource<T>, object, string>
            (this, MessengerTokens.ThemeChanged, OnThemeChanged);
    }
    
    public void ApplyTheme(ElementTheme theme)
    {
        switch (theme)
        {
            case ElementTheme.Light: ToLight(Resource); return;
            case ElementTheme.Default:
            case ElementTheme.Dark:
            default: ToDark(Resource); return;
        }
    }
    
    public void ApplyTheme(ApplicationTheme theme)
    {
        switch (theme)
        {
            case ApplicationTheme.Light: ToLight(Resource); return;
            case ApplicationTheme.Dark:
            default: ToDark(Resource); return;
        }
    }
    
    private static void OnThemeChanged(ThemeAwareResource<T> recipient, object message)
    {
        switch (message)
        {
            case ElementTheme et:
                switch (et)
                {
                    case ElementTheme.Light: recipient.ToLight(recipient.Resource); return;
                    case ElementTheme.Default:
                    case ElementTheme.Dark:
                    default: recipient.ToDark(recipient.Resource); return;
                }
            case ApplicationTheme at:
                switch (at)
                {
                    case ApplicationTheme.Light: recipient.ToLight(recipient.Resource); return;
                    case ApplicationTheme.Dark:
                    default: recipient.ToDark(recipient.Resource); return;
                }
            default: return;
        }
    }
}