using Cirrus.Models.Abstract.Primitives;

namespace Cirrus.Models.Abstract;

public interface ITopChart: INavigatiable, IPlayable
{
    ulong UserId { get; }
    string Nickname { get; }
    bool IsAccessible { get; }
}