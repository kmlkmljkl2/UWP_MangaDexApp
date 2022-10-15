using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Text.Json;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UWP_MangaDexApp.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            this.InitializeComponent();
        }

        private static ApplicationDataContainer LocalSettings { get; } = ApplicationData.Current.LocalSettings;
        private SettingsClass Settingz { get; } = (SettingsClass)(LocalSettings.Values[LocalData.ClientSettings] != null ? JsonSerializer.Deserialize(LocalSettings.Values[LocalData.ClientSettings] as string, typeof(SettingsClass)) : new SettingsClass());
        public SettingsClass RequestSettings => Settingz;

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            LocalSettings.Values[LocalData.ClientSettings] = JsonSerializer.Serialize(Settingz);
        }

        public class SettingsClass : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public Action SettingsChanged { get; set; }

            public bool DataSaver
            {
                get => _dataSaver;
                set
                {
                    _dataSaver = value;
                    OnPropertyChanged();
                }
            }

            public bool DifferentPort
            {
                get => _differentPort;
                set
                {
                    _differentPort = value;
                    OnPropertyChanged();
                }
            }

            public bool Erotica
            {
                get => _erotica;
                set
                {
                    _erotica = value;
                    OnPropertyChanged();
                }
            }

            public bool Pornographic
            {
                get => _pornographic;
                set
                {
                    _pornographic = value;
                    OnPropertyChanged();
                }
            }

            public bool Safe
            {
                get => _safe;
                set
                {
                    _safe = value;
                    OnPropertyChanged();
                }
            }

            public bool Suggestive
            {
                get => _suggestive;
                set
                {
                    _suggestive = value;
                    OnPropertyChanged();
                }
            }

            public int ChapterLoadAmount
            {
                get => _chapterAmount;
                set
                {
                    if (_chapterAmount != value)
                    {
                        _chapterAmount = value;
                        OnPropertyChanged();
                    }
                }
            }

            private int _chapterAmount { get; set; } = 32;
            private bool _dataSaver { get; set; } = false;
            private bool _differentPort { get; set; } = false;
            private bool _erotica { get; set; } = true;
            private bool _pornographic { get; set; } = false;
            private bool _safe { get; set; } = true;
            private bool _suggestive { get; set; } = true;

            public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
            {
                // Raise the PropertyChanged event, passing the name of the property whose value has changed.
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}