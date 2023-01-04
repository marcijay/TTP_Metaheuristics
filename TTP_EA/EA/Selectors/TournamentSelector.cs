using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Specimen;

namespace TTP_EA.EA.Selectors
{
    public class TournamentSelection<T> : ISelector<T> where T : ITTPSpecimen<T>
    {
        public int SpecimenCount { get; set; }
        public bool IsMinimalizing { get; set; }

        public TournamentSelection(int specimenCount, bool isMinimalizing)
        {
            SpecimenCount = specimenCount;
            IsMinimalizing = isMinimalizing;
        }

        public virtual IList<T> Select(IList<T> currentPopulation)
        {
            Random random = new Random();
            List<T> selectedSpecimens = new List<T>();
            while (selectedSpecimens.Count != currentPopulation.Count)
            {
                List<T> tournamentSelectedSpecimens = new List<T>();
                for (int j = 0; j < SpecimenCount; j++)
                {
                    var index = random.Next(currentPopulation.Count);
                    tournamentSelectedSpecimens.Add(currentPopulation[index]);
                }
                if (IsMinimalizing)
                {
                    selectedSpecimens.Add(tournamentSelectedSpecimens.MinBy(Evaluate).Clone());
                }
                else
                {
                    selectedSpecimens.Add(tournamentSelectedSpecimens.MaxBy(Evaluate).Clone());
                }
            }
            return selectedSpecimens;
        }

        private double Evaluate(T specimen)
        {
            return specimen.Score();
        }
    }
}
