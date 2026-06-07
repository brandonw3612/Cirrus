using Cirrus.Models.Abstract.Primitives;

namespace Cirrus.Models.Abstract;

public interface IQuery : INavigatiable
{
    string Keyword { get; }
}