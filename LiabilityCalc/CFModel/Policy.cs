using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiabilityCalc;

namespace LiabilityCalc
{
    public class Record
    { 
        public string ID;
        public int ContNo;
        public int ElapsedMth;
        public int ContAge;
        public int Sex;
        public string ProdSeg;
        public int ProdCode;
        public double Prem;
        public int PremYr;
        public double AccumPrem;
        public double ContAmt;
        public int StartAgeOfSomething;
        public double SumFund;
        public double FundVal01;
        public double FundVal02;
        public double FundVal03;
        public double FundAllo01;
        public double FundAllo02;
        public double FundAllo03;

        //public static Record GetSample()
        //{
        //    var sample = new Record();
        //    sample.ContNo = 0;
        //    sample.ElapsedMth = 0;
        //    sample.ContAge = 25;
        //    sample.Sex = 1;
        //    sample.ProdSeg = (int)EnumProdSeg.Life;
        //    sample.ProdCode = (int)EnumProdCode.Life_Fixed_AA0_Bonus0;
        //    sample.Prem = 5;
        //    sample.PremYear = 10;
        //    sample.ContAmt = 1000.0;

        //    sample.FundNAVs[0] = 25; sample.FundNAVs[1] = 15; sample.FundNAVs[2] = 10;
        //    sample.PremAlloRates[0] = 0.5; sample.PremAlloRates[1] = 0.3; sample.PremAlloRates[2] = 0.2;
            
        //    return sample;
        //}
    }

    public class Policy
    {
        public Record Rec { get; private set; }
        public Dictionary<int, double> MortRates { get; private set; }
        public Dictionary<string, Dictionary<int, double>> LapseRates { get; private set; }

        public Policy(Record rec, TableManager tm)
        {
            this.Rec = rec;
            SetRates(tm);
        }

        public void SetRates(TableManager tm)
        {
            //string vk = $@"{Rec.ProdSeg}_{Rec.Sex.ToString()}";
            MortRates = tm.Mort.Where(x => (x.VarKey == Rec.ProdSeg) && (x.VarValue == Rec.Sex.ToString())).First().Vals;

            LapseRates = tm.Lapse.Where(x => x.VarKey == Rec.ProdSeg).ToDictionary(x=>x.VarValue, x=>x.Vals);
        }

        public static List<Policy> GetPolicies(List<Record> recs, TableManager tm)
        {
            var res = new List<Policy>();

            foreach (var rec in recs)
            {
                res.Add(new Policy(rec, tm));
            }

            return res;
        }

    }
}