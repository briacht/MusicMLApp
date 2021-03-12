using System;
using System.Globalization;
using System.Windows.Data;

namespace IntelligentDemo.Convertors
{
    public class ScoreToWidthConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var score = value as double?;
            if (score == null || targetType != typeof(double))
            {
                return null;
            }
            else
            {
                return score.Value * 350;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
