using System.Collections;
using Cirrus.Commanding.Primitives;
using Cirrus.Extensions;
using Cirrus.Models.Abstract.Primitives;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Cirrus.Commanding;

public sealed partial class MultipleTargetNavigateCommand : CommandWrapper
{
    public MultipleTargetNavigateCommand()
    {
        InnerCommand = new RelayCommandEx<FrameworkElement, TappedRoutedEventArgs>(static (sender, args) =>
        {
            if (sender.DataContext is not IEnumerable enumerableContext ||
                enumerableContext.OfType<INavigatiable>().ToArray() is not { Length: > 0 } targets) return;
            if (targets is { Length: 1 })
            {
                NavigateCommand.Instance.Execute(targets[0]);
                return;
            }
            new MenuFlyout
            {
                MenuFlyoutPresenterStyle = new(typeof(MenuFlyoutPresenter))
                {
                    Setters =
                    {
                        new Setter(FrameworkElement.MinWidthProperty, 250d)
                    }
                }
            }.AddItems(targets.Select(static t =>
            {
                var text = t is INamedEntity ne ? ne.EntityName : "[UnknownName]";
                return new MenuFlyoutItem
                {
                    Text = text,
                    Command = NavigateCommand.Instance,
                    CommandParameter = t
                };
            }).OfType<MenuFlyoutItemBase?>().ToArray()).ShowAt(sender, args.GetPosition(sender));
        });
    }
}