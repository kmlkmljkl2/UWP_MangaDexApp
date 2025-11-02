using MangaDexSharp;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using UWP_MangaDexApp.Tests;
using Windows.Storage;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UWP_MangaDexApp.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AccountPage : Page
    {
        private bool LoggedIn { get => MainWindow.Dex.Client.IsLoggedIn; }
        private string ProfilePicURL { get => "https://mangadex.org/avatar.png"; }
        private FakeClasses.ObservableString UserName { get; } = new FakeClasses.ObservableString();
        private FakeClasses.ObservableString ClientSecret { get; } = new FakeClasses.ObservableString();

        private FakeClasses.ObservableString ClientId { get; } = new FakeClasses.ObservableString();

        private FakeClasses.ObservableString Roles { get; } = new FakeClasses.ObservableString();
        private FakeClasses.ObservableString GuiD { get; } = new FakeClasses.ObservableString();
        private FakeClasses.ObservableString ErrorMessage { get; } = new FakeClasses.ObservableString();
        private MangaDex Mangadexx => MainWindow.Dex;

        public AccountPage()
        {
            this.InitializeComponent();
            Loaded += AccountPage_Loaded;
        }

        private void AccountPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (LoggedIn)
            {
                HideLogin();
            }
            else
            {
                ShowLogin();
            }
            var dataContainer = ApplicationData.Current.LocalSettings;
            if(dataContainer.Values.ContainsKey(LocalData.ClientSecret))
            {
                ClientSecret.Data = dataContainer.Values[LocalData.ClientSecret] as string;
            }
            if(dataContainer.Values.ContainsKey(LocalData.ClientId))
            {
                ClientId.Data = dataContainer.Values[LocalData.ClientId] as string;
            }
            if(dataContainer.Values.ContainsKey(LocalData.UserName))
            {
                UserName.Data = dataContainer.Values[LocalData.UserName] as string;
            }

        }

        private void HideLogin()
        {
            LoginGrid.Visibility = Visibility.Collapsed;
            AccountGrid.Visibility = Visibility.Visible;
            LoadData();
        }

        private void LoadData()
        {
            var user = Mangadexx.Client.CurrentUser;
            if (user == null) return;
            this.UserName.Data = user.Username;
            this.GuiD.Data = user.Id.ToString();
            this.Roles.Data = string.Join(", ", user.Roles);
        }

        private void ShowLogin()
        {
            LoginGrid.Visibility = Visibility.Visible;
            AccountGrid.Visibility = Visibility.Collapsed;
        }

        private async void LoginBTN_Click(object sender, RoutedEventArgs e)
        {
            if (UsernameBox.Text != "" && PasswordBox.Password != "")
            {
                MainWindow.Dex.Client.CleanCredentials();
                UserCredentials user = new UserCredentials(UsernameBox.Text, PasswordBox.Password, ClientIdBox.Text, ClientSecretBox.Text);
                MainWindow.Dex.Client.SetUserCredentials(user);
                try
                {
                    if (RememberMe.IsChecked == true)
                    {
                        MainWindow.MainPage.SaveSettings(UsernameBox.Text, MainWindow.Dex.Client.Auth.LastRefreshToken, ClientIdBox.Text, ClientSecretBox.Text);
                    }

                    await MainWindow.Dex.Client.Auth.Login();
                    PasswordBox.Password = "";
                    await MainWindow.Dex.LoadFollowedManga();
                    MainWindow.Dex.LoadMore();
                    HideLogin();

                    if (RememberMe.IsChecked == true)
                    {
                        MainWindow.MainPage.SaveSettings(UsernameBox.Text, MainWindow.Dex.Client.Auth.LastRefreshToken, ClientIdBox.Text, ClientSecretBox.Text);
                    }
                }
                catch (Exception ex)
                {
                    MainWindow.MainPage.ResetToken();
                    ShowLogin();
                    ErrorMessage.Data = $"Login Failed!\n{ex.Message}";
                }
            }
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            await MainWindow.Dex.Client.Auth.Logout();
            MainWindow.MainPage.ResetToken();
            MainWindow.Dex.FollowedFeed.Clear();
            ShowLogin();
        }

        private void PasswordBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key.Equals(VirtualKey.Enter))
            {
                LoginBTN_Click(null, null);
            }
        }
    }
}