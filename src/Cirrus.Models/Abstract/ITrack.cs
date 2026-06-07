using Cirrus.Models.Abstract.Primitives;

namespace Cirrus.Models.Abstract;

public interface ITrack : ISharable, INamedEntity
{
    ulong TrackId { get; }
    string Title { get; }
    string INamedEntity.EntityName => Title;
}