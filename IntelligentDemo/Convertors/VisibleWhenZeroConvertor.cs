using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace IntelligentDemo.Convertors
{
    public sealed class VisibleWhenZeroConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null || System.Convert.ToInt32(value) == 0 ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
