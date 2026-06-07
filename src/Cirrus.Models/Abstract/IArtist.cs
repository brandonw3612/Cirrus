using Cirrus.Models.Abstract.Primitives;

namespace Cirrus.Models.Abstract;

public interface IArtist : INavigatiable, ISharable, IPlayable, INamedEntity
{
    ulong ArtistId { get; }
    string Name { get; }
    string INamedEntity.EntityName => Name;
}