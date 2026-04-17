using AntFarm.main;
using AntFarm.entetys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AntFarm.handelers
{
    public class GridRenderer
    {
        
        private static readonly Dictionary<string, Brush> CellBrushes = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Air",   new SolidColorBrush(Color.FromRgb(169, 245, 110)) }, 
            { "Dirt",  new SolidColorBrush(Color.FromRgb(160, 82, 45)) },  
            { "stone", new SolidColorBrush(Color.FromRgb(169, 169, 169)) } 
        };

        private static readonly Brush UndergroundAirBrush = new SolidColorBrush(Color.FromRgb(148, 125, 101)); // #947d65
        private static readonly Brush FallbackBrush = Brushes.HotPink;
        private static readonly Brush MatteBlackBorder = new SolidColorBrush(Color.FromRgb(30, 30, 30));

        public static void Render(Game game, Canvas canvas)
        {
            if (game == null || canvas == null || game.grid == null) return;
            
            var (cols, rows) = game.getGridDims();
            if (cols <= 0 || rows <= 0) return;

            canvas.Children.Clear();

            double cellWidth = canvas.ActualWidth / cols;
            double cellHeight = canvas.ActualHeight / rows;

            // Define which task types get a red 'X' marker on the map
            var targetTaskTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
            { 
                "dig", 
                "buildfarm", 
                "buildfoodstore" 
            };

            // Pre-calculate all orders to find them efficiently:
            // 1. From pending tasks in the queue
            var queuedMarkers = game.queue1.tasks
                .Where(t => targetTaskTypes.Contains(t.tasktype))
                .Select(t => t.targetposition);

            // 2. From assigned tasks currently being worked by ants
            var assignedMarkers = new List<(int, int)>();
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    var cell = game.grid.GetCellAtLocation(x, y);
                    foreach (var e in cell.Entities)
                    {
                        if (e is Ant ant && ant.Currenttask != null && targetTaskTypes.Contains(ant.Currenttask.tasktype))
                        {
                            assignedMarkers.Add(ant.Currenttask.targetposition);
                        }
                    }
                }
            }

            // Combine both into a single fast lookup set
            var actionMarkers = queuedMarkers.Concat(assignedMarkers).ToHashSet();

            // PHASE 1: Draw all base grid cells first (guarantees they stay in the background)
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    double xPos = x * cellWidth;
                    double yPos = y * cellHeight;

                    var cell = game.grid.GetCellAtLocation(x, y);
                    string cellType = cell.GetType().Name;

                    Brush fillBrush;
                    // If it's Air and in the bottom 3/4 of the grid, use the underground color
                    if (string.Equals(cellType, "Air", StringComparison.OrdinalIgnoreCase) && y >= rows / 4.0)
                    {
                        fillBrush = UndergroundAirBrush;
                    }
                    else
                    {
                        fillBrush = CellBrushes.TryGetValue(cellType, out Brush brush) ? brush : FallbackBrush;
                    }

                    Rectangle rect = new Rectangle
                    {
                        Width = cellWidth,
                        Height = cellHeight,
                        Fill = fillBrush,
                        Stroke = MatteBlackBorder, 
                        StrokeThickness = 0.5      
                    };
                    Canvas.SetLeft(rect, xPos);
                    Canvas.SetTop(rect, yPos);
                    canvas.Children.Add(rect);
                }
            }

            // PHASE 2: Draw top entity and markers
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    double xPos = x * cellWidth;
                    double yPos = y * cellHeight;

                    var cell = game.grid.GetCellAtLocation(x, y);
                    var entities = cell.Entities.ToList();

                    if (entities.Count > 0)
                    {
                        // Get highest precedence entity (lowest number)
                        var entityToDraw = entities.OrderBy(e => GetPrecedence(e)).First();
                        
                        DrawEntity(entityToDraw, xPos, yPos, cellWidth, cellHeight, canvas);

                        // Draw +x badge in corner if multiple entities are present
                        if (entities.Count > 1)
                        {
                            DrawBadge(entities.Count - 1, xPos, yPos, cellWidth, cellHeight, canvas);
                        }
                    }

                    // Draw Action Orders (Red "X" for dig and build tasks)
                    if (actionMarkers.Contains((x, y)))
                    {
                        // Use a brighter red and thicker stroke
                        Brush vibrantRed = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                        double markerThickness = Math.Max(3, cellWidth * 0.1);

                        Line line1 = new Line
                        {
                            Stroke = vibrantRed,
                            StrokeThickness = markerThickness,
                            X1 = xPos + cellWidth * 0.1,
                            Y1 = yPos + cellHeight * 0.1,
                            X2 = xPos + cellWidth * 0.9,
                            Y2 = yPos + cellHeight * 0.9,
                            StrokeEndLineCap = PenLineCap.Round,
                            StrokeStartLineCap = PenLineCap.Round
                        };
                        Line line2 = new Line
                        {
                            Stroke = vibrantRed,
                            StrokeThickness = markerThickness,
                            X1 = xPos + cellWidth * 0.9,
                            Y1 = yPos + cellHeight * 0.1,
                            X2 = xPos + cellWidth * 0.1,
                            Y2 = yPos + cellHeight * 0.9,
                            StrokeEndLineCap = PenLineCap.Round,
                            StrokeStartLineCap = PenLineCap.Round
                        };
                        canvas.Children.Add(line1);
                        canvas.Children.Add(line2);
                    }
                }
            }
        }

        private static int GetPrecedence(Entity e)
        {
            if (e is FoodStore || e is farm || e is Food) return 1;
            if (e is Queen) return 2;
            if (e is Egg) return 3;
            if (e is Ant) return 4;
            return 5;
        }

        private static void DrawEntity(Entity e, double xPos, double yPos, double cellWidth, double cellHeight, Canvas canvas)
        {
            if (e is Food || e is farm || e is FoodStore)
            {
                float fillFraction = 0f;

                if (e is Food f)
                {
                    fillFraction = f.fractionOfFoodLeft();
                    fillFraction = Math.Max(0f, Math.Min(1f, float.IsNaN(fillFraction) ? 0f : fillFraction));

                    // Natural Food: 5 small orange circles in a pyramid (2 on top, 3 on bottom)
                    double d = cellWidth * 0.25; // diameter
                    Brush foodBrush = Brushes.Orange;

                    // Bottom row (3 circles)
                    double bottomY = yPos + cellHeight * 0.45;
                    for (int i = 0; i < 3; i++)
                    {
                        Ellipse circle = new Ellipse { Width = d, Height = d, Fill = foodBrush, Stroke = Brushes.Black, StrokeThickness = 0.5 };
                        Canvas.SetLeft(circle, xPos + cellWidth * 0.125 + (i * d));
                        Canvas.SetTop(circle, bottomY);
                        canvas.Children.Add(circle);
                    }

                    // Top row (2 circles)
                    double topY = yPos + cellHeight * 0.2;
                    for (int i = 0; i < 2; i++)
                    {
                        Ellipse circle = new Ellipse { Width = d, Height = d, Fill = foodBrush, Stroke = Brushes.Black, StrokeThickness = 0.5 };
                        Canvas.SetLeft(circle, xPos + cellWidth * 0.25 + (i * d));
                        Canvas.SetTop(circle, topY);
                        canvas.Children.Add(circle);
                    }
                }
                else
                {
                    Brush fillColor;
                    if (e is farm fm)
                    {
                        fillColor = Brushes.ForestGreen; // Deep forest green
                        fillFraction = fm.fractionOfFoodLeft();
                    }
                    else // FoodStore
                    {
                        fillColor = Brushes.SaddleBrown; // Slightly different color for store to distinguish from natural orange food
                        fillFraction = ((FoodStore)e).fractionoffoodleft();
                    }

                    fillFraction = Math.Max(0f, Math.Min(1f, float.IsNaN(fillFraction) ? 0f : fillFraction));

                    // Draw Hexagon (6 points) for Stores and Farms
                    Polygon hexagon = new Polygon
                    {
                        Fill = fillColor,
                        Stroke = Brushes.Black,
                        StrokeThickness = 0.5,
                        Points = new PointCollection
                        {
                            new Point(xPos + cellWidth * 0.5, yPos + cellHeight * 0.1),  // Top Center
                            new Point(xPos + cellWidth * 0.9, yPos + cellHeight * 0.3),  // Top Right
                            new Point(xPos + cellWidth * 0.9, yPos + cellHeight * 0.7),  // Bottom Right
                            new Point(xPos + cellWidth * 0.5, yPos + cellHeight * 0.9),  // Bottom Center
                            new Point(xPos + cellWidth * 0.1, yPos + cellHeight * 0.7),  // Bottom Left
                            new Point(xPos + cellWidth * 0.1, yPos + cellHeight * 0.3)   // Top Left
                        }
                    };
                    canvas.Children.Add(hexagon);
                }

                // Draw Horizontal Fill Bar
                double maxBarWidth = cellWidth * 0.6; // Max width of the bar
                double currentBarWidth = maxBarWidth * fillFraction;

                Rectangle bar = new Rectangle
                {
                    Width = currentBarWidth,
                    Height = cellHeight * 0.25, // Increased from 0.15 to make the bar taller
                    Fill = Brushes.LimeGreen, // Bar color
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.5
                };

                // Center bar horizontally and align it near the bottom (moved slightly up to fit the taller bar)
                Canvas.SetLeft(bar, xPos + (cellWidth * 0.2));
                Canvas.SetTop(bar, yPos + cellHeight * 0.70);
                canvas.Children.Add(bar);
            }
            else if (e is Egg)
            {
                // Scaled down to 0.25 and perfectly centered
                double eggSize = cellWidth * 0.25;
                
                Ellipse e1 = new Ellipse { Width = eggSize, Height = eggSize, Fill = Brushes.White, Stroke = Brushes.Black, StrokeThickness = 0.5 };
                Canvas.SetLeft(e1, xPos + (cellWidth * 0.225));
                Canvas.SetTop(e1, yPos + (cellHeight * 0.425)); 
                
                Ellipse e2 = new Ellipse { Width = eggSize, Height = eggSize, Fill = Brushes.White, Stroke = Brushes.Black, StrokeThickness = 0.5 };
                Canvas.SetLeft(e2, xPos + (cellWidth * 0.525));
                Canvas.SetTop(e2, yPos + (cellHeight * 0.425)); 

                Ellipse e3 = new Ellipse { Width = eggSize, Height = eggSize, Fill = Brushes.White, Stroke = Brushes.Black, StrokeThickness = 0.5 };
                Canvas.SetLeft(e3, xPos + (cellWidth * 0.375));
                Canvas.SetTop(e3, yPos + (cellHeight * 0.225)); 

                canvas.Children.Add(e1);
                canvas.Children.Add(e2);
                canvas.Children.Add(e3);
            }
            else if (e is Ant)
            {
                Brush entityBrush = e is Queen ? Brushes.Yellow : Brushes.DarkGray; 
                
                Ellipse antCircle = new Ellipse
                {
                    Width = cellWidth * 0.7,
                    Height = cellHeight * 0.7,
                    Fill = entityBrush
                };
                
                Canvas.SetLeft(antCircle, xPos + (cellWidth * 0.15));
                Canvas.SetTop(antCircle, yPos + (cellHeight * 0.15));
                canvas.Children.Add(antCircle);
            }
        }

        private static void DrawBadge(int additionalCount, double xPos, double yPos, double cellWidth, double cellHeight, Canvas canvas)
        {
            TextBlock tb = new TextBlock
            {
                Text = $"+{additionalCount}",
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)), // Semi-transparent black background
                FontSize = cellHeight * 0.35,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(1)
            };
            
            // Positioning it towards the top right corner of the cell
            Canvas.SetLeft(tb, xPos + cellWidth * 0.55);
            Canvas.SetTop(tb, yPos + cellHeight * 0.05);
            
            canvas.Children.Add(tb);
        }
    }
}