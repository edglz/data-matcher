using System;
using System.Globalization;
using System.Windows.Data;

namespace Comparador.Converters
{
    /// <summary>
    /// Convierte un valor booleano a "A" o "B"
    /// </summary>
    public class BoolToABConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "A" : "B";
            }

            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return stringValue == "A";
            }

            return false;
        }
    }
}
