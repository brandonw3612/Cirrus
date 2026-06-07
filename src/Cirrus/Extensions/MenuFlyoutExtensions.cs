using Microsoft.UI.Xaml.Controls;

namespace Cirrus.Extensions;

public static class MenuFlyoutExtensions
{
    public static MenuFlyout AddItems(this MenuFlyout flyout, MenuFlyoutItemBase?[] items)
    {
        foreach (var item in items)
        {
            if (item is null) continue;
            flyout.Items.Add(item);
        }
        return flyout;
    }

    public static MenuFlyoutSubItem AddItems(this MenuFlyoutSubItem subItem, MenuFlyoutItemBase?[] items)
    {
        foreach (var item in items)
        {
            if (item is null) continue;
            subItem.Items.Add(item);
        }
        return subItem;
    }
}