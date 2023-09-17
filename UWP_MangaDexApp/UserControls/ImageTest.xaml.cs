using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;
using static System.Net.Mime.MediaTypeNames;
using Image = Microsoft.UI.Xaml.Controls.Image;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UWP_MangaDexApp.UserControls
{
    public sealed partial class ImageTest : UserControl, INotifyPropertyChanged
    {
       // private static HttpClient _httpClient = new HttpClient();
        public ImageTest()
        {
            this.InitializeComponent();
        }

        //public BitmapImage Source
        //{
        //    set
        //    {
        //        if (value == null) return;
        //        if (PageImage.Source != value)
        //        {
        //            value.DownloadProgress += Value_DownloadProgress;
        //            PageImage.Source = value;
        //            OnPropertyChanged();
        //            value = null;
        //        }

        //    }
        //    get => (BitmapImage)PageImage.Source;
        //}

        public Uri UriSource
        {
            set
            {
                if(value == null) return;

                SetImage(value);
             //   OnPropertyChanged();

                return;

                //BitmapImage bit = new BitmapImage()
                //{
                //    CreateOptions = BitmapCreateOptions.IgnoreImageCache
                //};
                //bit.UriSource = value;
                //if (PageImage.Source == bit)
                //{
                //    bit = null;
                //    return; 
                //}


                //bit.DownloadProgress += Value_DownloadProgress;
                //bit.ImageFailed += Bit_ImageFailed;
                //PageImage.Source = bit;


                //bit = null;
                
               
            }
        }
        private async void SetImage(Uri imageUrl)
        {
            //var response = await _httpClient.GetAsync(imageUrl);

           // if (response.IsSuccessStatusCode)
            {
               // var stream = await response.Content.ReadAsStreamAsync();
                BitmapImage bitmapImage = new BitmapImage(imageUrl);
                //  await bitmapImage.SetSourceAsync(stream.AsRandomAccessStream());
                //WriteableBitmap writeableBitmap = new WriteableBitmap(1, 1);
                //await writeableBitmap.SetSourceAsync(stream.AsRandomAccessStream());
                bitmapImage.DownloadProgress += Value_DownloadProgress;
                PageImage.Source = bitmapImage;

                //if (Ring != null)
                //{
                //    Ring.Visibility = Visibility.Collapsed;
                //    Ring = null;
                //}
                PageImage.Visibility = Visibility.Visible;
                OnPropertyChanged();

                bitmapImage.DecodePixelHeight = 0;
                bitmapImage.DecodePixelWidth = 0;
            }

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
                PageImage.Visibility = Visibility.Visible;
            }
            OnPropertyChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        ~ImageTest()
        {
            //PageImage.Width = 0;
            //PageImage.Height = 0;
            PageImage = null;
        }
    }
}