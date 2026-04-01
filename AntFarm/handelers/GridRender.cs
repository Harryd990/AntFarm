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
        // Air is now #a9f56e (RGB: 169, 245, 110)
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

            // Pre-calculate all dig orders to find them efficiently
            var digOrders = game.queue1.tasks
                .Where(t => string.Equals(t.tasktype, "dig", StringComparison.OrdinalIgnoreCase))
                .Select(t => t.targetposition)
                .ToHashSet();

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

                    // Draw Dig Orders (Red "X")
                    if (digOrders.Contains((x, y)))
                    {
                        Line line1 = new Line
                        {
                            Stroke = Brushes.Red,
                            StrokeThickness = 2,
                            X1 = xPos,
                            Y1 = yPos,
                            X2 = xPos + cellWidth,
                            Y2 = yPos + cellHeight
                        };
                        Line line2 = new Line
                        {
                            Stroke = Brushes.Red,
                            StrokeThickness = 2,
                            X1 = xPos + cellWidth,
                            Y1 = yPos,
                            X2 = xPos,
                            Y2 = yPos + cellHeight
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
                Brush fillColor;
                string labelText;
                Brush textColor = Brushes.Black;

                if (e is Food)
                {
                    fillColor = Brushes.Yellow; // Vibrant yellow
                    labelText = "food";
                }
                else if (e is farm)
                {
                    fillColor = Brushes.ForestGreen; // Deep forest green
                    labelText = "farm";
                    textColor = Brushes.White; // Make text readable on dark green background
                }
                else // FoodStore
                {
                    fillColor = Brushes.Orange;
                    labelText = "store";
                }

                // Slightly smaller triangles (brought in coordinates from edges)
                Polygon triangle = new Polygon
                {
                    Fill = fillColor,
                    Points = new PointCollection
                    {
                        new Point(xPos + cellWidth * 0.5, yPos + cellHeight * 0.1),  // Top Center
                        new Point(xPos + cellWidth * 0.9, yPos + cellHeight * 0.9),  // Bottom Right
                        new Point(xPos + cellWidth * 0.1, yPos + cellHeight * 0.9)   // Bottom Left
                    }
                };
                canvas.Children.Add(triangle);

                TextBlock label = new TextBlock
                {
                    Text = labelText,
                    Foreground = textColor,
                    FontSize = cellHeight * 0.35, // Bigger text
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    Width = cellWidth
                };

                // Push text slightly further down so it fits in the wider bottom part of the triangle
                Canvas.SetLeft(label, xPos);
                Canvas.SetTop(label, yPos + (cellHeight * 0.5)); 
                canvas.Children.Add(label);
            }
            else if (e is Egg)
            {
                double eggSize = cellWidth * 0.4;
                
                Ellipse e1 = new Ellipse { Width = eggSize, Height = eggSize, Fill = Brushes.White };
                Canvas.SetLeft(e1, xPos + (cellWidth * 0.15));
                Canvas.SetTop(e1, yPos + (cellHeight * 0.5));
                
                Ellipse e2 = new Ellipse { Width = eggSize, Height = eggSize, Fill = Brushes.White };
                Canvas.SetLeft(e2, xPos + (cellWidth * 0.55));
                Canvas.SetTop(e2, yPos + (cellHeight * 0.5));

                Ellipse e3 = new Ellipse { Width = eggSize, Height = eggSize, Fill = Brushes.White };
                Canvas.SetLeft(e3, xPos + (cellWidth * 0.35));
                Canvas.SetTop(e3, yPos + (cellHeight * 0.2));

                canvas.Children.Add(e1);
                canvas.Children.Add(e2);
                canvas.Children.Add(e3);
            }
            else if (e is Ant)
            {
                Brush entityBrush = e is Queen ? Brushes.Yellow : Brushes.DarkGray; // Changed from LightGray to DarkGray
                
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