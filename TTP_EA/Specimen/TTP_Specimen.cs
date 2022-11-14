using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Data;
using TTP_EA.Specimen.Creators;

namespace TTP_EA.Specimen
{
    public class TTP_Specimen
    {
        public TTP_Data ProblemData { get; set; }
        public List<City> VisitedCities { get; set; }
        public HashSet<Item> TakenItems { get; set; }
        public ISpecimenCreator Creator { get; }
        public double KnapsackWeight { get; set; }
        public double? FitnessScore { get; set; }

        public TTP_Specimen(TTP_Data problemData, ISpecimenCreator creator)
        {
            ProblemData = problemData;
            VisitedCities = new List<City>();
            TakenItems = new HashSet<Item>();
            KnapsackWeight = 0;
            Creator = creator;
            FitnessScore = null;
        }

        public void Init()
        {
            Creator.Create(this);
        }

        public TTP_Specimen Clone()
        {
            TTP_Specimen clonedSpecimen = new TTP_Specimen(ProblemData, Creator);
            clonedSpecimen.VisitedCities = new List<City>(VisitedCities);
            clonedSpecimen.TakenItems = new HashSet<Item>(TakenItems);
            clonedSpecimen.KnapsackWeight = KnapsackWeight;

            return clonedSpecimen;
        }

        public override bool Equals(object? obj)
        {
            if (obj is TTP_Specimen)
            {
                return Equals((TTP_Specimen)obj);
            }
            return false;
        }

        public bool Equals(TTP_Specimen? other)
        {
            if (other == null)
            {
                return false;
            }
            if (VisitedCities.Count != other.VisitedCities.Count || TakenItems.Count != other.TakenItems.Count)
            {
                return false;
            }
            var cityCount = VisitedCities.Count;
            for (int i = 0; i < cityCount; i++)
            {
                if (VisitedCities[i] != other.VisitedCities[i])
                {
                    return false;
                }
            }
            foreach (var item in TakenItems)
            {
                //if(item.Value != other.TakenItems.GetValueOrDefault(item.Key))
                //{
                //    return false;
                //}
                if (!other.TakenItems.Contains(item))
                {
                    return false;
                }
            }
            return true;
        }

        public double GetTravelDistance()
        {
            double distance = 0;

            for(int i = 1; i < VisitedCities.Count; i++)
            {
                distance += ProblemData.GetDistance(VisitedCities[i - 1], VisitedCities[i]);

                if (i == VisitedCities.Count - 1)
                {
                    distance += ProblemData.GetDistance(VisitedCities[i], VisitedCities[0]);
                }
            }

            return distance;
        }

        public bool AddItemToKnapsack(Item item)
        {
            if (ProblemData.KnapsackCapacity >= KnapsackWeight + item.Weight)
            {
                TakenItems.Add(item);
                KnapsackWeight += item.Weight;
                return true;
            }
            return false;
        }

        public void RemoveItemFromKnapsack(Item item)
        {
            TakenItems.Remove(item);
            KnapsackWeight -= item.Weight;
        }

        public void RemoveAllItemsFromKnapsack()
        {
            TakenItems = new HashSet<Item>();
            KnapsackWeight = 0;
        }

        public bool IsItemIsInKnapsack(Item item)
        {
            return TakenItems.Contains(item);
        }

        public Item[] GetKnapsackItems()
        {
            return TakenItems.ToArray();
        }

        private void UpdateWeight(ref double weight, City city)
        {
            for (int i = 0; i < city.ItemsToTake.Count; i++)
            {
                var item = city.ItemsToTake[i];
                if (TakenItems.Contains(item))
                {
                    weight += item.Weight;
                }
            }
        }

        public double Score()
        {
            //return GetTravelDistance();

            if (FitnessScore.HasValue)
            {
                return FitnessScore.Value;
            }

            double profit = 0d;
            double time = 0d;
            double currentWeight = 0d;

            foreach (var item in TakenItems)
            {
                profit += item.Profit;
            }
            
            UpdateWeight(ref currentWeight, VisitedCities[0]);

            for (int i = 1; i < VisitedCities.Count; i++)
            {
                var distance = ProblemData.GetDistance(VisitedCities[i - 1], VisitedCities[i]);
                var currentSpeed = ProblemData.MaxSpeed - currentWeight * ((ProblemData.MaxSpeed - ProblemData.MinSpeed) / ProblemData.KnapsackCapacity);
                time += distance / currentSpeed;

                UpdateWeight(ref currentWeight, VisitedCities[i]);
            }

            FitnessScore = profit - time;

            return FitnessScore.Value;
        }

        public override int GetHashCode()
        {
            return -1;
        }
    }
}
