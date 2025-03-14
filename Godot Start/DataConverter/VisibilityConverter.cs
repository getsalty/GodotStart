using Microsoft.UI.Xaml.Data;
using System;

namespace DataConverter
{
    public partial class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? "Visible": "Collapsed";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? "Visible" : "Collapsed";
        }
    }
}