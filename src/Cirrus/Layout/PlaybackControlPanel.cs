using Windows.Foundation;
using Cirrus.Models.Business.Playback;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Cirrus.Layout;

public partial class PlaybackControlPanel : Panel
{
    public double RightTranslationX
    {
        get => (double)GetValue(RightTranslationXProperty);
        set => SetValue(RightTranslationXProperty, value);
    }

    public static readonly DependencyProperty RightTranslationXProperty =
        DependencyProperty.Register(nameof(RightTranslationX), typeof(double), typeof(PlaybackControlPanel),
            new PropertyMetadata(0d));

    public UIElement? ArtworkControl
    {
        get => (UIElement?)GetValue(ArtworkControlProperty);
        set => SetValue(ArtworkControlProperty, value);
    }

    public static readonly DependencyProperty ArtworkControlProperty =
        DependencyProperty.Register(nameof(ArtworkControl), typeof(UIElement), typeof(PlaybackControlPanel),
            new PropertyMetadata(null, OnChildrenChanged));

    public UIElement? CoreControl
    {
        get => (UIElement?)GetValue(CoreControlProperty);
        set => SetValue(CoreControlProperty, value);
    }

    private static readonly DependencyProperty CoreControlProperty =
        DependencyProperty.Register(nameof(CoreControl), typeof(UIElement), typeof(PlaybackControlPanel),
            new PropertyMetadata(null, OnChildrenChanged));

    public UIElement? LyricsControl
    {
        get => (UIElement?)GetValue(LyricsControlProperty);
        set => SetValue(LyricsControlProperty, value);
    }

    private static readonly DependencyProperty LyricsControlProperty =
        DependencyProperty.Register(nameof(LyricsControl), typeof(UIElement), typeof(PlaybackControlPanel),
            new PropertyMetadata(null, OnChildrenChanged));

    public UIElement? QueueControl
    {
        get => (UIElement?)GetValue(QueueControlProperty);
        set => SetValue(QueueControlProperty, value);
    }

    private static readonly DependencyProperty QueueControlProperty =
        DependencyProperty.Register(nameof(QueueControl), typeof(UIElement), typeof(PlaybackControlPanel),
            new PropertyMetadata(null, OnChildrenChanged));

    public PlaybackControlViewMode ViewMode
    {
        get => (PlaybackControlViewMode)GetValue(ViewModeProperty);
        set => SetValue(ViewModeProperty, value);
    }

    private static readonly DependencyProperty ViewModeProperty =
        DependencyProperty.Register(nameof(ViewMode), typeof(PlaybackControlViewMode), typeof(PlaybackControlPanel),
            new PropertyMetadata(PlaybackControlViewMode.Cover, OnViewModeUpdated));

    private static void OnChildrenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PlaybackControlPanel panel) return;
        if (e.OldValue is UIElement oldElement) panel.Children.Remove(oldElement);
        if (e.NewValue is UIElement newElement) panel.Children.Add(newElement);
    }

    private static async void OnViewModeUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PlaybackControlPanel
            {
                LyricsControl: { } lyricsControl,
                QueueControl: { } queueControl
            } || e.NewValue is not PlaybackControlViewMode viewMode) return;
        lyricsControl.Visibility =
            viewMode is PlaybackControlViewMode.Lyrics ? Visibility.Visible : Visibility.Collapsed;
        queueControl.Visibility = viewMode is PlaybackControlViewMode.Queue ? Visibility.Visible : Visibility.Collapsed;
    }

    private const double HorizontalSpacing = 32d;
    private const double VerticalSpacing = 0d;

    protected override Size MeasureOverride(Size availableSize)
    {
        if (availableSize.Width <= 0d || availableSize.Height <= 0d)
            return base.MeasureOverride(availableSize);

        var availableWidth = availableSize.Width;
        var availableHeight = availableSize.Height;
        var shortSide = Math.Min(availableWidth, availableHeight);
        var maxCoverSize = shortSide < 800d ? 300d : Math.Floor(shortSide / 200d) * 100d;

        bool doubleColumn;
        double columnWidth1, columnWidth2;
        var singleColumnWidth = Math.Min(availableWidth, maxCoverSize);
        var doubleColumnWidth1 = Math.Min((availableWidth - HorizontalSpacing) * 0.5d, maxCoverSize);
        var doubleColumnWidth2 = Math.Min(availableWidth - HorizontalSpacing - doubleColumnWidth1, maxCoverSize * 1.5d);
        if (ViewMode is PlaybackControlViewMode.Cover)
        {
            doubleColumn = false;
            columnWidth1 = singleColumnWidth;
            columnWidth2 = 0d;
        }
        else
        {
            doubleColumn = true;
            columnWidth1 = doubleColumnWidth1;
            columnWidth2 = doubleColumnWidth2;
        }

        var finalWidth = columnWidth1 + columnWidth2;
        if (doubleColumn) finalWidth += HorizontalSpacing;

        RightTranslationX = 0.5d * (singleColumnWidth + doubleColumnWidth2 - doubleColumnWidth1);

        if (ArtworkControl is not { } artworkControl ||
            CoreControl is not { } coreControl ||
            LyricsControl is not { } lyricsControl ||
            QueueControl is not { } queueControl)
            return base.MeasureOverride(availableSize);

        coreControl.Measure(new(columnWidth1, availableHeight));
        var coverSize = Math.Min(columnWidth1, availableHeight - coreControl.DesiredSize.Height - VerticalSpacing);
        artworkControl.Measure(new(coverSize, coverSize));

        var finalHeight = coverSize + coreControl.DesiredSize.Height + VerticalSpacing;

        lyricsControl.Measure(new(doubleColumnWidth2, finalHeight));
        queueControl.Measure(new(doubleColumnWidth2, finalHeight));

        return new Size(finalWidth, finalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (ArtworkControl is not { } artworkControl ||
            CoreControl is not { } coreControl ||
            LyricsControl is not { } lyricsControl ||
            QueueControl is not { } queueControl)
            return base.ArrangeOverride(finalSize);

        var finalWidth = finalSize.Width;
        var finalHeight = finalSize.Height;
        var shortSide = Math.Min(finalWidth, finalHeight);
        var maxCoverSize = shortSide < 800d ? 300d : Math.Floor(shortSide / 200d) * 100d;

        double columnWidth1, columnWidth2;
        var singleColumnWidth = Math.Min(finalWidth, maxCoverSize);
        var doubleColumnWidth1 = Math.Min((finalWidth - HorizontalSpacing) * 0.5d, maxCoverSize);
        var doubleColumnWidth2 = Math.Min(finalWidth - HorizontalSpacing - doubleColumnWidth1, maxCoverSize * 1.5d);
        if (ViewMode is PlaybackControlViewMode.Cover)
        {
            columnWidth1 = singleColumnWidth;
            columnWidth2 = 0d;
        }
        else
        {
            columnWidth1 = doubleColumnWidth1;
            columnWidth2 = doubleColumnWidth2;
            if (ViewMode is PlaybackControlViewMode.Lyrics && lyricsControl.DesiredSize.Width == 0d) columnWidth2 = 0;
            if (ViewMode is PlaybackControlViewMode.Queue && queueControl.DesiredSize.Width == 0d) columnWidth2 = 0;
        }

        var totalWidth = columnWidth1 + columnWidth2;
        if (ViewMode is not PlaybackControlViewMode.Cover && columnWidth2 > 0d) totalWidth += HorizontalSpacing;
        var totalHeight = artworkControl.DesiredSize.Height + VerticalSpacing + coreControl.DesiredSize.Height;

        var originX = (finalSize.Width - totalWidth) / 2d;
        var originY = (finalSize.Height - totalHeight) / 2d;
        artworkControl.Arrange(new(
            new(originX + (columnWidth1 - artworkControl.DesiredSize.Width) / 2d, originY),
            artworkControl.DesiredSize
        ));
        coreControl.Arrange(new(
            new(originX, originY + artworkControl.DesiredSize.Height + VerticalSpacing),
            coreControl.DesiredSize
        ));
        lyricsControl.Arrange(new(
            originX + columnWidth1 + HorizontalSpacing, originY,
            columnWidth2, totalHeight
        ));
        queueControl.Arrange(new(
            originX + columnWidth1 + HorizontalSpacing, originY,
            columnWidth2, totalHeight
        ));
        return finalSize;
    }
}