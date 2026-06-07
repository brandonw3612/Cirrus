using Cirrus.Primitives;
using Cirrus.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Cirrus.Dialogs;

public abstract class LoginDialogContentBase(LoginDialogContentViewModel viewModel) :
    ModeledView<LoginDialogContentViewModel>(viewModel);

public sealed partial class LoginDialogContent
{
    public LoginDialogContent(ContentDialog parentDialog) : base(new(parentDialog))
    {
        InitializeComponent();
    }
}
