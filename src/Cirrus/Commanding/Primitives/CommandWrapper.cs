using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Cirrus.Commanding.Primitives;

public partial class CommandWrapper : ObservableObject, ICommand
{
    [ObservableProperty] public partial IRelayCommand? InnerCommand { get; set; }

    partial void OnInnerCommandChanged(IRelayCommand? oldValue, IRelayCommand? newValue)
    {
        if (oldValue is not null) oldValue.CanExecuteChanged -= OnCanExecuteChanged;
        if (newValue is null) return;
        newValue.CanExecuteChanged -= OnCanExecuteChanged;
        newValue.CanExecuteChanged += OnCanExecuteChanged;
    }
    
    public bool CanExecute(object? parameter) => InnerCommand?.CanExecute(parameter) ?? false;
    
    public void Execute(object? parameter) => InnerCommand?.Execute(parameter);
    
    private void OnCanExecuteChanged(object? sender, EventArgs e) => CanExecuteChanged?.Invoke(this, e);

    public event EventHandler? CanExecuteChanged;
}