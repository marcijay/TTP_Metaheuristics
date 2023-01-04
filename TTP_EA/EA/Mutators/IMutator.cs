using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Specimen;

namespace TTP_EA.EA.Mutators
{
    public interface IMutator<T> where T : ITTPSpecimen<T>
    {
        double MutationProbability { get; set; }
        IList<T> Mutate(IList<T> currentPopulation);
        void MutateSingleSpecimen(T specimen);
    }
}
