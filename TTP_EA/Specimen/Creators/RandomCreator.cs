using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Data;

namespace TTP_EA.Specimen
{
    public class RandomCreator<T> : ISpecimenCreator<T> where T : ITTPSpecimen<T>
    {
        public TTP_Data ProblemData { get; set; }
        public double ItemAddProbability { get; set; }

        public RandomCreator(TTP_Data problemData, double itemAddProbability)
        {
            ProblemData = problemData;
            ItemAddProbability = itemAddProbability;
        }

        public void Create(T specimen)
        {
            List<City> citiesToAdd = ProblemData.Cities.ToList();

            Random random = new Random();

            while(citiesToAdd.Count > 0)
            {
                City city = citiesToAdd[random.Next(0, citiesToAdd.Count)];
                specimen.VisitedCities.Add(city);
                double prob = 1 - ItemAddProbability;

                if(prob <= random.NextDouble() && city.ItemsToTake.Count > 0)
                {
                    specimen.AddItemToKnapsack(city.ItemsToTake[random.Next(city.ItemsToTake.Count)]);
                }
                citiesToAdd.Remove(city);
            }
        }
    }
}
