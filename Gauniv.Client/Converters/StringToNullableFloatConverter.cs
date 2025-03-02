using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Gauniv.Client.Converters
{
    public class StringToNullableFloatConverter : IValueConverter
    {
        // Convertit un float? en string
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float floatValue)
            {
                return floatValue.ToString(culture);
            }
            return string.Empty;
        }

        // Convertit une string en float? ; si la chaîne est vide, renvoie null
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;
            if (string.IsNullOrWhiteSpace(s))
                return null;
            if (float.TryParse(s, NumberStyles.Float, culture, out float result))
                return result;
            return null;
        }
    }
}
