using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Data;
using TTP_EA.Specimen;

namespace TTP_EA.EA.Mutators
{
    public class InversionMutator<T> : IMutator<T> where T : ITTPSpecimen<T>
    {
        private double _mutationProbability;
        public double MutationProbability
        {
            get
            {
                return _mutationProbability;
            }
            set
            {
                _mutationProbability = value;
                probability = 1 - value;
            }
        }
        private readonly Random random;
        private double probability;

        public InversionMutator(double mutationProbability)
        {
            MutationProbability = mutationProbability;
            random = new Random();
            probability = 1 - MutationProbability;
        }

        public IList<T> Mutate(IList<T> currentPopulation)
        {
            foreach (T specimen in currentPopulation)
            {
                if (probability <= random.NextDouble())
                {
                    MutateSingleSpecimen(specimen);
                }
            }
            return currentPopulation;
        }

        public void MutateSingleSpecimen(T specimen)
        {
            var startIndex = random.Next(specimen.VisitedCities.Count);
            var length = random.Next(specimen.VisitedCities.Count - startIndex);
            var swappedCities = specimen.VisitedCities.GetRange(startIndex, length);
            swappedCities.Reverse();
            specimen.VisitedCities.RemoveRange(startIndex, length);
            specimen.VisitedCities.InsertRange(startIndex, swappedCities);
        }
    }
}
