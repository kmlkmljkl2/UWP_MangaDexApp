using MangaDexSharp.Resources;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UWP_MangaDexApp.MainPages;
using UWP_MangaDexApp.Settings;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UWP_MangaDexApp
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public static UIElement StartPage { get; private set; }
        public static InfoPage InfoPage { get; } = new InfoPage();
        public static MainWindow MainPage { get; private set; }
        public static ReadPage ReadPage { get; } = new ReadPage();
        public static MangaDex Dex { get; private set; }
        private MangaDex PriDex => Dex;
        public static SettingsPage SettingsP { get; } = new SettingsPage();
        private ApplicationDataContainer LocalSettings { get; } = ApplicationData.Current.LocalSettings;
        public string Username => LocalSettings.Values[LocalData.UserName] == null ? null : LocalSettings.Values[LocalData.UserName] as string;
        public string Token => LocalSettings.Values[LocalData.Token] == null ? null : LocalSettings.Values[LocalData.Token] as string;
        private double FollowedFeedScrollOffset { get; set; }
        private double MangaFeedScrollOffset { get; set; }

        public MainWindow()
        {
            this.InitializeComponent();
            MainPage = this;
            Start();
        }

        private async void Start()
        {
            while (!_contentLoaded)
            {
                await Task.Delay(100);
            }

            // base.Content = InfoPage;

            SetAppName();
            StartPage = Content;
            Dex = new MangaDex(Token);

            //Load Settings Start app
        }

        public void ResetToken()
        {
            LocalSettings.Values[LocalData.Token] = null;
        }

        public void SaveSettings(string UserName = null, string Token = null)
        {
            if (UserName != null)
            {
                LocalSettings.Values[LocalData.UserName] = UserName;
            }
            if (Token != null)
            {
                LocalSettings.Values[LocalData.Token] = Token;
            }
        }

        private void MangaFeed_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem == null) return;
            GoToPage(ETC.Pages.InfoPage);
            InfoPage.SetContent(e.ClickedItem as Manga);
        }

        public void GoToPage(ETC.Pages page, Manga manga = null)
        {
            switch (page)
            {
                case ETC.Pages.ReadPage:
                    Content = ReadPage;
                    break;

                case ETC.Pages.StartPage:
                    Content = StartPage;
                    break;

                case ETC.Pages.InfoPage:
                    Content = InfoPage;
                    if (manga != null)
                    {
                        InfoPage.SetContent(manga);
                        InfoPage.CameFromSettings = true;
                    }
                    break;

                case ETC.Pages.SettingsPage:
                    Content = SettingsP;
                    break;
            }
        }

        public void GoToSettings(SettingsEnum.SettingsPage? page = null)
        {
            GoToPage(ETC.Pages.SettingsPage);
            if (page == null)
                SettingsP.GoToPage();
            else
                SettingsP.GoToPage(page);
        }

        private void SettingsBTN_Click(object sender, RoutedEventArgs e)
        {
            GoToSettings(SettingsEnum.SettingsPage.Settings);
        }

        private async void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Microsoft.UI.Xaml.Controls.Primitives.ToggleButton)sender;
            if (button.IsChecked == false)
            {
                FollowedFeedScrollOffset = MangaFeedScroll.VerticalOffset;
                MangaFeed.ItemsSource = Dex.MangaFeed;
                MangaFeedScroll.ScrollToVerticalOffset(MangaFeedScrollOffset);
            }
            else
            {
                MangaFeedScrollOffset = MangaFeedScroll.VerticalOffset;
                MangaFeed.ItemsSource = Dex.FollowedFeed;
                MangaFeedScroll.ScrollToVerticalOffset(FollowedFeedScrollOffset);
                if (Dex.FollowedFeed.Count == 0)
                {
                    await Dex.LoadFollowedManga();
                }
            }
        }

        private async void LoadMoreBTN_Click(object sender, RoutedEventArgs e)
        {
            if (ToggBTN.IsChecked == true)
            {
                await Dex.LoadFollowedManga();
            }
            else
            {
                await Dex.LoadMangaFeed();
            }
        }

        private async void RefreshBTN_Click(object sender, RoutedEventArgs e)
        {
            await Dex.Refresh();
            GC.Collect();
        }

        private void ProfileBTN_Click(object sender, RoutedEventArgs e)
        {
            GoToPage(ETC.Pages.SettingsPage);
            SettingsP.GoToPage(SettingsEnum.SettingsPage.Account);
        }

        private void SeasonalManga_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            FlipView flipView = (FlipView)sender;
            if (flipView.SelectedItem != null)
            {
                Content = InfoPage;
                InfoPage.SetContent(flipView.SelectedItem as Manga);
            }
        }

        private void MangaFeed_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var columns = Math.Ceiling(base.Bounds.Width / 300);
            ((ItemsWrapGrid)MangaFeed.ItemsPanelRoot).ItemWidth = e.NewSize.Width / columns;
        }

        private bool GrabbingMangaFeed = false;

        private async void MangaFeedScroll_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (GrabbingMangaFeed) return;
            var scrollViewer = MangaFeedScroll;

            var estimate = scrollViewer?.ScrollableHeight * 0.95;
            if (scrollViewer?.VerticalOffset >= estimate)
            {
                GrabbingMangaFeed = true;
                // var test = scrollViewer.VerticalOffset;
                try
                {
                    if (MangaFeed.ItemsSource == Dex.MangaFeed)
                        await Dex.LoadMangaFeed();
                    if (MangaFeed.ItemsSource == Dex.FollowedFeed)
                        await Dex.LoadFollowedManga();
                    if (MangaFeed.ItemsSource == Dex.SearchResult)
                        await Dex.SearchMore();
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
        private bool Searching = false;
        private async void Searchbar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Searching) return;
            Searching = true;
            var box = (TextBox)sender;
            string text = box.Text;
            while (true)
            {
                if (box.Text == "")
                    break;
                await Task.Delay(500);
                if (box.Text == text)
                {
                    Searching = false;
                    break;
                }
                else
                    text = box.Text;
            }
            if (text == "")
            {
                if (ToggBTN.IsChecked == false)
                {
                    MangaFeed.ItemsSource = Dex.MangaFeed;
                }
                else
                {
                    MangaFeed.ItemsSource = Dex.FollowedFeed;
                }
                Searching = false;
                return;
            }
            await Dex.Search(box.Text);
            MangaFeed.ItemsSource = Dex.SearchResult;

        }

        public void SetAppName(string Name = "MangaDexApp")
        {
            base.Title = Name;
        }

        //private void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    StackPanel block = sender as StackPanel;
        //    block.MaxWidth = Bounds.Width * 0.95;
        //}
    }

    public class ETC
    {
        public enum Pages
        {
            ReadPage,
            StartPage,
            InfoPage,
            SettingsPage
        }
    }
}
