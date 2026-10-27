using DynamicData.Binding;
using Cirrus.Models.Abstract.Primitives;

namespace Cirrus.LiveModels;

public partial class NavigatiableGroup(IEnumerable<INavigatiable> source) : ObservableCollectionExtended<INavigatiable>(source)
{
    public string GroupHeader
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged(new(nameof(GroupHeader)));
        }
    } = string.Empty;
}