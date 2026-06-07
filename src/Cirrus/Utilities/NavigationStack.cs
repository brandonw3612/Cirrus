using Cirrus.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Cirrus.Utilities;

public partial class NavigationStack : ObservableObject
{
    private readonly Dictionary<string, (Type ViewType, ViewModel ViewModel)> _pageContentDictionary = new();
    private readonly Stack<string> _pageIdentifierStack = new();

    public bool CanNavigateBack => _pageIdentifierStack is { Count: > 1 };

    public void Push(Type pageType, ViewModel viewModel)
    {
        if (viewModel.ViewIdentifier == string.Empty) return;
        var viewIdentifier = viewModel.ViewIdentifier;
        if (_pageIdentifierStack.Count == 0 || viewIdentifier != _pageIdentifierStack.Peek())
            _pageIdentifierStack.Push(viewIdentifier);
        _pageContentDictionary[viewIdentifier] = (pageType, viewModel);
        OnPropertyChanged(nameof(CanNavigateBack));
    }

    public (Type ViewType, ViewModel ViewModel)? Pop()
    {
        if (_pageIdentifierStack.Count == 0) return null;
        var identifier = _pageIdentifierStack.Pop();
        var result = _pageContentDictionary[identifier];
        if (!_pageIdentifierStack.Contains(identifier)) _pageContentDictionary.Remove(identifier);
        OnPropertyChanged(nameof(CanNavigateBack));
        return result;
    }

    public (Type ViewType, ViewModel ViewModel)? Peek()
    {
        if (_pageIdentifierStack.Count == 0) return null;
        var identifier = _pageIdentifierStack.Peek();
        return (_pageContentDictionary[identifier].ViewType, _pageContentDictionary[identifier].ViewModel);
    }
}