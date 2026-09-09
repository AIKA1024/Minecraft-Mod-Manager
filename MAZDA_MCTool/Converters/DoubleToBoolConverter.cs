using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace MAZDA_MCTool.Converters;

public class DoubleToBoolConverter :MarkupExtension, IValueConverter
{
  public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    if (double.TryParse(parameter?.ToString(),out var p) && value is double v)
      return Math.Abs(p - v) < 0.001;
    return false;
  }

  public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    throw new NotImplementedException();
  }

  public override object ProvideValue(IServiceProvider serviceProvider)
  {
    return this;
  }
}