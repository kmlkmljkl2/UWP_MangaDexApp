using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UWP_MangaDexApp.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private static MangaPage mangaPage { get; } = new MangaPage();
        private static Lists listPage { get; } = new Lists();
        private static AccountPage accountPage { get; } = new AccountPage();
        private SettingsEnum.SettingsPage? LastPage { get; set; } = null;
        private static Settings settingsPage { get; } = new Settings();

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        public Settings.SettingsClass GetSettings()
        {
            return settingsPage.RequestSettings;
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem == null || args.SelectedItem.GetType() != typeof(NavigationViewItem)) return;
            if (args.IsSettingsSelected)
            {
                GoToPage(SettingsEnum.SettingsPage.Settings);
                return;
            }
            NavigationViewItem item = args.SelectedItem as NavigationViewItem;
            switch (item.Tag)
            {
                case "Account":
                    GoToPage(SettingsEnum.SettingsPage.Account);
                    break;

                case "List":
                    GoToPage(SettingsEnum.SettingsPage.List);
                    break;

                case "Manga":
                    GoToPage(SettingsEnum.SettingsPage.Manga);
                    break;
            }
        }

        public void GoToPage(SettingsEnum.SettingsPage? settings = null)
        {
            if ((settings != null && LastPage != null) && (settings == LastPage)) return;

            if (settings == null && LastPage != null)
            {
                settings = LastPage;
            }
            if (settings == null)
            {
                settings = SettingsEnum.SettingsPage.Account;
            }
            if (settings != SettingsEnum.SettingsPage.Manga)
            {
                mangaPage.Clear();
            }
            switch (settings)
            {
                case SettingsEnum.SettingsPage.Manga:
                    NavView.Content = mangaPage;
                    break;

                case SettingsEnum.SettingsPage.List:
                    NavView.Content = listPage;
                    break;

                case SettingsEnum.SettingsPage.Account:
                    NavView.Content = accountPage;
                    break;

                case SettingsEnum.SettingsPage.Settings:
                    NavView.Content = settingsPage;
                    NavView.SelectedItem = null;
                    break;
            }
            LastPage = settings;
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            MainWindow.MainPage.GoToPage(ETC.Pages.StartPage);
        }
    }
}