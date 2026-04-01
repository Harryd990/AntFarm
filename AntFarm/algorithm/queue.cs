using AntFarm.entetys;
using AntFarm.main;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm.algorithm
{
    public class queue
    {
        public queue()
        {
        }
        
        public List<Task> tasks = new List<Task>();
        public int lasttaskid = 0;

        public void addtask(Task newtask)
        {
            tasks.Add(newtask);
        }

        public void removetask(int taskid)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                if (tasks[i].id == taskid)
                {
                    tasks.RemoveAt(i);
                    i--;
                }
            }
        }

        // NOTE: this method no longer mutates the ant (does not set ant.clamedtaskid).
        // The caller should validate and then claim the task on the ant.
        public Task getnexttask(Game game, Ant ant)
        {
            /*
             * Keep priority logic here if needed.
             * Important: return the next task (and remove it from internal list) but do NOT modify the ant.
             */

            if (tasks.Count > 0)
            {
                // Look for the first task that is reachable
                for (int i = 0; i < tasks.Count; i++)
                {
                    Task t = tasks[i];

                    // If it's a dig command, ensure there is at least one adjacent traversable cell to stand on
                    if (string.Equals(t.tasktype, "dig", StringComparison.OrdinalIgnoreCase))
                    {
                        var (tx, ty) = t.targetposition;
                        bool isReachable = false;
                        
                        (int dx, int dy)[] dirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };
                        foreach (var d in dirs)
                        {
                            
                            int nx = tx + d.dx; 
                            int ny = ty + d.dy;
                            
                            if (game.grid.IsInGridRange(nx, ny) && game.grid.GetCellAtLocation(nx, ny).IsTraversable)
                            {
                                isReachable = true;
                                break;
                            }
                        }

                        // If all surrounding cells are solid dirt/stone, skip this task. 
                        // It will remain in the queue until an adjacent block is dug out.
                        if (!isReachable) continue; 
                    }

                    // Found a valid/reachable task! Remove and dispatch it.
                    tasks.RemoveAt(i);
                    return t;
                }
            }

            // Fall back to wandering if there are no tasks, or if all tasks are currently unreachable
            Random rand = new Random();
            int x = rand.Next(game.GridWidth);
            int y = rand.Next(0, game.GridHeight / 4);
            algorithm.Task wander = new algorithm.Task(lasttaskid++, "wander", (x, y));
            return wander;
        }
        
    }
    // add priority queue for ant tasks but priorities should change dynamically based on colony state
}
