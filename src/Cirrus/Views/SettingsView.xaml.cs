using Cirrus.Primitives;
using Cirrus.ViewModels;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Animations.Expressions;
using Microsoft.UI.Xaml.Hosting;

namespace Cirrus.Views;

public abstract class SettingsViewBase : View<SettingsViewModel>;

public sealed partial class SettingsView
{
    public SettingsView()
    {
        InitializeComponent();
    }

    [RelayCommand]
    private void OnLoaded()
    {
        var scrollViewerManipulationPropertySet =
            ElementCompositionPreview.GetScrollViewerManipulationPropertySet(RootScrollViewer);

        var scrollingProperties = scrollViewerManipulationPropertySet
            .GetSpecializedReference<ManipulationPropertySetReferenceNode>();

        var progressNode = ExpressionFunctions.Clamp(-scrollingProperties.Translation.Y / 100, 0, 1);
        
        var backBorderVisual = HeaderFloatingBackBorder.GetVisual();
        backBorderVisual.StartAnimation("Opacity", progressNode);

        var textVisual = HeaderTextBlock.GetVisual();
        var textVisualAnimation = progressNode * 32 + 16;
        textVisual.StartAnimation("Offset.X", textVisualAnimation);
    }
}
