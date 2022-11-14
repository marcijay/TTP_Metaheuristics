using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Data;

namespace TTP_EA.Problem_Data
{
    public struct PathDistance
    {
        public City From { get; set; }
        public City To { get; set; }
        public double Distance { get; set; }
    }
}
