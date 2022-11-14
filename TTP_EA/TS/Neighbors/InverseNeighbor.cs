using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Specimen;

namespace TTP_EA.TS.Neighbors
{
    public class InverseNeighbor : INeighbor
    {
        public TTP_Specimen Change(TTP_Specimen specimen)
        {
            Random random = new Random();
            var startIndex = random.Next(specimen.VisitedCities.Count);
            var length = random.Next(specimen.VisitedCities.Count - startIndex);
            var swappedCities = specimen.VisitedCities.GetRange(startIndex, length);
            swappedCities.Reverse();
            specimen.VisitedCities.RemoveRange(startIndex, length);
            specimen.VisitedCities.InsertRange(startIndex, swappedCities);

            return specimen;
        }
    }
}
