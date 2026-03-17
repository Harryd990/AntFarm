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
                Task nexttask = tasks[0];
                tasks.RemoveAt(0);
                return nexttask;
            }
            else
            {
                Random rand = new Random();
                int x = rand.Next(game.GridWidth);
                int y = rand.Next(0, game.GridHeight / 4);
                algorithm.Task wander = new algorithm.Task(lasttaskid++, "wander", (x, y));
                return wander;
            }
        }
        
    }
    // add priority queue for ant tasks but priorities should change dynamically based on colony state
}
