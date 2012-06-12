using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace LocalizationSupport.Converters
{
    /// <summary>
    /// Converts a value of type System.Drawing.Image into a BitmapImage that can be used as ImageSource
    /// property value.
    /// </summary>
    public class BitmapImageToImageSourceConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            System.Drawing.Image image = value as System.Drawing.Image;
            MemoryStream ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            BitmapImage bm = new BitmapImage();
            bm.BeginInit();
            bm.StreamSource = ms;
            bm.EndInit();
            return bm;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
