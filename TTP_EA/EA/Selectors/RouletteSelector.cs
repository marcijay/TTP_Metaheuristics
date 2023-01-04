using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Specimen;

namespace TTP_EA.EA.Selectors
{
    public class RouletteSelection<T> : ISelector<T> where T : ITTPSpecimen<T>
    {
        public bool IsMinimalizing { get; set; }

        public RouletteSelection(bool isMinimalizing)
        {
            IsMinimalizing = isMinimalizing;
        }

        public virtual IList<T> Select(IList<T> currentPopulation)
        {
            Dictionary<T, (double from, double to)> weightedSpecimens = new Dictionary<T, (double from, double to)>();
            var sum = 0d;
            var min = double.MaxValue;
            var max = double.MinValue;
            foreach (var specimen in currentPopulation)
            {
                var score = specimen.Score();
                if (score > max)
                {
                    max = score;
                }
                else if (score < min)
                {
                    min = score;
                }
            }
            foreach (var specimen in currentPopulation)
            {
                var score = specimen.Score();
                var normalizedScore = Normalize(score, max, min);
                weightedSpecimens.Add(specimen, (sum, sum + normalizedScore));
                sum += normalizedScore;
            }
            Random random = new Random();
            List<T> selectedSpecimens = new List<T>();
            for (int i = 0; i < currentPopulation.Count; i++)
            {
                var value = random.NextDouble() * sum;
                var specimen = weightedSpecimens.First(ws => ws.Value.from <= value && ws.Value.to > value);
                selectedSpecimens.Add(specimen.Key.Clone());
            }
            return selectedSpecimens;
        }

        private double Normalize(double value, double max, double min)
        {
            if (min == max)
            {
                return 1;
            }
            if (IsMinimalizing)
            {
                return (max - value) / (max - min);
            }
            return (value - min) / (max - min);
        }
    }
}
