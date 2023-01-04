using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Data;
using TTP_EA.EA.Crossovers;
using TTP_EA.EA.Mutators;
using TTP_EA.EA.Selectors;
using TTP_EA.Logger;
using TTP_EA.Specimen;

namespace TTP_EA.EA
{
    public interface IEvolutionaryAlgorithm<T> where T : ITTPSpecimen<T>
    {
        public uint PopulationSize { get; set; }
        public uint Generation { get; set; }
        public uint GenerationsToCheck { get; set; }
        public TTP_Data ProblemData { get; set; }
        public CSV_Logger<EARecord> Logger { get; set; }

        public T Execute();
        public void NewPopulation();
        public void InitializePopulation();
    }
}
