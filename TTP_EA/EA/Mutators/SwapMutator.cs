using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Data;
using TTP_EA.Specimen;

namespace TTP_EA.EA.Mutators
{
    public class SwapMutator<T> : IMutator<T> where T : ITTPSpecimen<T>
    {
        private double _mutationProbability;
        public double MutationProbability {
            get
            {
                return _mutationProbability;
            } 
            set
            {
                _mutationProbability = value;
                probability = 1 - value;
            } }
        private double probability;
        private readonly Random random;

        public SwapMutator(double mutationProbability)
        {
            MutationProbability = mutationProbability;
            random = new Random();
            probability = 1 - MutationProbability;
        }

        public IList<T> Mutate(IList<T> currentPopulation)
        {
            foreach (T specimen in currentPopulation)
            {
                MutateSingleSpecimen(specimen);
            }
            return currentPopulation;
        }

        public void MutateSingleSpecimen(T specimen)
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
    }
}
