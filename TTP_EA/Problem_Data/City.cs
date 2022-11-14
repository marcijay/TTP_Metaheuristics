using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTP_EA.Data
{
    public class City
    {
        public int Index { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public List<Item> ItemsToTake { get; set; }

        public City()
        {
            Index = 0;
            X = 0;
            Y = 0;
            ItemsToTake = new List<Item>();
        }

        public City(int index, double x, double y)
        {
            Index = index;
            X = x;
            Y = y;
            ItemsToTake = new List<Item>();
        }
    }
}
