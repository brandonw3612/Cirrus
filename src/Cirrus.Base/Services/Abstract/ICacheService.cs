using Cirrus.Models.Business.Track;

namespace Cirrus.Base.Services.Abstract;

public interface ICacheService
{
    ValueTask InitializeAsync(CancellationToken cancellationToken = default);
    
    Task<Dictionary<ulong, OnlineTrack>> GetOnlineTracksAsync(
        IReadOnlyList<long> trackIds,
        CancellationToken cancellationToken = default
    );
    
    Task SaveOnlineTracksAsync(
        IReadOnlyList<OnlineTrack> tracks,
        CancellationToken cancellationToken = default
    );
}