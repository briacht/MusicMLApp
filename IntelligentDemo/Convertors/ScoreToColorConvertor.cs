using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace IntelligentDemo.Convertors
{
    public class ScoreToColorConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var score = value as double?;
            if (score == null || targetType != typeof(Brush))
            {
                return null;
            }
            else
            {
                return new SolidColorBrush(Convert(score));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static Color Convert(double? score)
        {
            // Green - orange - red
            if(score > 0.75)
            {
                return Color.FromArgb(255, 0, 255, 0);
            }
            else if (score < 0.25)
            {
                return Color.FromArgb(255, 255, 0, 0);

            }
            else
            {
                return Color.FromArgb(255, 0, 0, 255);

            }
        }
    }
}
