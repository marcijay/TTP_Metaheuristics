using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Specimen;

namespace TTP_EA.EA.Selectors
{
    public interface ISelector
    {
        IList<TTP_Specimen> Select(IList<TTP_Specimen> currentPopulation);
    }
}
