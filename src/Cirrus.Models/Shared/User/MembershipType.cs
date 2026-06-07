namespace Cirrus.Models.Shared.User;

[Flags]
public enum MembershipType
{
    None = 0b000,
    Package = 0b001,
    Premium = 0b011,
    PremiumPlus = 0b111
}

public static class MembershipTypeExtensions
{
    public static bool HasAccessToPremiumTracks(this MembershipType value)
    {
        return ((int)value & 0b001) != 0;
    }
    
    public static bool HasAccessToLosslessAudio(this MembershipType value)
    {
        return ((int)value & 0b010) != 0;
    }
    
    public static bool HasAccessToMasterAudio(this MembershipType value)
    {
        return ((int)value & 0b100) != 0;
    }
}