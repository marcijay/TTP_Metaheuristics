using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Specimen;

namespace TTP_EA.EA.Mutators
{
    public interface IMutator
    {
        double MutationProbability { get; set; }
        IList<TTP_Specimen> Mutate(IList<TTP_Specimen> currentPopulation);
    }
}
