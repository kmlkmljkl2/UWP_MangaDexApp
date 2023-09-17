using MangaDexSharp.Resources;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Net.Http;
using UWP_MangaDexApp.UserControls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UWP_MangaDexApp.MainPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ReadPage : Page
    {
        private static HttpClient _Client = new HttpClient();
        public ReadPage()
        {
            this.InitializeComponent();
        }

        public void AddImage(Uri Image)
        {
            list.Items.Add(Image);
        }

        public void ClearItems()
        {
            //foreach(ImageTest i in list.Items)
            //{
            //    i.Height = 0;
            //    i.Width = 0;
            //}

            list.Items.Clear();
            
            GC.Collect();
        }

        private void BTNBack_Click(object sender, RoutedEventArgs e)
        {
            ClearItems();
            MainWindow.MainPage.GoToPage(ETC.Pages.InfoPage);
        }

        private void FullScreenBTN_Click(object sender, RoutedEventArgs e)
        {
        }

        private async void NextPageBTN_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.InfoPage.List.SelectedIndex - 1 > -1)
            {
                var current = MainWindow.InfoPage.List.SelectedItem as Chapter;
                while (current.ChapterName == (MainWindow.InfoPage.List.SelectedItem as Chapter).ChapterName || (MainWindow.InfoPage.List.SelectedItem as Chapter).IsExternal)
                {
                    if (MainWindow.InfoPage.List.SelectedIndex - 1 == -1)
                    {
                        break;
                    }
                    MainWindow.InfoPage.List.SelectedIndex--;
                }
                await MainWindow.InfoPage.NextChapter(MainWindow.InfoPage.List.SelectedItem as Chapter);
            }
        }

        private async void PreviousPageBTN_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.InfoPage.List.SelectedIndex + 1 < MainWindow.InfoPage.List.Items.Count)
            {
                var current = MainWindow.InfoPage.List.SelectedItem as Chapter;
                // In case 2 Scanlations do the same manga
                while (current.ChapterName == (MainWindow.InfoPage.List.SelectedItem as Chapter).ChapterName || (MainWindow.InfoPage.List.SelectedItem as Chapter).IsExternal)
                {
                    if (MainWindow.InfoPage.List.SelectedIndex + 1 == MainWindow.InfoPage.List.Items.Count)
                    {
                        break;
                    }
                    MainWindow.InfoPage.List.SelectedIndex++;
                }
                await MainWindow.InfoPage.NextChapter(MainWindow.InfoPage.List.SelectedItem as Chapter);
            }
        }
    }
}