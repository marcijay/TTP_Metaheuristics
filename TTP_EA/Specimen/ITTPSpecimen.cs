using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Data;

namespace TTP_EA.Specimen
{
    public interface ITTPSpecimen<T> where T : ITTPSpecimen<T>
    {
        public ISpecimenCreator<T> Creator { get; }
        public double? FitnessScore { get; set; }
        public TTP_Data ProblemData { get; set; }
        public List<City> VisitedCities { get; set; }
        public HashSet<Item> TakenItems { get; set; }
        public double KnapsackWeight { get; set; }

        public double Score();
        public void Init();
        public bool AddItemToKnapsack(Item item);
        public void RemoveAllItemsFromKnapsack();

        T Clone();
    }
}
