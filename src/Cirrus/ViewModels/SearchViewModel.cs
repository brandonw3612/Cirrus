using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using Windows.Storage;
using Cirrus.Extensions;
using Cirrus.Models.Business.Search;
using Cirrus.Network;
using Cirrus.Primitives;
using Cirrus.Serialization;
using Cirrus.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace Cirrus.ViewModels;

public sealed partial class SearchViewModel : ViewModel
{
    public override string ViewIdentifier => "Search";

    [ObservableProperty] public partial string SearchKeyword { get; set; } = string.Empty;

    public ObservableCollection<TrendingSearchKeyword> TrendingSearchKeywords { get; } = new();
    public ObservableCollection<string> SearchHistoryKeywords { get; } = new();

    [ObservableProperty] public partial List<string> SearchSuggestions { get; private set; } = [];

    private Subject<string>? _suggestionQueryInput;
    private IDisposable? _suggestionQuerySubscription;

    public override void AllocateDisposable()
    {
        _suggestionQueryInput = new();
        _suggestionQuerySubscription = _suggestionQueryInput
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ObserveOn(DispatcherQueue!)
            .Subscribe(async keyword =>
            {
                if (keyword is { Length: > 0 })
                {
                    try
                    {
                        var response = await Client.Search.SuggestKeywordsAsync(keyword);
                        SearchSuggestions = response.Keywords;
                    }
                    catch
                    {
                        // Ignored.
                    }
                }
                else
                {
                    SearchSuggestions = [];
                }
            });
    }

    public override void RecycleDisposable()
    {
        _suggestionQuerySubscription?.Dispose();
        _suggestionQuerySubscription = null;
        _suggestionQueryInput?.Dispose();
        _suggestionQueryInput = null;
    }

    public override async Task LoadDataAsync()
    {
        SearchHistoryKeywords.Clear();
        (await LoadSearchHistoryCacheAsync()).ForEach(SearchHistoryKeywords.Add);

        TrendingSearchKeywords.Clear();
        var response = await Client.Search.GetTrendingKeywordsAsync();
        foreach (var entry in response.Entries)
        {
            TrendingSearchKeywords.Add(entry.ToBusinessModel());
        }
    }

    [RelayCommand]
    private void OnInputKeywordChanged(AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason is not AutoSuggestionBoxTextChangeReason.UserInput) return;
        _suggestionQueryInput?.OnNext(SearchKeyword.Trim());
    }

    [RelayCommand]
    private void ClearSearchHistory()
    {
        ClearSearchHistoryCache();
        SearchHistoryKeywords.Clear();
    }

    [RelayCommand]
    private void OnSearchSuggestionChosen(AutoSuggestBoxSuggestionChosenEventArgs args) =>
        SearchKeyword = args.SelectedItem.ToString()?.Trim() ?? string.Empty;

    [RelayCommand]
    private Task OnSearchQuerySubmitted() => NavigateToSearchResults(SearchKeyword);

    [RelayCommand]
    private async Task NavigateToSearchResults(string searchKeyword)
    {
        if (searchKeyword.Trim() is not { Length: > 0 } keyword) return;
        var updatedHistory = await LogKeywordToCacheAsync(keyword);
        SearchHistoryKeywords.Clear();
        updatedHistory.ForEach(SearchHistoryKeywords.Add);
        if (MainWindow.Current is { } window)
        {
            window.Navigate(typeof(SearchResultView), keyword);
        }
    }

    #region Local Cache Operations

    private static async Task<List<string>> LoadSearchHistoryCacheAsync()
    {
        try
        {
            var localFolder = ApplicationData.Current.LocalFolder.Path;
            var cacheFilePath = Path.Join(localFolder, "Cache", "SearchHistory.json");
            if (!File.Exists(cacheFilePath)) return [];
            var fileContent = await File.ReadAllTextAsync(cacheFilePath);
            return JsonSerializer.Deserialize<List<string>>(fileContent, AppSerializationContext.Default.ListString) ??
                   [];
        }
        catch
        {
            return [];
        }
    }

    private static async Task<List<string>> LogKeywordToCacheAsync(string keyword)
    {
        try
        {
            var localFolder = ApplicationData.Current.LocalFolder.Path;
            var cacheFolder = Path.Join(localFolder, "Cache");
            Directory.CreateDirectory(cacheFolder);
            var cacheFilePath = Path.Join(cacheFolder, "SearchHistory.json");
            var searchHistoryKeywords = await LoadSearchHistoryCacheAsync();
            if (searchHistoryKeywords.Contains(keyword)) searchHistoryKeywords.Remove(keyword);
            searchHistoryKeywords.Insert(0, keyword);
            var fileContent =
                JsonSerializer.Serialize(searchHistoryKeywords, AppSerializationContext.Default.ListString);
            await File.WriteAllTextAsync(cacheFilePath, fileContent);
            return searchHistoryKeywords;
        }
        catch
        {
            return new();
        }
    }

    private static void ClearSearchHistoryCache()
    {
        try
        {
            var localFolder = ApplicationData.Current.LocalFolder.Path;
            var cacheFilePath = Path.Join(localFolder, "Cache", "SearchHistory.json");
            if (!File.Exists(cacheFilePath)) return;
            File.Delete(cacheFilePath);
        }
        catch
        {
            // Ignored.
        }
    }

    #endregion
}