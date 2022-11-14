using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Data;
using TTP_EA.Specimen;

namespace TTP_EA.EA.Mutators
{
    public class InversionMutator : IMutator
    {
        public double MutationProbability { get; set; }

        public InversionMutator(double mutationProbability)
        {
            MutationProbability = mutationProbability;
        }

        public IList<TTP_Specimen> Mutate(IList<TTP_Specimen> currentPopulation)
        {
            Random random = new Random();
            var probability = 1 - MutationProbability;
            foreach (TTP_Specimen specimen in currentPopulation)
            {
                if (probability <= random.NextDouble())
                {
                    var startIndex = random.Next(specimen.VisitedCities.Count);
                    var length = random.Next(specimen.VisitedCities.Count - startIndex);
                    var swappedCities = specimen.VisitedCities.GetRange(startIndex, length);
                    swappedCities.Reverse();
                    specimen.VisitedCities.RemoveRange(startIndex, length);
                    specimen.VisitedCities.InsertRange(startIndex, swappedCities);
                }
            }
            return currentPopulation;
        }
    }
}
