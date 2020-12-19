using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace TAF.AutomationTool.Ui.Converters
{
    public class IntToStringConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ComboBoxItem item)) return 0;
            int.TryParse(item.Content?.ToString() ?? string.Empty, out var result);
            return result;

        }
    }
}
