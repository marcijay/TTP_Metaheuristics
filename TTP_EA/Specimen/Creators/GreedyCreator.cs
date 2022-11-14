using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Data;

namespace TTP_EA.Specimen.Creators
{
    public class GreedyCreator : ISpecimenCreator
    {
        public TTP_Data ProblemData { get; set; }

        public GreedyCreator(TTP_Data problemData)
        {
            ProblemData = problemData;
        }
        public void Create(TTP_Specimen specimen)
        {
            HashSet<City> cities = ProblemData.Cities.ToHashSet();
            var distances = ProblemData.GetDistanceMatrix();
            var random = new Random();
            var currentCity = ProblemData.Cities[random.Next(0, cities.Count)];
            cities.Remove(currentCity);
            specimen.VisitedCities.Add(currentCity);

            while (cities.Count > 0)
            {
                var paths = distances[currentCity.Index - 1];
                double maxDistance = -1d;
                var selectedPath = paths[0];

                foreach (var path in paths)
                {
                    if (path.From != path.To && cities.Contains(path.To) && maxDistance < path.Distance)
                    {
                        selectedPath = path;
                        maxDistance = path.Distance;
                    }
                }
                currentCity = selectedPath.To;
                cities.Remove(currentCity);
            }
            FillGreedyKnapsack(specimen);
        }

        public static void FillGreedyKnapsack(TTP_Specimen specimen)
        {
            specimen.RemoveAllItemsFromKnapsack();
            var itemValues = new Dictionary<Item, double>();
            var pathLength = specimen.ProblemData.GetDistance(specimen.VisitedCities[0], specimen.VisitedCities[specimen.VisitedCities.Count - 1]);

            for (int i = specimen.VisitedCities.Count - 1; i > 0; i--)
            {
                foreach (var item in specimen.VisitedCities[i].ItemsToTake)
                {
                    itemValues.Add(item, item.Profit / (item.Weight * pathLength));
                    //itemValues.Add(item, item.Profit - pathLength);
                }
                pathLength += specimen.ProblemData.GetDistance(specimen.VisitedCities[i], specimen.VisitedCities[i - 1]);
            }
            itemValues = itemValues.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            while (itemValues.Count > 0)
            {
                specimen.AddItemToKnapsack(itemValues.Keys.First());
                itemValues.Remove(itemValues.Keys.First());
            }
        }
    }
}
