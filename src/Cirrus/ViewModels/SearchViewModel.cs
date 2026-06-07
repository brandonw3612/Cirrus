using System.Collections.ObjectModel;
using System.Text.Json;
using Windows.Storage;
using Cirrus.Models.Business.Search;
using Cirrus.Network;
using Cirrus.Primitives;
using Cirrus.Serialization;
using Cirrus.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using Pulse.Debouncers;

namespace Cirrus.ViewModels;

public sealed partial class SearchViewModel : ViewModel
{
    public override string ViewIdentifier => "Search";

    [ObservableProperty] public partial string SearchKeyword { get; set; } = string.Empty;

    private string _userInputKeyword = string.Empty;

    public ObservableCollection<TrendingSearchKeyword> TrendingSearchKeywords { get; } = new();
    public ObservableCollection<string> SearchHistoryKeywords { get; } = new();

    [ObservableProperty] public partial List<string> SearchSuggestions { get; private set; } = [];

    private readonly AsyncTaskDebouncer _suggestionQueryDebouncer;

    public SearchViewModel()
    {
        _suggestionQueryDebouncer = new()
        {
            TaskTimeout = TimeSpan.FromMicroseconds(500),
            Task = () => DispatcherQueue!.EnqueueAsync(async () =>
            {
                if (_userInputKeyword is { Length: > 0 })
                {
                    try
                    {
                        var response = await Client.Search.SuggestKeywordsAsync(_userInputKeyword);
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
            })
        };
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
        _userInputKeyword = SearchKeyword.Trim();
        _suggestionQueryDebouncer.Invoke();
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