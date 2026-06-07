using Cirrus.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;

namespace Cirrus.Primitives;

public abstract partial class ViewModel : ObservableObject
{
    public abstract string ViewIdentifier { get; }

    public DispatcherQueue? DispatcherQueue { get; set; } = MainWindow.Current?.DispatcherQueue;

    [ObservableProperty] public partial bool IsLoading { get; set; } = false;
    
    public abstract Task LoadDataAsync();

    public virtual void AllocateDisposable()
    {
        // Do nothing.
    }
    
    public virtual void RecycleDisposable()
    {
        // Do nothing.
    }
}