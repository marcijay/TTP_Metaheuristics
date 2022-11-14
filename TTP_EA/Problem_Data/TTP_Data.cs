using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Problem_Data;

namespace TTP_EA.Data
{
    public class TTP_Data
    {
        public string Name { get; set; }
        public int NumberOfCities { get; set; }
        public int NumberOfItems { get; set; }
        public int KnapsackCapacity { get; set; }
        public double MinSpeed { get; set; }
        public double MaxSpeed { get; set; }
        public List<City> Cities { get; set; }
        public List<Item> Items { get; set; }
        //public Dictionary<(City from, City to), double> Distances { get; set; }

        public PathDistance[][] Distances { get; set; }

        public PathDistance[][] GetDistanceMatrix()
        {
            return Distances;
        }

        public void GenerateDistances()
        {
            Distances = new PathDistance[NumberOfCities][];
            foreach (var city in Cities)
            {
                Distances[city.Index - 1] = new PathDistance[NumberOfCities];
                foreach (var target in Cities)
                {
                    var distance = Math.Sqrt(Math.Pow(city.X - target.X, 2) + Math.Pow(city.Y - target.Y, 2));
                    Distances[city.Index - 1][target.Index - 1] = new PathDistance()
                    {
                        Distance = distance,
                        From = city,
                        To = target
                    };
                }
            }
        }

        public double GetDistance(City first, City second)
        {
            return Distances[first.Index - 1][second.Index - 1].Distance;
        }

        public TTP_Data()
        {
            Cities = new List<City>();
            Items = new List<Item>();
            //Distances = new Dictionary<(City, City), double>();
            Distances = new PathDistance[0][];

            Name = "";
            NumberOfCities = 0;
            NumberOfItems = 0;
            KnapsackCapacity = 0;
            MinSpeed = 0;
            MaxSpeed = 0;
        }

        //public Dictionary<(City from, City to), double> GetDistances()
        //{
        //    if (Distances.Count != 0)
        //    {
        //        return Distances;
        //    }
        //    foreach (var city in Cities)
        //    {
        //        foreach (var other in Cities)
        //        {
        //            var distance = Math.Sqrt(Math.Pow(city.X - other.X, 2) + Math.Pow(city.Y - other.Y, 2));
        //            Distances.Add((city, other), distance);
        //        }
        //    }
        //    return Distances;
        //}
    }
}
