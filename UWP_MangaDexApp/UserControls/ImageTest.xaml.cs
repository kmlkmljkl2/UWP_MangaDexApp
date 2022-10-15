using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.ComponentModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UWP_MangaDexApp.UserControls
{
    public sealed partial class ImageTest : UserControl, INotifyPropertyChanged, IDisposable
    {
        public ImageTest()
        {
            this.InitializeComponent();
        }

        public BitmapImage Source
        {
            set
            {
                if (value == null) return;
                if (Image.Source != value)
                {
                    value.DownloadProgress += Value_DownloadProgress;
                    Image.Source = value;
                    OnPropertyChanged();
                }
            }
            get => (BitmapImage)Image.Source;
        }

        public Uri UriSource
        {
            set
            {
                if(value == null) return;
                BitmapImage bit = new BitmapImage(value)
                {
                    CreateOptions = BitmapCreateOptions.IgnoreImageCache
                };
                if (Image.Source == bit) return;

                bit.DownloadProgress += Value_DownloadProgress;
                bit.ImageFailed += Bit_ImageFailed;
                Image.Source = bit;
                OnPropertyChanged();
            }
        }

        private void Bit_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {


        }

        private void Value_DownloadProgress(object sender, DownloadProgressEventArgs e)
        {
            if(Ring == null)
            {
                return;
            }
            Ring.Value = e.Progress;
            if (e.Progress == 100)
            {
                Ring.Visibility = Visibility.Collapsed;
                Ring = null;
                Image.Visibility = Visibility.Visible;
            }
            OnPropertyChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            
            Image.Source = null;
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}