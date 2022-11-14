using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTP_EA.Logger
{
    public class TSRecord : IRecord
    {
        public int Generation { get; set; }
        public double BestSpecimenScore { get; set; }
        public double AverageSpecimenScore { get; set; }
        public double CurrentSpecimenScore { get; set; }
        public double WorstSpecimenScore { get; set; }

        public TSRecord(int generation, double bestSpecimenScore, double averageSpecimenScore, double currentSpecimenScore, double worstSpecimenScore)
        {
            Generation = generation;
            BestSpecimenScore = bestSpecimenScore;
            AverageSpecimenScore = averageSpecimenScore;
            CurrentSpecimenScore = currentSpecimenScore;
            WorstSpecimenScore = worstSpecimenScore;
        }
    }
}
