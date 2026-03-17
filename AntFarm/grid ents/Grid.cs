using AntFarm.entetys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm
{
    internal class Grid
    {
        Random random = new Random();

        // properties
        public int width {  get; private set; }
        public int height { get; private set; }
        private Cell[,] cells;
        
        public Grid(int w, int h) 
        { 
            width = w;
            height = h;
            //create array of empty slots for cells.
            cells = new Cell[w, h];

            // create the cell objects in each slot
            for (int x=0; x<w; x++)
            {
                // makes top quater air 
                for (int y=0; y<h/4; y++)
                {
                    cells[x, y] = new Air(x, y);
                }
                // makes rest dirt and some stone randomly
                for (int y=h/4; y<h/2; y++)
                {
                    int num = random.Next(15);
                    if (num == 0) 
                    {
                        cells[x, y] = new stone(x, y);
                    }
                    else
                    {
                        cells[x, y] = new Dirt(x, y);
                    }
                        
                }
                // deeper down more stone
                for (int y = h / 2; y < h; y++)
                {
                    int num = random.Next(2);
                    if (num == 0)
                    {
                        cells[x, y] = new stone(x, y);
                    }
                    else
                    {
                        cells[x, y] = new Dirt(x, y);
                    }

                }
            }
        }

        public Cell GetCellAtLocation(int x, int y)
        {
            if (IsInGridRange(x,y)) return cells[x, y];
            else throw new InvalidOperationException("Not in range");
                
        }
        public void AddEntityToCellLocation(int x, int y, Entity thing)
        {
   
            cells[x,y].AddEntity(thing);
            thing.Position = (x, y);
        }
        public bool IsInGridRange(int x, int y)
        {
            //check if x,y is in range
            return !((x < 0 || y < 0) || (x >= width || y >= height));
   
        }

        public string GetCellDetails(int x, int y)
        {
            string output = $"Cell {x},{y}\nCell Contents: \n";
            foreach (Entity e in GetCellAtLocation(x, y).Entities)
            {
                output += $"ID : {e.Id} Species: {e.Species}";
            }
            return output;
        }
        public void PrintGrid()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Console.Write(cells[x, y].Symbol);
                }
                Console.WriteLine();
            }
        }
        public void ConvertToAir(int x, int y)
        {
            cells[x, y] = new Air(x, y);
        }
        public void ReplaceCellAtLocation(int x, int y, Cell newCell)
        {
            var oldCell = cells[x, y];
            var entitiesToMove = oldCell.Entities.ToList();
            foreach (var entity in entitiesToMove)
            {
                newCell.AddEntity(entity);
            }
            
            cells[x, y] = newCell;
            newCell.X = x;
            newCell.Y = y;
        }


    }
}

