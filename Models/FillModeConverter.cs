using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace DrawingAppCG.Models.Converters
{
    public class FillModeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is FillMode fillMode && parameter is string param)
            {
                switch (param)
                {
                    case "None":
                        return fillMode == FillMode.None;
                    case "Color":
                        return fillMode == FillMode.Color;
                    case "Image":
                        return fillMode == FillMode.Image;
                }
            }
            return false;
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter is string param)
            {
                if(Enum.TryParse<FillMode>(param, out var fillMode))
                    return fillMode;
            }

            return null;
        }
    }
}
