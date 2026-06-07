using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace Cirrus.Primitives;

public abstract class ModeledView<TViewModel>(TViewModel viewModel) : Page
    where TViewModel : ObservableObject
{
    public TViewModel ViewModel { get; } = viewModel;
}