using Cirrus.Controls;
using Cirrus.Extensions;
using Cirrus.Models.Business.Lyrics;
using Cirrus.Primitives;
using Cirrus.ViewModels;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Cirrus.Views;

public abstract class PlaybackControlViewBase : View<PlaybackControlViewModel>;

public sealed partial class PlaybackControlView
{    
    public PlaybackControlView()
    {
        InitializeComponent();
        ViewModel.RenderLyrics = RenderLyrics;
        ViewModel.SyncLyrics = SyncLyrics;
        ViewModel.ToggleLyricsPreview = ToggleLyricsPreview;
    }

    private void RenderLyrics(TrackLyrics? lyrics)
    {
        LyricsScroller.ClearItems();
        if (lyrics is null) return;
        foreach (var line in lyrics.Lines)
        {
            var control = new LyricLineControl(line, ViewModel.PlaybackServiceBridge);
            GlidyScroller.SetScaleFactor(control, 1.03d);
            control.SetBinding(LyricLineControl.IsTranslationEnabledProperty, new Binding
            {
                Source = ViewModel,
                Path = new PropertyPath(nameof(ViewModel.IsLyricsTranslationEnabled)),
                Mode = BindingMode.OneWay
            });
            control.SetBinding(LyricLineControl.BaseFontSizeProperty, new Binding
            {
                Source = ViewModel,
                Path = new PropertyPath(nameof(ViewModel.LyricsBaseFontSize)),
                Mode = BindingMode.OneWay
            });
            LyricsScroller.AppendItem(control);
        }
    }
    
    private void SyncLyrics(TimeSpan current, TimeSpan total)
    {
        if (ViewModel.CurrentTrackLyrics is not { } lyrics ||
            LyricsScroller.ContentLength <= 0f ||
            LyricsScroller.Items.Count != lyrics.Lines.Count) return;
        var matched = lyrics.Lines.MatchPosition(current, out var highlightedLines);
        var newOffset = LyricsScroller.GetElementOffset(highlightedLines[0]);
        var below = Math.Min(LyricsScroller.ContentLength - newOffset, LyricsScroller.ActualHeight * 0.618);
        var above = LyricsScroller.ActualHeight - below;

        float min = highlightedLines.Min(), max = highlightedLines.Max();
        for (var i = 0; i < lyrics.Lines.Count; i++)
        {
            double opacity;
            if (i >= min && i <= max) opacity = 1d;
            else
            {
                var x = i < min
                    ? (newOffset - LyricsScroller.GetElementOffset(i + 1)) / above
                    : (LyricsScroller.GetElementOffset(i) - newOffset) / below;
                x = Math.Clamp(x, 0, 1);
                opacity = 0.7 * (1.0 - x) * (1.0 - x) + 0.1;
            }
            if (LyricsScroller.Items[i] is not LyricLineControl llc) continue;
            llc.StaticClarity = opacity;
            llc.IsScaled = matched && highlightedLines.Any(idx => (int)idx == i);
        }
        
        if (!ViewModel.LyricsScrollerOccupied)
        {
            LyricsScroller.HighlightedChildIndex = (int)Math.Ceiling(min);
        }

        if (!matched) return;
        foreach (var line in highlightedLines)
        {
            (LyricsScroller.Items[(int)line] as LyricLineControl)?.PlaybackPosition = current.TotalMilliseconds;
        }
    }

    private void ToggleLyricsPreview(bool preview)
    {
        foreach (var child in LyricsScroller.Items.OfType<LyricLineControl>())
        {
            child.IsPreviewed = preview;
        }
    }

    [RelayCommand]
    private void ToggleViewOff()
    {
        if (MainWindow.Current is not { } window) return;
        window.TogglePlaybackDrawer(false);
    }
}
