using System.Text.RegularExpressions;
using Cirrus.Models.Business.Lyrics;
using Cirrus.Models.Network.Response.Track;

namespace Cirrus.Extensions;

public static class LyricsExtensions
{
    private record ParsedLine(TimeSpan LineStart, string Content);
    private record ParsingResult(List<ParsedLine> Parsed, List<string> Unparsed);
    private record B2BLyricLine(TimeSpan Start, TimeSpan Duration, List<LyricFragment> Fragments);
    
    public static TrackLyrics ToLocal(this LyricsApiResponse resp, TimeSpan trackDuration)
    {
        TrackLyrics local = new()
        {
            SyncedLyricsContributor = resp.SyncedLyricsContributor,
            TranslationContributor = resp.TranslationContributor
        };
        try
        {
            var rawParsed = ParseLrc(resp.OriginalLyrics?.Content ?? string.Empty);
            var pieces = rawParsed.Parsed
                .Where(l => l.Content.Trim() != string.Empty)
                .Select(l => new LyricLine
                {
                    Start = l.LineStart,
                    FallbackText = l.Content
                }).ToList();
            pieces.Sort();

            // Fill in duration
            for (var i = 0; i < pieces.Count; i++)
            {
                pieces[i].Duration = (i < pieces.Count - 1 ? pieces[i + 1].Start : trackDuration) - pieces[i].Start;
            }

            // Unparsed as additional content
            if (rawParsed.Unparsed.Count > 0)
            {
                // TODO: Rich content implementation
                local.FallbackAdditionalContent = string.Join('\n', rawParsed.Unparsed);
            }

            // Fill in translation
            if (pieces.Count > 0 &&
                resp.TranslatedLyrics?.Content is { Length: > 0 } trans)
            {
                var transParsed = ParseLrc(trans);
                foreach (var parsedLine in transParsed.Parsed)
                {
                    var match = pieces.FirstOrDefault(p => p.Start == parsedLine.LineStart);
                    if (match is { } mergedPiece &&
                        mergedPiece.FallbackText != parsedLine.Content)
                    {
                        mergedPiece.Translation = parsedLine.Content;
                    }
                }
            }
            
            // Fill in beat-to-beat lyrics
            if (pieces.Count > 0 &&
                resp.B2BLyrics?.Content is { Length: > 0 } lyrics)
            {
                var b2BLines = ParseB2B(lyrics).OrderBy(l => l.Start).ToList();
                var lastMatch = -1;
                foreach (var line in b2BLines)
                {
                    var initial = pieces.FindIndex(
                        lastMatch + 1,
                        p => B2BMatchByTime(p, line)
                    );
                    if (initial == -1) continue;
                    var trailing = pieces.FindIndex(
                        initial,
                        p => !B2BMatchByTime(p, line)
                    );
                    if (trailing == -1) trailing = pieces.Count;
                    int iterator;
                    for (iterator = initial; iterator < trailing; iterator++)
                    {
                        if (B2BMatchByContent(pieces[iterator], line)) break;
                    }
                    if (iterator < trailing) lastMatch = iterator;
                }
            }

            local.Lines = pieces;
            return local;
        }
        catch
        {
            return local;
        }
    }

    private static ParsingResult ParseLrc(string lrc)
    {
        if (lrc.Length == 0) return new([], []);
        List<ParsedLine> parsed = [];
        List<string> unparsed = [];
        Regex formatReg = new(@"\[[0-9,:,\.]*\]", RegexOptions.Compiled);
        Regex timestampReg = new(@"^\d+:\d+([:.]\d+)?$", RegexOptions.Compiled);
        var lines = lrc.Split('\n');
        foreach (var line in lines)
        {
            if (formatReg.Matches(line) is { Count: > 0 } matches)
            {
                var lyric = formatReg.Replace(line, string.Empty).Trim();
                foreach (Match match in matches)
                {
                    var timestamp = match.Value[1..^1];
                    if (timestampReg.IsMatch(timestamp))
                    {
                        parsed.Add(new(ParseTimeSpan(timestamp), lyric));
                    }
                }
            }
            else
            {
                unparsed.Add(line);
            }
        }
        return new(parsed, unparsed);
    }

    private static List<B2BLyricLine> ParseB2B(string lrc)
    {
        List<B2BLyricLine> result = [];
        if (lrc.Length == 0) return result;
        
        Regex lineMarkRegex = new(@"^\[\d+,\d+\]", RegexOptions.Compiled);
        Regex fragmentMarkRegex = new(@"\(\d+,\d+,\d+\)", RegexOptions.Compiled);
        
        var lines = lrc.Split('\n');
        
        foreach (var line in lines)
        {
            var lineMarkMatch = lineMarkRegex.Match(line);
            if (!lineMarkMatch.Success) continue;
            var lineTimeInfo = lineMarkMatch.Value[1..^1].Split(',');
            var lineStart = TimeSpan.FromMilliseconds(double.Parse(lineTimeInfo[0]));
            var lineDuration = TimeSpan.FromMilliseconds(double.Parse(lineTimeInfo[1]));
            var fragsStr = lineMarkRegex.Replace(line, string.Empty).Trim();
            
            List<LyricFragment> fragments = [];
            var fragmentMatches = fragmentMarkRegex.Matches(fragsStr);
            for (var i = 0; i < fragmentMatches.Count; i++)
            {
                var marks = fragmentMatches[i].Value[1..^1].Split(',');
                var fragStart = TimeSpan.FromMilliseconds(double.Parse(marks[0]));
                var fragDuration = TimeSpan.FromMilliseconds(double.Parse(marks[1]));
                var fragTextStart = fragmentMatches[i].Index + fragmentMatches[i].Length;
                var fragTextEnd = i < fragmentMatches.Count - 1 ? fragmentMatches[i + 1].Index : fragsStr.Length;
                var fragText = fragsStr[fragTextStart..fragTextEnd];
                fragments.Add(new()
                {
                    Start = fragStart,
                    Duration = fragDuration,
                    Text = fragText
                });
            }

            if (fragments.Count > 0)
            {
                result.Add(new(lineStart, lineDuration, fragments));
            }
        }

        return result;
    }

    private static bool B2BMatchByTime(LyricLine line, B2BLyricLine b2BLine)
    {
        var start = Math.Max(line.Start.TotalMilliseconds, b2BLine.Start.TotalMilliseconds);
        var end = Math.Min((line.Start + line.Duration).TotalMilliseconds,
            (b2BLine.Start + b2BLine.Duration).TotalMilliseconds);
        var range = end - start;
        return range / line.Duration.TotalMilliseconds >= 0.5 ||
               range / b2BLine.Duration.TotalMilliseconds >= 0.5;
    }

    private static bool B2BMatchByContent(LyricLine line, B2BLyricLine b2BLine)
    {
        var lineText = line.FallbackText;
        List<LyricFragment> corrected = [];
        var empty = 0;
        foreach (var f in b2BLine.Fragments)
        {
            if (f.Text.Trim() == string.Empty)
            {
                empty++;
                continue;
            }

            if (!ApproximatelyEquals(
                    lineText.Trim(), f.Text.Trim(), 3,
                    out var matchPosition, out var matchLength
                ))
                continue;
            if (matchPosition > 0)
            {
                if (corrected.LastOrDefault() is { } last)
                {
                    last.Text += lineText[..matchPosition];
                }
                else
                {
                    matchLength += matchPosition;
                    matchPosition = 0;
                }
            }
            corrected.Add(new()
            {
                Start = f.Start,
                Duration = f.Duration,
                Text = lineText.Substring(matchPosition, matchLength)
            });
            lineText = lineText.Substring(matchPosition + matchLength);
        }
        if (corrected.Count + empty < b2BLine.Fragments.Count * 0.75d)
            return false;
        if (lineText.Length > 0)
        {
            corrected.Last().Text += lineText;
        }
        line.Start = TimeSpan.FromMilliseconds(Math.Min(
            line.Start.TotalMilliseconds,
            corrected.First().Start.TotalMilliseconds
        ));
        line.Duration = corrected.Last().Start + corrected.Last().Duration - line.Start;
        line.B2BLyrics = corrected;
        return true;
    }

    private static bool ApproximatelyEquals(string main, string pattern, int tolerance, out int position, out int length)
    {
        position = length = 0;
        var error = 0;
        int mainPos = -1, tempMainPos = main.IndexOf(pattern[0]);
        if (tempMainPos == -1) return false;
        foreach (var t in pattern)
        {
            while (tempMainPos < main.Length &&
                   !main[tempMainPos].ToString().Equals(t.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                tempMainPos++;
                error++;
                if (error > tolerance) return false;
            }
            if (tempMainPos == main.Length) return false;
            if (mainPos == -1) mainPos = tempMainPos;
            tempMainPos++;
        }
        position = mainPos;
        length = tempMainPos - mainPos;
        return true;
    }

    private static TimeSpan ParseTimeSpan(string str)
    {
        var colons = str.Count(':');
        if (colons > 2)
        {
            var lastColon = str.LastIndexOf(':');
            str = str[..lastColon] + '.' + str[(lastColon + 1)..];
        }
        var segments = str.Split(':');
        var timeBase = 1d;
        var result = TimeSpan.Zero;
        for (var i = segments.Length - 1; i >= 0; i--)
        {
            result += TimeSpan.FromSeconds(timeBase * double.Parse(segments[i]));
            timeBase *= 60;
        }
        return result;
    }

    public static bool MatchPosition(this IList<LyricLine> lines, TimeSpan pos, out float[] matches)
    {
        if (lines.Count == 0)
        {
            matches = [-0.5f];
            return false;
        }
        
        List<float> res = new();
        for (var i = 0; i < lines.Count; i++)
        {
            if (pos >= lines[i].Start && pos < lines[i].Start + lines[i].Duration)
            {
                res.Add(i);
            }
        }

        if (res.Count == 0)
        {
            for (var i = 0; i < lines.Count; i++)
            {
                if (pos >= lines[i].Start) continue;
                matches = [i - 0.5f];
                return false;
            }
            matches = [lines.Count - 0.5f];
            return false;
        }
        
        matches = res.ToArray();
        return true;
    }
}