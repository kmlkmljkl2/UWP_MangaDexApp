using MangaDexSharp.Enums;
using MangaDexSharp.Resources;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UWP_MangaDexApp.Tests;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UWP_MangaDexApp.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MangaPage : Page
    {
        private bool GrabbingMangaFeed { get; set; } = false;

        public MangaPage()
        {
            this.InitializeComponent();
            Loaded += MangaPage_Loaded;
            Unloaded += MangaPage_Unloaded;
        }

        private FakeClasses.ObservableString CounterAndLoginStatus { get; } = new FakeClasses.ObservableString();
        private int CurrentIndex { get; set; } = 0;
        private ObservableCollection<Manga> List { get; } = new ObservableCollection<Manga>();
        private bool LoggedIn => MainWindow.Dex.Client.IsLoggedIn;
        private MangaDex Mangadexx => MainWindow.Dex;
        private Dictionary<Guid, MangaReadingStatus> MoreMangas { get; set; } = new Dictionary<Guid, MangaReadingStatus>();

        private List<MangaReadingStatus> ReadingStatuses { get; } = new List<MangaReadingStatus>()
        {
            MangaReadingStatus.Reading,
            MangaReadingStatus.PlanToRead,
            MangaReadingStatus.Completed,
            MangaReadingStatus.OnHold,
            MangaReadingStatus.ReReading,
            MangaReadingStatus.Dropped
        };

        private Dictionary<Guid, MangaReadingStatus> StatusChache { get; set; } = new Dictionary<Guid, MangaReadingStatus>();

        public void Clear()
        {
            List.Clear();
            CounterAndLoginStatus.Data = "";
            StatusChache.Clear();
        }

        private async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = (ComboBox)sender;
            if (box.SelectedItem == null || !LoggedIn) return;
            List.Clear();
            Dictionary<Guid, MangaReadingStatus> RequestedTag = StatusChache.Where(x => x.Value == (MangaReadingStatus)box.SelectedItem).ToDictionary(x => x.Key, x => x.Value);
            if (RequestedTag.Count == 0) return;
            if (RequestedTag.Count > 100)
            {
                MoreMangas = RequestedTag;
            }
            else
            {
                MoreMangas.Clear();
            }
            CurrentIndex = 0;
            if (RequestedTag == null)
            {
                CounterAndLoginStatus.Data = "No Manga";
                return;
            }
            CounterAndLoginStatus.Data = $"{RequestedTag.Count} Manga";
            var Mangas = await Mangadexx.LoadManga(MangaIds: RequestedTag.Keys.Skip(CurrentIndex).Take(100).ToArray());
            CurrentIndex += 100;
            foreach (var i in Mangas)
            {
                i.Cover = await i.GetMainCover();
                List.Add(i);
            }
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem == null) return;
            MainWindow.MainPage.GoToPage(ETC.Pages.InfoPage, e.ClickedItem as Manga);
        }

        private async void GridView_PointerWheelChanged(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (GrabbingMangaFeed || MoreMangas.Count == 0 || MoreMangas.Count < CurrentIndex) return;
            var scrollViewer = ScrollViewer;
            var estimate = scrollViewer.ScrollableHeight * 0.8;
            if (scrollViewer.VerticalOffset > estimate)
            {
                GrabbingMangaFeed = true;
                try
                {
                    var Mangas = await Mangadexx.LoadManga(MangaIds: MoreMangas.Keys.Skip(CurrentIndex).Take(100).ToArray());
                    CurrentIndex += 100;
                    foreach (var i in Mangas)
                    {
                        i.Cover = await i.GetMainCover();
                        List.Add(i);
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    string s = "";
#endif
                }
                GrabbingMangaFeed = false;
            }
        }

        private void GridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var columns = Math.Ceiling(MainWindow.MainPage.Bounds.Width / 300);
            ((ItemsWrapGrid)((GridView)sender).ItemsPanelRoot).ItemWidth = e.NewSize.Width / columns;
        }

        private async void MangaPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!LoggedIn)
            {
                SelectionComboBox.IsEnabled = false;
                CounterAndLoginStatus.Data = "Not logged in!";
                return;
            }
            SelectionComboBox.IsEnabled = true;
            StatusChache = (Dictionary<Guid, MangaReadingStatus>)await Mangadexx.Client.Manga.GetReadingStatusesOfUser();
        }

        private void MangaPage_Unloaded(object sender, RoutedEventArgs e)
        {
            GC.Collect();
        }
    }
}