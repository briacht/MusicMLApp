using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace IntelligentDemo.Convertors
{
    public class RatioConverter : DependencyObject, IValueConverter
    {
        public double MaxValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        { 
            double size = System.Convert.ToDouble(value) * System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
            return Math.Min(size, MaxValue).ToString("G0", CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { 
            throw new NotImplementedException();
        }
    }
}
