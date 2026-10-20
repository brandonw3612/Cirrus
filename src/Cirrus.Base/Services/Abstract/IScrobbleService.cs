using Cirrus.Models.Network.Track;

namespace Cirrus.Base.Services.Abstract;

public interface IScrobbleService
{
    Task RefreshStatusAsync(TrackBrief track, CancellationToken cancellationToken = default);
    
    Task ScrobbleAsync(TrackBrief track, DateTimeOffset startedAt, CancellationToken cancellationToken = default);
}