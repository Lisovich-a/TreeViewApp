using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace TreeViewApp
{
    [ValueConversion(typeof(TreeViewItem), typeof(Thickness))]
    public class TreeViewMoveLeftConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Thickness(-MarginCalculator.GetLeftMargin(value),0,0,0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TreeViewMoveRightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Thickness(MarginCalculator.GetLeftMargin(value), 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    internal static class MarginCalculator
    {
        public static int GetLeftMargin(object value)
        {
            if (value is DependencyObject item)
            {
                var parent = VisualTreeHelper.GetParent(item);
                if (parent is TreeView)
                    return 0;

                if (parent is TreeViewItem)
                {
                    return 20 + GetLeftMargin(parent);
                }
                else
                {
                    return GetLeftMargin(parent);
                }
            }

            return 0;
        }
    }
}
