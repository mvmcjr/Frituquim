using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Frituquim.Helpers;

public class MarginSetter
{
    public static Thickness GetMargin(DependencyObject obj) => (Thickness)obj.GetValue(MarginProperty);

    public static void SetMargin(DependencyObject obj, Thickness value) => obj.SetValue(MarginProperty, value);

    // Using a DependencyProperty as the backing store for Margin. This enables animation, styling, binding, etc…
    public static readonly DependencyProperty MarginProperty =
        DependencyProperty.RegisterAttached(nameof(FrameworkElement.Margin), typeof(Thickness),
            typeof(MarginSetter), new UIPropertyMetadata(new Thickness(), MarginChangedCallback));

    public static void MarginChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
    {
        // Make sure this is put on a panel
        var panel = sender as Panel;

        if (panel == null) return;

        panel.Loaded += Panel_Loaded;
    }

    private static void Panel_Loaded(object sender, EventArgs e)
    {
        var panel = sender as Panel;

        // Go over the children and set margin for them:
        foreach (FrameworkElement fe in panel.Children.OfType<FrameworkElement>())
            fe.Margin = GetMargin(panel);
    }
}