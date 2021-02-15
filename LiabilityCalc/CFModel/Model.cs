using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace LiabilityCalc
{
    public partial class ModelBase 
    {
        public class ProjParams
        {
            public int ProjMth;
            public int CurrentMth;
            public int CurrentYr;
            public int CurrentAge;
            public int ProjPeriod;
        }

        public class ProjVars
        {
            public double Prem;
            public double LoadingAlpha;
            public double LoadingBeta;
            public double LoadingGamma;

            public double GmxbFee;
            public double GmxbClaim;

            public double MortR;
            public double LapseR;

            public double DiscountedFactorBOP = 1.0;
            public double DiscountedFactorEOP = 1.0;

            public double LivesBOP = 1.0;
            public double LivesEOP = 1.0;

            public double MortPeople;
            public double LapsePeople;

            public double AccumPrem;

            public bool IsRebalancing = false;
            public double Bonus;

            public int FundsCnt;
            public double[] Navs;
            public double[] PremAlloRates;
        }

        public readonly double SmallDouble = 0.000000001;
        public readonly int EndAge = 100;

        protected RunSettings RunP;
        protected Policy Pol;
        protected ScenarioManager SM;

        protected Product Prd;
        protected ProjParams p;
        private ProjVars v;

        protected double[] Dscrt;
        protected Dictionary<int, double[]> AssetYield;

        protected Result ResultVals = null;
        //protected double[][] ResultVals = null;

        protected bool printsThisProjection = false;
        protected List<object[]> MthlyVars;
        protected int MaxAge;

        public static ModelBase GetSelectedModel(RunSettings rs, Policy p, ScenarioManager sm)
        {
            if (p.Rec.ProdCode < 4) return new ModelAnnuityRop(rs, p, sm, new ModelAnnuity.ProjVarsAnnuity());
            else if (p.Rec.ProdCode < 8) return new ModelAnnuityGLWB(rs, p, sm, new ModelAnnuityGLWB.ProjVarsAnnuityGLWB());
            else if (p.Rec.ProdCode < 12) return new ModelLifeFixed(rs, p, sm, new ModelLife.ProjVarsLife());
            else return new ModelLifeIncreasing(rs, p, sm, new ModelLifeIncreasing.ProjVarsLifeIncreasing());
        }

        public ModelBase(RunSettings rs, Policy p, ScenarioManager sm, ProjVars v_)
        {
            // 속성 할당
            RunP = rs;
            Pol = p;
            SM = sm;
            v = v_;

            //ResultVals = new Result();
        }

        public virtual void Run(OutputManager outs) { }
        
        protected virtual void Init()
        {
            //var dt1 = DateTime.ParseExact(RunParams.StartYM.ToString(), "yyyyMM", CultureInfo.CurrentCulture);

            //var dt2 = DateTime.ParseExact(Pol.Rec.cont.contYM.ToString(), "yyyyMM", CultureInfo.CurrentCulture);
            //ElapsedMth = (dt1.Year - dt2.Year) * 12 + (dt1.Month - dt2.Month) + 1,   
            
            p = new ProjParams();
            p.CurrentMth = Pol.Rec.ElapsedMth;
            p.CurrentYr = (Pol.Rec.ElapsedMth - 1) / 12 + 1;
            p.CurrentAge = Pol.Rec.ContAge + Pol.Rec.ElapsedMth / 12;
            p.ProjPeriod = GetProjMth(Pol) - Pol.Rec.ElapsedMth; // 상품별로 구현

            v.FundsCnt = 3;
            v.Navs = new double[v.FundsCnt];
            v.PremAlloRates = new double[v.FundsCnt];
            InitScenarios();

            v.AccumPrem = Math.Min(Pol.Rec.PremYr * 12, Pol.Rec.ElapsedMth) * Pol.Rec.Prem;
            v.Navs[0] = Pol.Rec.FundVal01;
            v.Navs[1] = Pol.Rec.FundVal02;
            v.Navs[2] = Pol.Rec.FundVal03;
            v.PremAlloRates[0] = Pol.Rec.FundAllo01 / 100.0;
            v.PremAlloRates[1] = Pol.Rec.FundAllo02 / 100.0;
            v.PremAlloRates[2] = Pol.Rec.FundAllo03 / 100.0;

            v.DiscountedFactorBOP = 1.0;
            v.LivesBOP = 1.0;

            Prd = new Product(Pol.Rec.ProdCode);

            MaxAge = Pol.MortRates.Keys.Max();
        }

        protected void InitScenarios()
        {
            double[] mmf = new double[SM.CurrentScenData[ScenType.Dscrt].Length];
            double[] fi = new double[SM.CurrentScenData[ScenType.FI].Length];
            double[] eq = new double[SM.CurrentScenData[ScenType.EQ].Length];
            Dscrt = new double[SM.CurrentScenData[ScenType.Dscrt].Length];

            SM.CurrentScenData[ScenType.Dscrt].CopyTo(mmf, 0);
            SM.CurrentScenData[ScenType.FI].CopyTo(fi, 0);
            SM.CurrentScenData[ScenType.EQ].CopyTo(eq, 0);
            SM.CurrentScenData[ScenType.Dscrt].CopyTo(Dscrt, 0);

            AssetYield = new Dictionary<int, double[]>();
            AssetYield.Add(0, mmf);   // mmf
            AssetYield.Add(1, fi);      // fi
            AssetYield.Add(2, eq);      // eq
        }

        protected void SetInitValues()
        {
            p.CurrentMth++;
            p.CurrentYr = (p.CurrentMth - 1) / 12 + 1;
            p.CurrentAge = Pol.Rec.ContAge + (p.CurrentMth - 1) / 12;
            
            v.DiscountedFactorBOP = v.DiscountedFactorEOP;
            v.LivesBOP = v.LivesEOP;
        }

        protected virtual void SetPremAndLoadings()
        {
            if (p.CurrentYr <= Pol.Rec.PremYr)
            {
                v.Prem = Pol.Rec.Prem;
                v.AccumPrem += Pol.Rec.Prem;

                v.LoadingAlpha = Math.Min(v.Navs.Sum() + v.Prem, Prd.LoadAlpha * Pol.Rec.Prem);
                v.LoadingBeta = Math.Min(v.Navs.Sum() + v.Prem - v.LoadingAlpha, Prd.LoadBeta * Pol.Rec.Prem);
            }
            else
            {
                v.Prem = 0.0;
                v.LoadingAlpha = 0.0;
                v.LoadingBeta = 0.0;
            }

            v.LoadingGamma = Math.Min(v.Navs.Sum() + v.Prem - v.LoadingAlpha - v.LoadingBeta, Prd.LoadGamma * Pol.Rec.Prem);

            double amt = v.Prem - v.LoadingAlpha - v.LoadingBeta - v.LoadingGamma;

            AddUpToNavByPremAllo(amt);
        }

        protected void SetGmxbFee()
        {
            double totalNav = v.Navs.Sum();
            v.GmxbFee = totalNav * Prd.GmxbFee / 12.0;
            AddUpToNavByNavAllo(-v.GmxbFee);
        }

        /// <summary>
        /// 자산 수익률 반영
        /// </summary>
        protected void SetFundReturn()
        {     
            for (int i = 0; i < v.FundsCnt; i++)
            {
                int scenIdx = i + 1;
                double fundCost = v.Navs[i] * SM.FundCosts[i] / 12.0;
                v.Navs[i] -= fundCost;
                v.Navs[i] *= (Math.Pow(1.0 + AssetYield[i][p.ProjMth - 1], 1.0 / 12.0)); 
            }
        }
               
        /// <summary>
        /// 사망률, 해지율, 잔존자 세팅
        /// </summary>
        protected virtual void SetDecRates()
        {
            v.MortR = Pol.MortRates[Math.Min(MaxAge, p.CurrentAge)] / 12.0;

            if (p.CurrentYr < Pol.Rec.PremYr) v.LapseR = Pol.LapseRates["InPayment"][p.CurrentYr + 1] / 12.0;
            else v.LapseR = Pol.LapseRates["AfterPayment"][p.CurrentYr - Pol.Rec.PremYr + 1] / 12.0;
        }

        protected virtual void DoRebalancing()
        {
            v.IsRebalancing = true;
            double sumNav = v.Navs.Sum();

            if (sumNav > SmallDouble)
            {
                double safeAssetProportion = (v.Navs[0] + v.Navs[1]) / sumNav;
                if (safeAssetProportion < 0.7 + SmallDouble)
                {
                    v.Navs[1] += (v.Navs[2] - sumNav * 0.3);
                    v.Navs[2] = sumNav * 0.3;
                }
            }
        }

        protected virtual void SetPeopleDscrt()
        {
            v.LivesEOP = v.LivesBOP * (1.0 - v.MortR) * (1.0 - v.LapseR);

            double totDecPeople = v.MortR + v.LapseR;
            if (totDecPeople > SmallDouble)
            {
                v.MortPeople = (v.LivesBOP - v.LivesEOP) * v.MortR / totDecPeople;
                v.LapsePeople = (v.LivesBOP - v.LivesEOP) * v.LapseR / totDecPeople;
            }

            double d = Math.Pow(1.0 + Dscrt[p.ProjMth - 1], 1.0 / 12.0);
            v.DiscountedFactorEOP = v.DiscountedFactorBOP / d;
        }
       
    }
}
