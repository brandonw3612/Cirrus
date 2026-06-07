namespace Cirrus.Models.Business.Track;

public class TrackPermission
{
    public required bool IsMembershipRequired { get; init; }
    public required bool IsPurchaseRequired { get; init; }
    public required bool IsFromCloudDrive { get; init; }
    public required bool CopyrightUnavailable { get; init; }
    public required bool IsPurchased { get; init; }
}