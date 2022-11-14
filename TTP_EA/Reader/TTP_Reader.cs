using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Data;

namespace TTP_EA.Reader
{
    public static class TTP_Reader
    {
        public static TTP_Data? Load(string path)
        {
            string[] lines = File.ReadAllLines(path);
            try
            {
                var data = new TTP_Data();

                data.Name = lines[0].Split('\t')[1];
                data.NumberOfCities = int.Parse(lines[2].Split('\t')[1]);
                data.NumberOfItems = int.Parse(lines[3].Split('\t')[1]);
                data.KnapsackCapacity = int.Parse(lines[4].Split('\t')[1]);

                var decimalFormat = new NumberFormatInfo() { CurrencyDecimalSeparator = "." };

                data.MinSpeed = double.Parse(lines[5].Split('\t')[1], decimalFormat);
                data.MaxSpeed = double.Parse(lines[6].Split('\t')[1], decimalFormat);

                for (int i = 10; i < 10 + data.NumberOfCities; i++)
                {
                    var lineData = lines[i].Split('\t');
                    data.Cities.Add(new City(int.Parse(lineData[0])
                        , double.Parse(lineData[1], decimalFormat)
                        , double.Parse(lineData[2], decimalFormat)
                        )
                    );
                }
                for (int i = 11 + data.NumberOfCities; i < 11 + data.NumberOfItems + data.NumberOfCities; i++)
                {
                    var lineData = lines[i].Split('\t');
                    var cityIndex = int.Parse(lineData[3]);
                    var city = data.Cities.First(n => n.Index == cityIndex);
                    var item = new Item(int.Parse(lineData[0]), int.Parse(lineData[1]), int.Parse(lineData[2]), cityIndex, city);
                    data.Items.Add(item);
                    city.ItemsToTake.Add(item);
                }
                data.GenerateDistances();
                return data;
            }
            catch (Exception)
            {
                Console.Error.WriteLine("Unreadable data!");
                return null;
            }
        }
    }
}
