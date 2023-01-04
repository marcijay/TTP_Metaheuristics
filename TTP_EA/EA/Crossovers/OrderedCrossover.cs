using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Data;
using TTP_EA.Specimen;

namespace TTP_EA.EA.Crossovers
{
    public class OrderedCrossover<T> : ICrossover<T> where T : ITTPSpecimen<T>
    {
        public double CrossoverProbability { get; set; }
        private readonly Random random;

        public OrderedCrossover(double probability)
        {
            CrossoverProbability = probability;
            random = new Random();
        }

        public IList<T> Crossover(IList<T> specimens)
        {
            var probability = 1 - CrossoverProbability;
            var newSpecimens = new List<T>();
            for (int i = 0; i < specimens.Count - 1; i++)
            {
                if (probability <= random.NextDouble() && (specimens[i] is TTP_Specimen || specimens[i] is TTP_Specimen_Gendered && specimens[i + 1] is TTP_Specimen_Gendered && (specimens[i] as TTP_Specimen_Gendered).Gender != (specimens[i + 1] as TTP_Specimen_Gendered).Gender))
                {
                    var newSpecimen = CrossSpecimens(specimens[i], specimens[i + 1]);
                    newSpecimens.Add(newSpecimen);
                }
                else
                {
                    newSpecimens.Add(specimens[i].Clone());
                }
            }
            newSpecimens.Add(specimens[specimens.Count - 1].Clone());
            return newSpecimens;
        }

        private T CrossSpecimens(T specimen, T otherSpecimen)
        {
            var newSpecimen = otherSpecimen.Clone();
            var random = new Random();
            var startIndex = random.Next(specimen.VisitedCities.Count - 5);
            var length = 5;
            //Console.WriteLine($"Start index: {startIndex}, length: {length}");
            var cities = specimen.VisitedCities.GetRange(startIndex, length);
            var otherCitiesOrdered = new List<City>();
            foreach (var orderedCity in otherSpecimen.VisitedCities)
            {
                if (!cities.Contains(orderedCity))
                {
                    otherCitiesOrdered.Add(orderedCity);
                }
            }
            int j = 0;
            for (int i = 0; i < newSpecimen.VisitedCities.Count; i++)
            {
                if (i >= startIndex && j < length)
                {
                    newSpecimen.VisitedCities[i] = cities[j];
                    j++;
                }
                else
                {
                    newSpecimen.VisitedCities[i] = otherCitiesOrdered[0];
                    otherCitiesOrdered.RemoveAt(0);
                }
            }
            return newSpecimen;
        }
    }
}
