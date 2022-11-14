using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTP_EA.Logger
{
    public struct TestInstanceData
    {
        public string InstanceName { get; set; }
        public double MaxScore { get; set; }
        public double MinScore { get; set; }
        public double AvgScore { get; set; }
        public double StdDeviation { get; set; }
        public double AvgSingleRunSeconds { get; set; }
    }
}
