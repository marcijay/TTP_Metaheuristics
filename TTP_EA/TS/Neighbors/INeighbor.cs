using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Specimen;

namespace TTP_EA.TS.Neighbors
{
    public interface INeighbor
    {
        TTP_Specimen Change(TTP_Specimen specimen);
    }
}
