using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Specimen;

namespace TTP_EA.EA.Crossovers
{
    public interface ICrossover
    {
        double CrossoverProbability { get; set; }
        IList<TTP_Specimen> Crossover(IList<TTP_Specimen> specimens);
    }
}
