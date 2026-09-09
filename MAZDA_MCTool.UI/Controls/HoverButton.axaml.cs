using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace MAZDA_MCTool.UI.Controls;

public class HoverButton : Button
{
    public static readonly StyledProperty<IBrush?> HoveredBrushProperty =
        AvaloniaProperty.Register<HoverButton, IBrush?>(
            nameof(HoveredBrush),
            defaultValue: null);

    public IBrush? HoveredBrush
    {
        get => GetValue(HoveredBrushProperty);
        set => SetValue(HoveredBrushProperty, value);
    }
    
    public static readonly StyledProperty<double> HoveredTransformDurationProperty =
        AvaloniaProperty.Register<HoverButton, double>(
            nameof(HoveredTransformDuration),
            defaultValue: 0);

    public double HoveredTransformDuration
    {
        get => GetValue(HoveredTransformDurationProperty);
        set => SetValue(HoveredTransformDurationProperty, value);
    }
}