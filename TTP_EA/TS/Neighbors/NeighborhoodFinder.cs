using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Specimen;
using TTP_EA.Specimen.Creators;

namespace TTP_EA.TS.Neighbors
{
    public class NeighborhoodFinder
    {
        public INeighbor Neighbor { get; set; }
        public NeighborhoodFinder(INeighbor neighbor)
        {
            Neighbor = neighbor;
        }

        public IEnumerable<TTP_Specimen> FindNeighborhood(TTP_Specimen specimen, int size)
        {
            var neighborhood = new List<TTP_Specimen>();
            for (int i = 0; i < size; i++)
            {
                var newSpecimen = Neighbor.Change(specimen.Clone());
                GreedyCreator.FillGreedyKnapsack(newSpecimen);
                neighborhood.Add(newSpecimen);
            }
            return neighborhood;
        }
    }
}
