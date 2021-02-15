using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiabilityCalc
{
    public partial class ModelBase
    {
        protected void AddUpToNavByNavAllo(double amt)
        {
            double sumNav = v.Navs.Sum();

            if (sumNav > SmallDouble)
            {
                for (int i = 0; i < v.FundsCnt; i++)
                {
                    v.Navs[i] = Math.Max(0.0, v.Navs[i] + (amt * v.Navs[i] / sumNav));
                }
            }
            else
                AddUpToNavByPremAllo(amt);
        }

        protected void AddUpToNavByPremAllo(double amt)
        {
            double sumNav = v.Navs.Sum();
            for (int i = 0; i < v.FundsCnt; i++)
            {
                v.Navs[i] = Math.Max(0.0, v.Navs[i] + (amt * v.PremAlloRates[i]));
            }
        }
    }
}
