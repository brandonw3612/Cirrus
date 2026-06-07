using Cirrus.Models.Abstract.Primitives;

namespace Cirrus.Models.Abstract;

public interface IUser : INavigatiable, ISharable, INamedEntity
{
    ulong UserId { get; }
    string Nickname { get; }
    string INamedEntity.EntityName => Nickname;
}