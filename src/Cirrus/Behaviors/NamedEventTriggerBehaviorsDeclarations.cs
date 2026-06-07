using Cirrus.Controls;
using Cirrus.Generated.Attributes;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;

namespace Cirrus.Behaviors;

[NamedEventTriggerBehavior(typeof(FrameworkElement), nameof(FrameworkElement.ActualThemeChanged))]
[NamedEventTriggerBehavior(typeof(FrameworkElement), nameof(TitleBar.BackRequested), Assemblies = ["Microsoft.WinUI"])]
[NamedEventTriggerBehavior(typeof(FrameworkElement), nameof(ButtonBase.Click))]
[NamedEventTriggerBehavior(typeof(FrameworkElement), nameof(FrameworkElement.DataContextChanged))]
[NamedEventTriggerBehavior(typeof(UIElement), nameof(UIElement.DoubleTapped))]
[NamedEventTriggerBehavior(typeof(KeyboardAccelerator), nameof(KeyboardAccelerator.Invoked))]
[NamedEventTriggerBehavior(typeof(UIElement), nameof(SplitView.PaneClosed))]
[NamedEventTriggerBehavior(typeof(UIElement), nameof(TitleBar.PaneToggleRequested), Assemblies = ["Microsoft.WinUI"])]
[NamedEventTriggerBehavior(typeof(UIElement), nameof(UIElement.PointerCaptureLost))]
[NamedEventTriggerBehavior(typeof(UIElement), nameof(UIElement.PointerEntered))]
[NamedEventTriggerBehavior(typeof(UIElement), nameof(UIElement.PointerExited))]
[NamedEventTriggerBehavior(typeof(UIElement), nameof(UIElement.PointerMoved))]
[NamedEventTriggerBehavior(typeof(UIElement), nameof(UIElement.PointerPressed))]
[NamedEventTriggerBehavior(typeof(UIElement), nameof(UIElement.PointerReleased))]
[NamedEventTriggerBehavior(typeof(UIElement), nameof(UIElement.PointerWheelChanged))]
[NamedEventTriggerBehavior(typeof(UIElement), nameof(UIElement.PreviewKeyDown))]
[NamedEventTriggerBehavior(typeof(UIElement), nameof(UIElement.PreviewKeyUp))]
[NamedEventTriggerBehavior(typeof(UIElement), nameof(AutoSuggestBox.QuerySubmitted))]
[NamedEventTriggerBehavior(typeof(UIElement), nameof(UIElement.RightTapped))]
[NamedEventTriggerBehavior(typeof(FrameworkElement), nameof(FrameworkElement.SizeChanged))]
[NamedEventTriggerBehavior(typeof(UIElement), nameof(AutoSuggestBox.SuggestionChosen))]
[NamedEventTriggerBehavior(typeof(UIElement), nameof(UIElement.Tapped))]
[NamedEventTriggerBehavior(typeof(UIElement), nameof(AutoSuggestBox.TextChanged))]
[NamedEventTriggerBehavior(typeof(FrameworkElement), nameof(FrameworkElement.Unloaded))]
[NamedEventTriggerBehavior(typeof(GlidyScroller), nameof(GlidyScroller.UserInteractionEnded))]
[NamedEventTriggerBehavior(typeof(GlidyScroller), nameof(GlidyScroller.UserInteractionStarted))]
[NamedEventTriggerBehavior(typeof(UIElement), nameof(ScrollView.ViewChanged))]
public class NamedEventTriggerBehaviorDeclarations;