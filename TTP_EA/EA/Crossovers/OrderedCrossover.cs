using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Data;
using TTP_EA.Specimen;

namespace TTP_EA.EA.Crossovers
{
    public class OrderedCrossover : ICrossover
    {
        public double CrossoverProbability { get; set; }

        public OrderedCrossover(double probability)
        {
            CrossoverProbability = probability;
        }

        public IList<TTP_Specimen> Crossover(IList<TTP_Specimen> specimens)
        {
            var random = new Random();
            var probability = 1 - CrossoverProbability;
            var newSpecimens = new List<TTP_Specimen>();
            for (int i = 0; i < specimens.Count - 1; i++)
            {
                if (probability <= random.NextDouble())
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

        private TTP_Specimen CrossSpecimens(TTP_Specimen specimen, TTP_Specimen otherSpecimen)
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
