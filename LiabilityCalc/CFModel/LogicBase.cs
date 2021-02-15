using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiabilityCalc
{
    public partial class ModelBase
    {
        protected int GetProjMth(Policy pol)
        {
            if (pol.Rec.ProdCode < 4)
            {
                return (pol.Rec.StartAgeOfSomething - pol.Rec.ContAge) * 12;
            }
            else
            {
                return (EndAge - pol.Rec.ContAge) * 12;
            }
        }
    }
}
