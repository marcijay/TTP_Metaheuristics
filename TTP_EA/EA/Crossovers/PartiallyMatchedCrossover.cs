using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Data;
using TTP_EA.Specimen;

namespace TTP_EA.EA.Crossovers
{
    public class PartiallyMatchedCrossover : ICrossover
    {
        public double CrossoverProbability { get; set; }

        public PartiallyMatchedCrossover(double crossoverProbability)
        {
            CrossoverProbability = crossoverProbability;
        }

        public IList<TTP_Specimen> Crossover(IList<TTP_Specimen> specimens)
        {
            var random = new Random();
            var prop = 1 - CrossoverProbability;
            var newSpecimens = new List<TTP_Specimen>();
            for (int i = 1; i < specimens.Count; i += 2)
            {
                if (prop <= random.NextDouble())
                {
                    var (specimen1, specimen2) = CrossSpecimens(specimens[i - 1], specimens[i]);
                    newSpecimens.Add(specimen1);
                    newSpecimens.Add(specimen2);
                }
                else
                {
                    newSpecimens.Add(specimens[i - 1].Clone());
                    newSpecimens.Add(specimens[i].Clone());
                }
            }
            if (specimens.Count % 2 == 0)
            {
                var specimen = specimens[specimens.Count - 1].Clone();
                newSpecimens.Add(specimen);
            }
            return newSpecimens;
        }

        private (TTP_Specimen, TTP_Specimen) CrossSpecimens(TTP_Specimen specimen, TTP_Specimen otherSpecimen)
        {
            var newSpecimen = specimen.Clone();
            var newOtherSpecimen = otherSpecimen.Clone();
            var random = new Random();
            var startIndex = random.Next(specimen.VisitedCities.Count);
            var length = random.Next(specimen.VisitedCities.Count - startIndex);
            var fromSpecimenRange = newSpecimen.VisitedCities.GetRange(startIndex, length);
            var fromOtherSpecimenRange = newOtherSpecimen.VisitedCities.GetRange(startIndex, length);

            var otherSpecimenCitiesOrdered = new List<City>();
            var specimenCitiesOrdered = new List<City>();

            for (int i = 0; i < specimen.VisitedCities.Count; i++)
            {
                if (!fromSpecimenRange.Contains(otherSpecimen.VisitedCities[i]))
                {
                    otherSpecimenCitiesOrdered.Add(otherSpecimen.VisitedCities[i]);
                }
                if (!fromOtherSpecimenRange.Contains(specimen.VisitedCities[i]))
                {
                    specimenCitiesOrdered.Add(specimen.VisitedCities[i]);
                }
            }
            int j = 0;
            for (int i = 0; i < specimen.VisitedCities.Count; i++)
            {
                if (i >= startIndex && j < length)
                {
                    newSpecimen.VisitedCities[i] = fromSpecimenRange[j];
                    newOtherSpecimen.VisitedCities[i] = fromOtherSpecimenRange[j];

                    j++;
                }
                else
                {
                    newSpecimen.VisitedCities[i] = otherSpecimenCitiesOrdered[0];
                    otherSpecimenCitiesOrdered.RemoveAt(0);

                    newOtherSpecimen.VisitedCities[i] = specimenCitiesOrdered[0];
                    specimenCitiesOrdered.RemoveAt(0);
                }
            }

            return (newSpecimen, newOtherSpecimen);
        }
    }
}
