using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace RectExtractDotnet.Converters
{
    public abstract class BaseConverter : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    public class BoolStringConverter : BaseConverter, IValueConverter
    {
        public string T { get; set; }
        public string F { get; set; }

        public object Convert(object value, Type targetType, object parameter,
                System.Globalization.CultureInfo culture)
        {
            return (bool)value ? T : F;
        }

        public object ConvertBack(object value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
