using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTP_EA.Specimen.Factories
{
    public  interface ISpecimenFactory<T> where T : ITTPSpecimen<T>
    {
        T ProduceSpecimen(int index);
    }
}
