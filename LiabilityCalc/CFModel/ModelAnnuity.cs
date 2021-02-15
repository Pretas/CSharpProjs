using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiabilityCalc
{
    public partial class ModelAnnuity : ModelBase
    {
        public class ProjVarsAnnuity : ProjVars
        {
            public double GuaranteeAmt;
        }

        private ProjVarsAnnuity vAnn;

        public ModelAnnuity(RunSettings rs, Policy p, ScenarioManager sm, ProjVarsAnnuity vAnn_) : base(rs, p, sm, vAnn_)
        {
            vAnn = vAnn_;
        }
  
        protected virtual void SetDecrement()
        {
            SetDecRates();

            // 다이나믹랩스 적용
            // 보증액 대비 적립금이 낮을수록 보증옵션의 가치가 올라가므로 해지를 덜 하게됨
            // 완납 상태에서 반영
            if (RunP.HasDynamicLapse)
            {
                double moneyness = vAnn.Navs.Sum() / vAnn.GuaranteeAmt;
                double coeff = Math.Min(0.5, Math.Max(-0.3, (moneyness - 1.2) * 0.5));
                vAnn.LapseR *= (1.0 + coeff); 
            }

            SetPeopleDscrt();
        }

        /// <summary>
        /// 연금개시 3년전에 유동성&채권형펀드 비중이 70%보다 낮을 경우 
        /// 주식형펀드(2) 금액을 채권형펀드(1)로 옮김
        /// </summary>
        protected override void DoRebalancing()
        {
            if (Prd.HasFundsRebalancing && (Pol.Rec.StartAgeOfSomething - 3 - Pol.Rec.ContAge) * 12 == p.CurrentMth)
            {
                base.DoRebalancing();
            }
            else
                vAnn.IsRebalancing = false;
        }

        /// <summary>
        /// 완납 시점 총보험료의 5%를 보너스로 투입
        /// </summary>
        protected virtual void SetBonus()
        {
            if (p.CurrentMth == Pol.Rec.PremYr * 12)
            {
                vAnn.Bonus = vAnn.AccumPrem * 0.05;
                AddUpToNavByPremAllo(vAnn.Bonus);
            }
            else
            {
                vAnn.Bonus = 0.0;
            }
        }

        
    }

    public partial class ModelAnnuityRop : ModelAnnuity
    {
        private ProjVarsAnnuity vAnnRop;

        public ModelAnnuityRop(RunSettings rs, Policy p, ScenarioManager sm, ProjVarsAnnuity vAnnRop_) : base(rs, p, sm, vAnnRop_)
        {
            vAnnRop = vAnnRop_;
        }

        public override void Run(OutputManager outs)
        {
            Init();

            // 월간 루프
            for (p.ProjMth = 1; p.ProjMth <= p.ProjPeriod; p.ProjMth++)
            {
                SetInitValues();
                SetPremAndLoadings();
                SetGmxbFee();
                SetBonus();
                SetFundReturn();
                DoRebalancing();
                SetDecrement();
                SetClaim();
                SetResult(outs);
            }

            if (printsThisProjection) Tools.LibFile.WriteFile(@"D:\testAnn.csv", MthlyVars, '\t');
        }

        protected override void SetPremAndLoadings()
        {
            base.SetPremAndLoadings();

            vAnnRop.GuaranteeAmt = vAnnRop.AccumPrem;
        }

        protected void SetClaim()
        {
            if ((Pol.Rec.StartAgeOfSomething - Pol.Rec.ContAge) * 12 == p.CurrentMth)
            {
                vAnnRop.GmxbClaim = Math.Max(0.0, vAnnRop.GuaranteeAmt * 1.2 - vAnnRop.Navs.Sum());
            }
            else
            {
                vAnnRop.GmxbClaim = 0.0;
            }
        }

        protected override double GetGmxbClaimDec()
        {
            return vAnnRop.GmxbClaim * vAnnRop.LivesEOP;
        }

        protected override double GetGmxbClaimDecPV()
        {
            return GetGmxbClaimDec() * vAnnRop.DiscountedFactorEOP;
        }
    }


    public partial class ModelAnnuityGLWB : ModelAnnuity
    {
        public class ProjVarsAnnuityGLWB : ProjVarsAnnuity
        {
            public double AnnuityBenefit;
        }

        private ProjVarsAnnuityGLWB vAnnGlwb;

        public ModelAnnuityGLWB(RunSettings rs, Policy p, ScenarioManager sm, ProjVarsAnnuityGLWB vAnnGlwb_) : base(rs, p, sm, vAnnGlwb_)
        {
            vAnnGlwb = vAnnGlwb_;
        }


        public override void Run(OutputManager outs)
        {
            Init();

            // 월간 루프
            for (p.ProjMth = 1; p.ProjMth <= p.ProjPeriod; p.ProjMth++)
            {
                SetInitValues();
                SetClaim();
                SetPremAndLoadings();
                SetGmxbFee();
                SetBonus();
                SetFundReturn();
                DoRebalancing();
                SetDecrement();
                SetResult(outs);
            }

            if (printsThisProjection) Tools.LibFile.WriteFile(@"D:\testAnn.csv", MthlyVars, '\t');
        }

        protected double AnnAmt = 0.0;

        protected override void Init()
        {
            base.Init();

            double allPrem = Pol.Rec.Prem * Pol.Rec.PremYr * 12;
            double allAnnAmt = allPrem * (1.0 + 0.1 * (Pol.Rec.StartAgeOfSomething - Pol.Rec.ContAge));
            AnnAmt = allAnnAmt / (90 - Pol.Rec.StartAgeOfSomething);
        }

        protected void SetClaim()
        {
            if (p.CurrentAge >= Pol.Rec.StartAgeOfSomething && (p.CurrentMth % 12 == 1))
            {
                double sumNav = vAnnGlwb.Navs.Sum();
                vAnnGlwb.GmxbClaim = Math.Max(0.0, AnnAmt - sumNav);
                vAnnGlwb.AnnuityBenefit = Math.Min(sumNav, AnnAmt);
                AddUpToNavByNavAllo(-vAnnGlwb.AnnuityBenefit);
                vAnnGlwb.GuaranteeAmt -= AnnAmt;
            }
            else
            {
                vAnnGlwb.AnnuityBenefit = 0.0;
                vAnnGlwb.GmxbClaim = 0.0;
            }
        }

        protected override void SetPremAndLoadings()
        {
            base.SetPremAndLoadings();

            vAnnGlwb.GuaranteeAmt = vAnnGlwb.AccumPrem * (1.0 + 0.03 * (p.CurrentYr-1));
        }

        protected override double GetGmxbClaimDec()
        {
            return vAnnGlwb.GmxbClaim * vAnnGlwb.LivesBOP;
        }

        protected override double GetGmxbClaimDecPV()
        {
            return GetGmxbClaimDec() * vAnnGlwb.DiscountedFactorBOP;
        }
    }
}
