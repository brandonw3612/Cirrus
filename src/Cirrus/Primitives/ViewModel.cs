using Cirrus.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;

namespace Cirrus.Primitives;

public abstract partial class ViewModel : ObservableObject
{
    public abstract string ViewIdentifier { get; }

    public DispatcherQueue? DispatcherQueue { get; set; } = MainWindow.Current?.DispatcherQueue;

    [ObservableProperty] public partial bool IsLoading { get; set; } = false;

    public bool Recycled { get; private set; } = true;

    public abstract Task LoadDataAsync();

    protected virtual void AllocateDisposable()
    {
        // Do nothing.
    }
    public void TryAllocateDisposable()
    {
        if (Recycled)
        {
            AllocateDisposable();
            Recycled = false;
        }
    }

    protected virtual void RecycleDisposable()
    {
        // Do nothing.
    }

    public void TryRecycleDisposable()
    {
        if (!Recycled)
        {
            RecycleDisposable();
            Recycled = true;
        }
    }
}