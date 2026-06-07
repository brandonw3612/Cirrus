using Windows.Foundation.Collections;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Cirrus.Helpers;

public class ListViewHelper
{
    public static readonly DependencyProperty BindEmptyVisibilityProperty = DependencyProperty.RegisterAttached(
        "BindEmptyVisibility", typeof(ListViewBase), typeof(ListViewHelper),
        new PropertyMetadata(null, OnBindEmptyVisibilityChanged));

    public static void SetBindEmptyVisibility(DependencyObject element, ListViewBase value) =>
        element.SetValue(BindEmptyVisibilityProperty, value);

    public static ListViewBase GetBindEmptyVisibility(DependencyObject element) =>
        (ListViewBase)element.GetValue(BindEmptyVisibilityProperty);
    
    private static void OnBindEmptyVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement targetElement) return;
        if (e.NewValue is ListViewBase newList)
        {
            UpdateVisibility(targetElement, newList.Items);
            WeakEventListener<UIElement, IObservableVector<object>, IVectorChangedEventArgs> wel = new(targetElement)
            {
                OnEventAction = (instance, source, _) => { UpdateVisibility(instance, (source as ItemCollection)!); },
                OnDetachAction = l => newList.Items.VectorChanged -= l.OnEvent
            };
            newList.Items.VectorChanged += wel.OnEvent;
        }
    }

    private static void UpdateVisibility(UIElement element, ItemCollection list)
    {
        element.Visibility = list.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }
}