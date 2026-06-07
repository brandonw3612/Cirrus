using System.Collections.ObjectModel;
using Cirrus.LiveModels;
using Cirrus.Models.Business.Playback;
using Cirrus.Network;
using Cirrus.Primitives;

namespace Cirrus.ViewModels;

public partial class CloudDriveViewModel : ViewModel
{
    public ObservableCollection<IndexedOnlineTrack> Tracks { get; } = new();
    public List<IAudioTrack<ulong>> PlayableTracks { get; } = new();
    
    public override string ViewIdentifier => "CloudDrive";
    public override async Task LoadDataAsync()
    {
        Tracks.Clear();
        var tracksAdded = 0;
        var index = 1;
        while (true)
        {
            var tracksResponse = await Client.Account.GetCloudTracksAsync(tracksAdded, 500);
            if (tracksResponse.Tracks is not { } tracks) break;
            foreach (var track in tracks)
            {
                if (track.Track is not { } t) continue;
                var local = t.ToBusinessModel();
                Tracks.Add(new()
                {
                    Track = local,
                    Index = index++.ToString()
                });
                PlayableTracks.Add(local);
            }
            tracksAdded += tracks.Count;
            if (!tracksResponse.HasMore) break;
        }
    }
}