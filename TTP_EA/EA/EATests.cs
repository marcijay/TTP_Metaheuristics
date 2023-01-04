using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Data;
using TTP_EA.Reader;
using TTP_EA.Specimen;
using TTP_EA.Specimen.Creators;

namespace TTP_EA.EA
{
    public static class Tests
    {
        public static void Test_Lab_1()
        {
            var filePath = "./../../../Data/trivial_0.ttp";

            var trivialData = TTP_Reader.Load(filePath);

            if (trivialData is not null)
            {
                Console.WriteLine($"Problem name: {trivialData.Name}");
                Console.WriteLine($"Number of cities: {trivialData.NumberOfCities}");
                Console.WriteLine($"Number of items: {trivialData.NumberOfItems}");
                Console.WriteLine($"Knapsack capacity: {trivialData.KnapsackCapacity}\n");

                Console.WriteLine("Cites:\n");

                foreach (City city in trivialData.Cities)
                {
                    Console.WriteLine($"Index: {city.Index}, X: {city.X}, Y: {city.Y}");
                }
                Console.WriteLine("\nItems:\n");
                foreach (Item item in trivialData.Items)
                {
                    Console.WriteLine($"Index: {item.Index}, value: {item.Profit}, weight: {item.Weight}");
                }

                double itemTakeProbalility = 0.2;
                RandomCreator<TTP_Specimen> randomCreator = new RandomCreator<TTP_Specimen>(trivialData, itemTakeProbalility);
                TTP_Specimen randomSpecimen = new TTP_Specimen(trivialData, randomCreator);

                Console.WriteLine($"\nRandom specimen knapsack weight before init: {randomSpecimen.KnapsackWeight}");
                Console.WriteLine($"Random travel distance before init: {randomSpecimen.GetTravelDistance()}");

                randomSpecimen.Init();

                Console.WriteLine($"Random specimen knapsack weight after init: {randomSpecimen.KnapsackWeight}");
                Console.WriteLine($"Random travel distance after init: {randomSpecimen.GetTravelDistance()}");

                GreedyCreator<TTP_Specimen> greedyCreator = new GreedyCreator<TTP_Specimen>(trivialData);
                TTP_Specimen greedySpecimen = new TTP_Specimen(trivialData, greedyCreator);

                Console.WriteLine($"\nGreedy specimen knapsack weight before init: {greedySpecimen.KnapsackWeight}");
                Console.WriteLine($"Greedy travel distance before init: {greedySpecimen.GetTravelDistance()}");

                greedySpecimen.Init();

                Console.WriteLine($"Greedy specimen knapsack weight after init: {greedySpecimen.KnapsackWeight}");
                Console.WriteLine($"Greedy travel distance after init: {greedySpecimen.GetTravelDistance()}");
            }
        }

        public static void Test_Crossover()
        {
            var filePath = "./../../../Data/trivial_0.ttp";
            var trivialData = TTP_Reader.Load(filePath);

            ISpecimenCreator<TTP_Specimen> creator = new RandomCreator<TTP_Specimen>(trivialData, 0.3);

            TTP_Specimen randomSpecimen = new TTP_Specimen(trivialData, creator);
            randomSpecimen.Init();
            string cities = "";
            foreach (var city in randomSpecimen.VisitedCities)
            {
                cities += $"{city.Index - 1} ";
            }
            Console.WriteLine(cities);

            TTP_Specimen randomSpecimen2 = new TTP_Specimen(trivialData, creator);
            randomSpecimen2.Init();
            cities = "";
            foreach (var city in randomSpecimen2.VisitedCities)
            {
                cities += $"{city.Index - 1} ";
            }
            Console.WriteLine(cities);
            Console.WriteLine("\n");

            (TTP_Specimen crossedSpecimen, TTP_Specimen crossedSpecimen2) = CrossSpecimens(randomSpecimen, randomSpecimen2);

            cities = "";
            foreach (var city in crossedSpecimen.VisitedCities)
            {
                cities += $"{city.Index - 1} ";
            }
            Console.WriteLine(cities);

            cities = "";
            foreach (var city in crossedSpecimen2.VisitedCities)
            {
                cities += $"{city.Index - 1} ";
            }
            Console.WriteLine(cities);

            //TTP_Specimen crossedSpecimen = CrossSpecimens(randomSpecimen, randomSpecimen2);

            //cities = "";
            //foreach (var city in crossedSpecimen.VisitedCities)
            //{
            //    cities += $"{city.Index - 1} ";
            //}
            //Console.WriteLine(cities);

        }

        public static (TTP_Specimen, TTP_Specimen) CrossSpecimens(TTP_Specimen specimen, TTP_Specimen otherSpecimen)
        {
            var newSpecimen = specimen.Clone();
            var newOtherSpecimen = otherSpecimen.Clone();
            var random = new Random();
            var startIndex = random.Next(specimen.VisitedCities.Count);
            var length = random.Next(specimen.VisitedCities.Count - startIndex);
            var fromSpecimenRange = newSpecimen.VisitedCities.GetRange(startIndex, length);
            var fromOtherSpecimenRange = newOtherSpecimen.VisitedCities.GetRange(startIndex, length);

            Console.WriteLine($"Start index {startIndex}, length {length}");

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
