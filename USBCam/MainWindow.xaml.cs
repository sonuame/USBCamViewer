using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace USBCam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool DeviceExist = false;
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            getCamList();
            imgVideo.Stretch = System.Windows.Media.Stretch.Fill;
        }



        private void getCamList()
        {
            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                cboDevices.Items.Clear();
                if (videoDevices.Count == 0)
                    throw new ApplicationException();

                DeviceExist = true;
                foreach (FilterInfo device in videoDevices)
                {
                    cboDevices.Items.Add(device.Name);
                }
                cboDevices.SelectedIndex = 0; //make dafault to first cam
            }
            catch (ApplicationException)
            {
                DeviceExist = false;
                cboDevices.Items.Add("No capture device on your system");
            }
        }

        private void btnRef_Click(object sender, RoutedEventArgs e)
        {
            getCamList();
        }

        private void cboDevices_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            CloseVideoSource();

            if (DeviceExist && cboDevices.SelectedIndex > -1)
            {
                videoSource = new VideoCaptureDevice(videoDevices[cboDevices.SelectedIndex].MonikerString);
                videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
                CloseVideoSource();

                //videoSource.DesiredFrameRate = 10;
                videoSource.Start();

            }
        }

        private void CloseVideoSource()
        {
            if (!(videoSource == null))
                if (videoSource.IsRunning)
                {
                    videoSource.SignalToStop();
                    videoSource = null;
                }
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            var source = (Bitmap)eventArgs.Frame.Clone();
            using (source)
            {
                IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    new Int32Rect { Height = source.Height, Width = source.Width },
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromWidthAndHeight(source.Width, source.Height));
                bs.Freeze();
                imgVideo.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => imgVideo.Source = bs));
            }

        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            CloseVideoSource();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            CloseVideoSource();
            Application.Current.Shutdown();
        }
    }
}
