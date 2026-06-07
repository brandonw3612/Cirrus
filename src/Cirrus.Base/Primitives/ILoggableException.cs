namespace Cirrus.Base.Primitives;

public interface ILoggableException
{
    (Exception Exception, Dictionary<string, string> Properties) GenerateLog();
}