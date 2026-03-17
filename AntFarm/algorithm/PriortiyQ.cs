using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm.algorithm
{
    internal class PriortiyQ
    {
        public PriortiyQ() { }
        public List<(int pos, int pri)> queue = new List<(int pos, int pri)>();

        public void Enqueue(int pos, int pri)
        {
            queue.Add((pos, pri));
        }
        public int Dequeue()
        {
            if (queue.Count == 0)
            {
                throw new Exception("the priortiy queue is empty");
            }
            int highestprindx = 0;
            for (int i = 1; i < queue.Count; i++)
            {
                if (queue[i].pri > queue[highestprindx].pri)
                {
                    highestprindx = i;
                }
            }
            int pos = queue[highestprindx].pos;
            queue.RemoveAt(highestprindx);
            return pos;
        }


    }
}
