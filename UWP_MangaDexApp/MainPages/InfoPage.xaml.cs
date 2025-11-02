using MangaDexSharp.Collections;
using MangaDexSharp.Enums;
using MangaDexSharp.Resources;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UWP_MangaDexApp.MainPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InfoPage : Page
    {
        private readonly List<MangaReadingStatus> MangaReadings = new()
        {
            MangaReadingStatus.Completed,
            MangaReadingStatus.Dropped,
            MangaReadingStatus.OnHold,
            MangaReadingStatus.PlanToRead,
            MangaReadingStatus.Reading,
            MangaReadingStatus.ReReading,
            MangaReadingStatus.Nothing
        };

        public InfoPage()
        {
            this.InitializeComponent();
            Loaded += InfoPage_Loaded;
        }

        private void InfoPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentManga != null)
                MainWindow.MainPage.SetAppName(CurrentManga.Title.EnglishOrDefault);
            NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Required;
        }

        public bool CameFromSettings { get; set; }
        private IReadOnlyCollection<Guid> AlreadyReadChapters { get; set; }
        private ObservableCollection<Chapter> ChapterList { get; } = new ObservableCollection<Chapter>();
        private Manga CurrentManga { get; set; }
        private IFixedPaginatedCollection<Chapter> CurrentMangaChapters { get; set; }
        private MangaReadingStatus? CurrentMangaReadStatus { get; set; }
        private bool LoadingChapter { get; set; }
        private static bool LoggedIn => MainWindow.Dex.Client.IsLoggedIn;
        private Chapter SelectedbyFlyoutItem { get; set; }
        private Tests.FakeClasses.ObservableString Status { get; } = new Tests.FakeClasses.ObservableString();
        private Tests.FakeClasses.ObservableString Tags { get; } = new Tests.FakeClasses.ObservableString();

        public async void List_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem == null) return;
            var Chapter = e.ClickedItem as Chapter;
            if (Chapter.IsExternal || Chapter == null)
            {
                //Project Renuion doesnt support it yet

                //MessageDialog message = new MessageDialog("No Chapters on MangaDex :lelebcry:")
                //{
                //    Title = "Error"
                //};
                //await message.ShowAsync();
                return;
            }

            MainWindow.MainPage.GoToPage(ETC.Pages.ReadPage);
            await NextChapter(Chapter);
        }

        public async Task NextChapter(Chapter chapter)
        {
            MainWindow.ReadPage.ClearItems();
            MainWindow.MainPage.SetAppName($"{(CurrentManga.Title.EnglishOrDefault.Length > 60 ? CurrentManga.Title.EnglishOrDefault.Remove(57) + "..." : CurrentManga.Title.EnglishOrDefault)}  Chapter: {chapter.ChapterName}");
            chapter.Opacity.Data = 0.5;
            using (MangaDexSharp.Objects.ChapterReadSession session = await chapter.StartReadingSession(false))
            {
                for (int i = 0; session.TotalPages > i; i++)
                {
                    Uri Image = session.CurrentPage.ImageUrl;
                    MainWindow.ReadPage.AddImage(Image);

                    session.NextPage();
                }
            }
        }

        public async void SetContent(Manga manga)
        {
            if (manga.Cover != null)
                Cover.Source = new BitmapImage(manga.Cover.Thumbnail512Px) { DecodePixelWidth = (int)Cover.ActualWidth };
            MainWindow.MainPage.SetAppName(manga.Title.EnglishOrDefault);
            CurrentManga = manga;
            Description.Text = manga.Description.EnglishOrDefault.Trim() == "" ? "*No Description*" : manga.Description.EnglishOrDefault;
            Title.Text = manga.Title.EnglishOrDefault; // Title should always be there i hope
            Status.Data = manga.Status.ToString();
            MangaReadingStatus? readingstatus = null;
            if (LoggedIn)
            {
                AlreadyReadChapters = await manga.GetReadChaptersIds();
                readingstatus = await manga.GetReadingStatus();
            }
            if (readingstatus != null)
            {
                CurrentMangaReadStatus = readingstatus;
                StatusComboBox.SelectedItem = StatusComboBox.Items.FirstOrDefault(x => (MangaReadingStatus)x == readingstatus);
            }
            else
            {
                StatusComboBox.SelectedIndex = StatusComboBox.Items.Count - 1;
            }
            //for the odd case that the Manga has no Chapter
            try
            {
                CurrentMangaChapters = await manga.GetFeed(MangaDexSharp.Parameters.Enums.OrderByType.Descending);
            }
            catch
            {
                return;
            }
            foreach (Chapter i in CurrentMangaChapters.CurrentPage)
            {
                IReadOnlyCollection<ScanlationGroup> group = await i.GetGroups();
                if (group.Count > 0)
                {
                    i.scanlationGroup = (List<ScanlationGroup>)group;
                }

                if (AlreadyReadChapters != null && AlreadyReadChapters.Contains(i.Id))
                {
                    i.Opacity.Data = 0.5;
                }
                ChapterList.Add(i);
            }

            Tags.Data = TagsToString(manga.Tags);
            GC.Collect();
        }

        private void BackBTN_Click(object sender, RoutedEventArgs e)
        {
            if (CameFromSettings)
            {
                MainWindow.MainPage.GoToSettings();
                CameFromSettings = false;
            }
            else
            {
                MainWindow.MainPage.GoToPage(ETC.Pages.StartPage);
            }
            MainWindow.MainPage.SetAppName();
            Cover.Source = null;
            ChapterList.Clear();
            TagArea.Text = "";
            CurrentMangaChapters = null;
            AlreadyReadChapters = null;
            GC.Collect();
        }

        private async void ChapterScrollView_PointerWheelChanged(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (LoadingChapter) return;
            var estimate = ChapterScrollView.ScrollableHeight * 0.9;
            LoadingChapter = true;
            try
            {
                if (ChapterScrollView.VerticalOffset > estimate && CurrentMangaChapters.Page < CurrentMangaChapters.TotalPages)
                {
                    await CurrentMangaChapters.NextPage();
                    foreach (Chapter i in CurrentMangaChapters.CurrentPage)
                    {
                        IReadOnlyCollection<ScanlationGroup> group = await i.GetGroups();
                        if (group.Count > 0)
                        {
                            i.scanlationGroup = (List<ScanlationGroup>)group;
                        }

                        if (AlreadyReadChapters.Contains(i.Id))
                        {
                            i.Opacity.Data = 0.5;
                        }
                        ChapterList.Add(i);
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                string s = "";
#endif
            }
            LoadingChapter = false;
        }

        private void List_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            ListView listView = (ListView)sender;
            SelectedbyFlyoutItem = ((FrameworkElement)e.OriginalSource).DataContext as Chapter;
            ListViewMenuFlyout.ShowAt(listView, e.GetPosition(listView));
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (!LoggedIn) return;

            if (sender.GetType() == typeof(MenuFlyoutItem))
            {
                var mfi = (MenuFlyoutItem)sender;
                switch (mfi.Tag)
                {
                    case "read":
                        await SelectedbyFlyoutItem.MarkRead();
                        break;

                    case "unread":
                        await SelectedbyFlyoutItem.MarkUnread();
                        break;
                }
            }
            // string test2 = "";
        }

        private async void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!LoggedIn) return;
            ComboBox box = (ComboBox)sender;
            if (box.SelectedItem != null && (MangaReadingStatus)box.SelectedItem != CurrentMangaReadStatus /*old readingstatus*/)
            {
                MangaReadingStatus status = (MangaReadingStatus)box.SelectedItem;
                CurrentMangaReadStatus = status;
                await CurrentManga.Unfollow();
                if (status == MangaReadingStatus.Nothing)
                {
                    await CurrentManga.UpdateReadingStatus(null);
                    return;
                }
                if (status == MangaReadingStatus.Reading)
                {
                    await CurrentManga.Follow();
                }
                await CurrentManga.UpdateReadingStatus(status);
            }
        }

        private string TagsToString(IReadOnlyCollection<Tag> tags)
        {
            if (tags == null) return "";
            string ta = "";
            foreach (var i in tags)
            {
                ta += i.Name + ",  ";
            }
            ta = ta.TrimEnd();
            if(ta.Length > 0)
            {
                ta = ta.Remove(ta.Length - 1);
            }
            return ta;
        }
    }
}