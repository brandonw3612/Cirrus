using Cirrus.Models.Abstract.Primitives;

namespace Cirrus.Models.Abstract;

public interface IPlaylist : INavigatiable, IPlayable, ISharable, INamedEntity
{
    ulong PlaylistId { get; }
    string Title { get; }
    string INamedEntity.EntityName => Title;
}