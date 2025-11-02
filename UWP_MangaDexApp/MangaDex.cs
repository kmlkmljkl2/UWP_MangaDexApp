using MangaDexSharp;
using MangaDexSharp.Collections;
using MangaDexSharp.Extensions.Search;
using MangaDexSharp.Parameters;
using MangaDexSharp.Parameters.Chapter;
using MangaDexSharp.Parameters.Follows;
using MangaDexSharp.Parameters.Manga;
using MangaDexSharp.Parameters.Order;
using MangaDexSharp.Parameters.Order.Chapter;
using MangaDexSharp.Resources;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UWP_MangaDexApp.Settings;
using Windows.Storage;

namespace UWP_MangaDexApp
{
    public class MangaDex
    {
        public MangaDex()
        {
            Client = new MangaDexClient();
            SearchEngine = new MangaSearchEngine(Client);
            MangaFeed = new ObservableCollection<Manga>();
            FollowedFeed = new ObservableCollection<Manga>();
            SearchResult = new ObservableCollection<Manga>();
            SeasonalManga = new ObservableCollection<Manga>();
        }

        public MangaDexClient Client { get; }
        public ObservableCollection<Manga> FollowedFeed { get; }
        public int FollowedFeedOffset { get; set; }
        public ObservableCollection<Manga> MangaFeed { get; }
        public ObservableCollection<Manga> SearchResult { get; }
        public int MangaFeedOffset { get; set; }
        public MangaSearchEngine SearchEngine { get; set; }
        public ObservableCollection<Manga> SeasonalManga { get; }
        private Settings.Settings.SettingsClass Settings => MainWindow.SettingsP.GetSettings();

        public async Task LoadFollowedManga()
        {
            if (Client.IsLoggedIn)
            {
                var Chapters = await Client.Follows.GetFollowedMangaFeed(GetFollowedParams());
                if (Chapters.Count == 0)
                {
                    FollowedFeedOffset -= Settings.ChapterLoadAmount;
                    return;
                }
                await LoadManga(Chapters);

                foreach (Chapter i in Chapters)
                {
                    Manga man = await i.GetManga();
                    try
                    {
                        man.Cover = await man.GetMainCover();
                    }
                    catch { }

                    if (!FollowedFeed.Contains(man))
                    {
                        i.IsNew = true;
                        FollowedFeed.Add(man);
                    }
                }
            }
        }

        public async Task<ResourceCollection<Manga>> LoadManga(ResourceCollection<Chapter> Chapters = null, Guid[] MangaIds = null)
        {
            if (MangaIds == null)
                return await Client.Manga.GetList(GetMangaListParams(Chapters));
            else
                return await Client.Manga.GetList(GetMangaListParams(MangaIds));
        }

        public async Task LoadMangaFeed()
        {
            ResourceCollection<Chapter> chapters = await Client.Chapter.GetList(GetChapterParams());
            if (chapters.Count == 0)
            {
                MangaFeedOffset -= Settings.ChapterLoadAmount;
                return;
            }
            await LoadManga(chapters);

            foreach (var i in chapters)
            {
                Manga Manga = await i.GetManga();
                try
                {
                    Manga.Cover = await Manga.GetMainCover();
                }
                catch
                { }

                if (!MangaFeed.Contains(Manga))
                {
                    i.IsNew = true;
                    MangaFeed.Add(Manga);
                }
                // i.Cover = await i.GetMainCover();
            }
        }
        public async void LoadMore()
        {
            if (FollowedFeed.Count < 21)
            {
                await LoadFollowedManga();
            }
            if (MangaFeed.Count < 21)
            {
                await LoadMangaFeed();
            }
        }

        public async Task LoadSeasonal()
        {
            CustomList list;
            try
            {
                list = await Client.List.GetCustomList(new Guid("d434f5f1-ff90-4fa5-be7c-2c070da79120"));
            }
            catch
            {
                return;
            }

            var page = await list.GetTitles();
            foreach (var manga in page.CurrentPage)
            {
                try
                {
                    manga.Cover = await manga.GetMainCover();
                }
                catch
                { }

                if (!SeasonalManga.Contains(manga))
                {
                    SeasonalManga.Add(manga);
                }
                if (page.Page < page.TotalPages)
                    await page.NextPage();
            }
        }

        public async Task Refresh()
        {
            FollowedFeed.Clear();
            FollowedFeedOffset = 0;
            MangaFeed.Clear();
            MangaFeedOffset = 0;
            await LoadFollowedManga();
            await LoadMangaFeed();
            LoadMore();
        }
        private IPaginatedCollection<Manga> CurrentSearch = null;
        public async Task Search(string TitleName, MangaSearchOrder order = MangaSearchOrder.MostFollows)
        {
            SearchResult.Clear();
            ResetSearch();
            SearchEngine.ItemsPerPage = 10;
            SearchEngine.Title = TitleName;
            SearchEngine.SearchOrder = order;
            try
            {
                CurrentSearch = await SearchEngine.Search();
                foreach(var manga in CurrentSearch.CurrentPage)
                {
                    try
                    {
                        manga.Cover = await manga.GetMainCover();
                    }
                    catch
                    { }
                    SearchResult.Add(manga);
                }
            }
            catch
            {
                CurrentSearch = null;
            }

        }
        public async Task SearchMore()
        {
            if (CurrentSearch == null || CurrentSearch.TotalPages <= CurrentSearch.Page) return;
            await CurrentSearch.NextPage();
            foreach(var manga in CurrentSearch.CurrentPage)
            {
                try
                {
                    manga.Cover = await manga.GetMainCover();
                }
                catch
                { }
                SearchResult.Add(manga);
            }
        }
        public void ResetSearch()
        {
            CurrentSearch = null;
            SearchEngine.ResetFilters();
        }
        public async void Start(string Token)
        {
            await TryLogin(Token);

            Client.Settings.FetchFollowedManga = Settings.ChapterLoadAmount;
            Client.Settings.AddTranslatedLanguage("en");

            await LoadFollowedManga();
            await LoadMangaFeed();
            LoadMore();
            await LoadSeasonal();
        }

        public async Task TryLogin(string Token)
        {
            if (Token != null)
            {
                try
                {
                    ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;
                    UserCredentials user = new UserCredentials(LocalSettings.Values[LocalData.UserName] as string, "", LocalSettings.Values[LocalData.ClientId] as string, LocalSettings.Values[LocalData.ClientSecret] as string);
                    MainWindow.Dex.Client.SetUserCredentials(user);
                    await Client.Auth.LoginWithToken(Token);
                    MainWindow.MainPage.SaveSettings(Token: Client.Auth.LastRefreshToken);
                }
                catch (Exception ex)
                {
                    MainWindow.MainPage.ResetToken();
                }
            }
        }

        private GetChapterListParameters GetChapterParams()
        {
            var orderParameters = new GetChapterListOrderParameters();
            orderParameters.OrderByDescending(x => x.CreatedAt);

            var chapterParameters = new GetChapterListParameters(orderParameters)
            {
                Includes = new IncludeParameters()
                {
                    //IncludeManga = true,
                    IncludeScanlationGroup = true,
                    //IncludeCover = true
                },
                Amount = Settings.ChapterLoadAmount,
                Position = MangaFeedOffset
            };
            MangaFeedOffset += Settings.ChapterLoadAmount;
            chapterParameters.ApplySettings(Client.Settings);
            return chapterParameters;
        }

        private GetFollowedMangaFeedParameters GetFollowedParams()
        {
            var orderParameters = new GetChapterListOrderParameters();
            orderParameters.OrderByDescending(x => x.CreatedAt);

            var chapterParameters = new GetFollowedMangaFeedParameters(orderParameters)
            {
                Includes = new IncludeParameters()
                {
                    // IncludeManga = true,
                    IncludeScanlationGroup = true,
                    // IncludeCover = true
                },
                Amount = Settings.ChapterLoadAmount,
                Position = FollowedFeedOffset,
            };
            FollowedFeedOffset += Settings.ChapterLoadAmount;
            chapterParameters.ApplySettings(Client.Settings);
            return chapterParameters;
        }

        private GetMangaListParameters GetMangaListParams(ResourceCollection<Chapter> chapters)
        {
            GetMangaListParameters para = new GetMangaListParameters()
            {
                Includes = new IncludeParameters
                {
                    IncludeCover = true,
                    IncludeScanlationGroup = true
                }
            };
            para.MangaIds = chapters.Select(x => x.MangaId).Distinct().ToList();
            para.Amount = para.MangaIds.Count;
            return para;
        }

        private GetMangaListParameters GetMangaListParams(List<Chapter> chapters)
        {
            GetMangaListParameters para = new GetMangaListParameters()
            {
                Includes = new IncludeParameters
                {
                    IncludeCover = true,
                    IncludeScanlationGroup = true
                }
            };
            para.MangaIds = chapters.Select(x => x.MangaId).Distinct().ToList();
            para.Amount = para.MangaIds.Count;
            return para;
        }

        private GetMangaListParameters GetMangaListParams(Guid[] chapters)
        {
            GetMangaListParameters para = new GetMangaListParameters()
            {
                Includes = new IncludeParameters
                {
                    IncludeCover = true,
                    IncludeScanlationGroup = true
                }
            };
            para.MangaIds = chapters.ToList();
            para.Amount = para.MangaIds.Count;
            return para;
        }
    }
}