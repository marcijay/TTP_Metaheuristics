using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Data;
using TTP_EA.Specimen;

namespace TTP_EA.EA.Mutators
{
    public class SwapMutator : IMutator
    {
        public double MutationProbability { get; set; }

        public SwapMutator(double mutationProbability)
        {
            MutationProbability = mutationProbability;
        }

        public IList<TTP_Specimen> Mutate(IList<TTP_Specimen> currentPopulation)
        {
            Random random = new Random();
            double probability = 1 - MutationProbability;
            foreach (TTP_Specimen specimen in currentPopulation)
            {
                for (int i = 0; i < specimen.VisitedCities.Count; i++)
                {
                    if (probability <= random.NextDouble())
                    {
                        int index2 = random.Next(specimen.VisitedCities.Count);
                        (specimen.VisitedCities[index2], specimen.VisitedCities[i]) = (specimen.VisitedCities[i], specimen.VisitedCities[index2]);
                    }
                }
            }
            return currentPopulation;
        }
    }
}
