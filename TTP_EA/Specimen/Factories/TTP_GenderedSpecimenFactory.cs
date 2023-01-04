using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP_EA.Data;

namespace TTP_EA.Specimen.Factories
{
    public class TTP_GenderedSpecimenFactory : ISpecimenFactory<TTP_Specimen_Gendered>
    {
        private readonly TTP_Data problemData;
        private readonly ISpecimenCreator<TTP_Specimen_Gendered> specimenCreator;

        public TTP_GenderedSpecimenFactory(TTP_Data problemData, ISpecimenCreator<TTP_Specimen_Gendered> specimenCreator)
        {
            this.problemData = problemData;
            this.specimenCreator = specimenCreator;
        }

        public TTP_Specimen_Gendered ProduceSpecimen(int index)
        {
            var specimen = new TTP_Specimen_Gendered(problemData, specimenCreator, index % 2 == 0);
            specimen.Init();
            return specimen;
        }
    }
}
