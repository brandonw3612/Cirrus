using Cirrus.Primitives;
using Cirrus.ViewModels;

namespace Cirrus.Dialogs;

public abstract class NetworkProxyConfigDialogContentBase(NetworkProxyConfigDialogContentViewModel viewModel) :
    ModeledView<NetworkProxyConfigDialogContentViewModel>(viewModel);

public sealed partial class NetworkProxyConfigDialogContent
{
    public NetworkProxyConfigDialogContent() : base(new())
    {
        InitializeComponent();
    }
}