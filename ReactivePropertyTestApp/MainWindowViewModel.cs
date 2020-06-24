using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RPTestApp
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ReactiveProperty<ImageSource> Icon { get; set; } = new ReactiveProperty<ImageSource>();
        //public ImageSource Icon { get; set; }

        public ReactiveCommand ChangeImageCommand { get; set; } = new ReactiveCommand();

        private List<Bitmap> bitmaps;
        private int iconNo = 0;

        public MainWindowViewModel()
        {
            bitmaps = new List<Bitmap>()
            {
                Resources.error_icon,
                Resources.warning_icon,
                Resources.disconnect_icon,
                Resources.connect_icon,
                Resources.setting_icon,
            };

            Icon.Value = createImageSource(bitmaps.First());

            ChangeImageCommand.Subscribe(async () =>
            {
                // UIスレッドでは大丈夫
                //changeIcon();

                // 別スレッドでやるとうまくいかない
                await Task.Run(() => changeIcon());
            });
        }

        private void changeIcon()
        {
            this.iconNo++;
            if (iconNo < bitmaps.Count)
            {
                Icon.Value = createImageSource(this.bitmaps[iconNo]);
            }
            else
            {
                Icon.Value = createImageSource(this.bitmaps.First());
                this.iconNo = 0;
            }
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        private ImageSource createImageSource(Bitmap bmp)
        {
            if (bmp == null)
            {
                return null;
            }

            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(handle);
            }
        }
    }
}
