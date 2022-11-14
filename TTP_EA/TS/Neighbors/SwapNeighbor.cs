using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Specimen;

namespace TTP_EA.TS.Neighbors
{
    public class SwapNeighbor : INeighbor
    {
        public TTP_Specimen Change(TTP_Specimen specimen)
        {
            Random random = new Random();
            var index = random.Next(specimen.VisitedCities.Count);
            var index2 = random.Next(specimen.VisitedCities.Count);
            (specimen.VisitedCities[index2], specimen.VisitedCities[index]) = (specimen.VisitedCities[index], specimen.VisitedCities[index2]);

            return specimen;
        }
    }
}
