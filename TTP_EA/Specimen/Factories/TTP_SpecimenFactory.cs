using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Data;

namespace TTP_EA.Specimen.Factories
{
    public class TTP_SpecimenFactory : ISpecimenFactory<TTP_Specimen>
    {
        private readonly TTP_Data problemData;
        private readonly ISpecimenCreator<TTP_Specimen> specimenCreator;

        public TTP_SpecimenFactory(TTP_Data problemData, ISpecimenCreator<TTP_Specimen> specimenCreator)
        {
            this.problemData = problemData;
            this.specimenCreator = specimenCreator;
        }

        public TTP_Specimen ProduceSpecimen(int index)
        {
            var specimen = new TTP_Specimen(problemData, specimenCreator);
            specimen.Init();
            return specimen;
        }
    }
}
