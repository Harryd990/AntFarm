using AntFarm.main;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AntFarm.handelers
{

    public class GridRenderer
    {

        private static readonly Dictionary<string, Brush> CellBrushes = new(StringComparer.OrdinalIgnoreCase)
        {
            { nameof(Air),   new SolidColorBrush(Color.FromRgb(20,  20,  40))  },
            { nameof(Dirt),  new SolidColorBrush(Color.FromRgb(101, 67,  33))  },
            { nameof(stone), new SolidColorBrush(Color.FromRgb(80,  80,  80))  },

        };

        private static readonly Brush FallbackBrush = Brushes.HotPink;
    }

}