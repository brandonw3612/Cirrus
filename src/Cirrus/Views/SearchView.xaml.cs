using Cirrus.Primitives;
using Cirrus.ViewModels;

namespace Cirrus.Views;

public abstract class SearchViewBase : View<SearchViewModel>;

public sealed partial class SearchView
{
    public SearchView()
    {
        InitializeComponent();
    }
}
