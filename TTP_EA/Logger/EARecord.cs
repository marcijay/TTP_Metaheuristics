using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTP_EA.Logger
{
    public class EARecord : IRecord
    {
        public int Generation { get; set; }
        public double MaxSpecimenScore { get; set; }
        public double MinSpecimenScore { get; set; }
        public double AverageSpecimenScore { get; set; }

        public EARecord(int generation, double maxSpecimenScore, double minSpecimenScore, double averageSpecimenScore)
        {
            Generation = generation;
            MaxSpecimenScore = maxSpecimenScore;
            MinSpecimenScore = minSpecimenScore;
            AverageSpecimenScore = averageSpecimenScore;
        }
    }
}
