using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTP_EA.Data
{
    public class Item
    {
        public int Index { get; set; }
        public int Profit { get; set; }
        public int Weight { get; set; }
        public int CityIndex { get; set; }
        public City CityWhereAvailable { get; set; }
        public Item()
        {
            Index = 0;
            Profit = 0;
            Weight = 0;
            CityIndex = 0;

            CityWhereAvailable = new City();
        }
        public Item(int index, int profit, int weight, int nodeIndex, City city)
        {
            Index = index;
            Profit = profit;
            Weight = weight;
            CityIndex = nodeIndex;
            CityWhereAvailable = city;
        }
    }
}
