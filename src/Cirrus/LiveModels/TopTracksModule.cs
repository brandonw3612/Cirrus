using Cirrus.Models.Business.Track;
using Cirrus.Models.Network.Response.Search.GeneralSearchModules;

namespace Cirrus.LiveModels;

public class TopTracksModule : SearchModuleBase
{
    public List<OnlineTrack> Tracks { get; set; } = [];
}