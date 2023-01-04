using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Specimen;

namespace TTP_EA.EA.Crossovers
{
    public interface ICrossover<T> where T : ITTPSpecimen<T>
    {
        double CrossoverProbability { get; set; }
        IList<T> Crossover(IList<T> specimens);
    }
}
