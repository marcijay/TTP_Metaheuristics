using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTP_EA.Specimen
{
    public interface ISpecimenCreator<T> where T : ITTPSpecimen<T>
    {
        void Create(T specimen); 
    }
}
