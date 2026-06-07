using Microsoft.UI.Xaml;

namespace Cirrus.Controls;

public sealed partial class SystemTrayIcon
{
    public App App { get; }

    public SystemTrayIcon()
    {
        App = (Application.Current as App)!;
        InitializeComponent();
    }
}
