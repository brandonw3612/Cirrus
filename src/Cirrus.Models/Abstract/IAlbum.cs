using Cirrus.Models.Abstract.Primitives;

namespace Cirrus.Models.Abstract;

public interface IAlbum : INavigatiable, ISharable, IPlayable, INamedEntity
{
    ulong AlbumId { get; }
    string Title { get; }
    string INamedEntity.EntityName => Title;
}