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

                    Brush fillBrush = CellBrushes.TryGetValue(cellType, out Brush brush) ? brush : FallbackBrush;
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

            // PHASE 2: Draw all entities, buildings, and markers on top of the grid
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    double xPos = x * cellWidth;
                    double yPos = y * cellHeight;

                    var cell = game.grid.GetCellAtLocation(x, y);
                    var entities = cell.Entities.ToList();
                    
                    var farmEntity = entities.OfType<farm>().FirstOrDefault();
                    var foodStoreEntity = entities.OfType<FoodStore>().FirstOrDefault();
                    var naturalFood = entities.OfType<Food>().FirstOrDefault();
                    var eggs = entities.OfType<Egg>().ToList();
                    var allAnts = entities.OfType<Ant>().ToList();

                    // Draw Natural Food (Lemon triangle)
                    if (naturalFood != null)
                    {
                        Polygon foodTriangle = new Polygon
                        {
                            Fill = Brushes.LemonChiffon, 
                            Points = new PointCollection
                            {
                                new Point(xPos + cellWidth / 2, yPos + (cellHeight * 0.2)), 
                                new Point(xPos + (cellWidth * 0.9), yPos + (cellHeight * 0.9)), 
                                new Point(xPos + (cellWidth * 0.1), yPos + (cellHeight * 0.9)) 
                            }
                        };
                        canvas.Children.Add(foodTriangle);
                    }

                    // Draw Buildings (Farm = Chartreuse, FoodStore = Orange triangle)
                    if (farmEntity != null || foodStoreEntity != null)
                    {
                        Brush buildingColor = farmEntity != null ? Brushes.Chartreuse : Brushes.Orange;

                        Polygon triangle = new Polygon
                        {
                            Fill = buildingColor,
                            Points = new PointCollection
                            {
                                new Point(xPos + cellWidth / 2, yPos),          
                                new Point(xPos + cellWidth, yPos + cellHeight), 
                                new Point(xPos, yPos + cellHeight)              
                            }
                        };
                        canvas.Children.Add(triangle);
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

                    // Draw Eggs (3 overlapping white circles)
                    foreach (var egg in eggs)
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

                    // Draw Ants (Light Gray Circle normal, Yellow Target for Queen)
                    foreach (var ant in allAnts)
                    {
                        Brush entityBrush = ant is Queen ? Brushes.Yellow : Brushes.LightGray;
                        
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
            }
        }
    }
}