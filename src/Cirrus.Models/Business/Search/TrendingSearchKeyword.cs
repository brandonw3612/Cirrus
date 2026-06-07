namespace Cirrus.Models.Business.Search;

public record TrendingSearchKeyword(
    string Keyword,
    string? ExtendedContent
)
{
    public bool HasExtendedContent => ExtendedContent is { Length: > 0 };
}