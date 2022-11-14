using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Specimen;

namespace TTP_EA.EA.Selectors
{
    public class TournamentSelection : ISelector
    {
        public int SpecimenCount { get; set; }
        public bool IsMinimalizing { get; set; }

        public TournamentSelection(int specimenCount, bool isMinimalizing)
        {
            SpecimenCount = specimenCount;
            IsMinimalizing = isMinimalizing;
        }

        public virtual IList<TTP_Specimen> Select(IList<TTP_Specimen> currentPopulation)
        {
            Random random = new Random();
            List<TTP_Specimen> selectedSpecimens = new List<TTP_Specimen>();
            while (selectedSpecimens.Count != currentPopulation.Count)
            {
                List<TTP_Specimen> tournamentSelectedSpecimens = new List<TTP_Specimen>();
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

        private double Evaluate(TTP_Specimen specimen)
        {
            return specimen.Score();
        }
    }
}
