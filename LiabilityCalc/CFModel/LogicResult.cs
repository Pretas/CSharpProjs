using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiabilityCalc
{
    public partial class ModelBase
    {
        protected virtual double GetGmxbClaimDec() { return 0.0; }
        protected virtual double GetGmxbClaimDecPV() { return 0.0; }

        protected void SetResult(OutputManager outs)
        {
            // 결과 : mth, accountValue, fee, claim, pv_fee, pv_claim
            double[] resLine = new double[5];
            resLine[0] = v.Navs.Sum() * v.LivesEOP;
            resLine[1] = v.GmxbFee * v.LivesBOP;
            resLine[2] = GetGmxbClaimDec();
            resLine[3] = v.GmxbFee * v.LivesBOP * v.DiscountedFactorBOP;
            resLine[4] = GetGmxbClaimDecPV();

            outs.SetMthlyValues(Pol.Rec.ContNo, SM.CurrentScenNo, p.ProjMth, resLine);

            if (printsThisProjection) SetMonthlyResult(p, v);
        }

        protected void SetMonthlyResult(ProjParams p, ProjVars v)
        {
            if (p.ProjMth == 1)
            {
                MthlyVars = new List<object[]>();

                List<object> names = new List<object>();
                names.AddRange(GetObjNames(p));
                names.AddRange(GetObjNames(v));
                MthlyVars.Add(names.ToArray());
            }

            List<object> vals = new List<object>();
            vals.AddRange(GetObjValues(p));
            vals.AddRange(GetObjValues(v));
            MthlyVars.Add(vals.ToArray());
        }

        public static List<object> GetObjNames(object obj)
        {
            var fields = obj.GetType().GetFields();

            List<object> names = new List<object>();

            for (int i = 0; i < fields.Length; i++)
            {
                string name = fields[i].Name;

                if (!fields[i].FieldType.IsArray)
                    names.Add(name);
                else
                {
                    var arr = fields[i].GetValue(obj) as Array;

                    for (int j = 0; j < arr.Length; j++) names.Add(name + "_" + j.ToString());
                }
            }

            return names;
        }

        public static List<object> GetObjValues(object obj)
        {
            var fields = obj.GetType().GetFields();

            List<object> vals = new List<object>();

            for (int i = 0; i < fields.Length; i++)
            {
                if (!fields[i].FieldType.IsArray)
                    vals.Add(fields[i].GetValue(obj));
                else
                {
                    var arr = fields[i].GetValue(obj) as Array;

                    for (int j = 0; j < arr.Length; j++) vals.Add(arr.GetValue(j));
                }
            }

            return vals;
        }
    }
}
