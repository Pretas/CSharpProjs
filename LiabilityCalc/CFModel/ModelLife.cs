using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiabilityCalc
{
    public partial class ModelLife : ModelBase
    {
        public class ProjVarsLife : ProjVars
        { }

        private ProjVarsLife vLife;

        public ModelLife(RunSettings rs, Policy p, ScenarioManager sm, ProjVarsLife vLife_) : base(rs, p, sm, vLife_)
        {
            vLife = vLife_;
        }

        public override void Run(OutputManager outs)
        {
            // 초기화, 프로젝션 전체에 필요한 변수들 정의
            Init();

            // 월간 루프
            for (p.ProjMth = 1; p.ProjMth <= p.ProjPeriod; p.ProjMth++)
            {
                SetInitValues();                
                SetPremAndLoadings();
                SetGmxbFee();
                SetBonus();
                SetDecRates();               
                SetRiskCharge();
                SetFundReturn();
                DoRebalancing();
                SetPeopleDscrt();
                SetClaim();
                SetResult(outs);
            }

            if (printsThisProjection) Tools.LibFile.WriteFile(@"D:\testLife.csv", MthlyVars, '\t');
        }

       
        protected virtual void SetRiskCharge()
        {
            // 월대체 : 위험보험료, 사업비2         
            double totNav = vLife.Navs.Sum();
            double riskPrem = Math.Max(0.0, (Pol.Rec.ContAmt - totNav)) * vLife.MortR;

            AddUpToNavByNavAllo(-riskPrem);
        }

        /// <summary>
        /// 65세 시점에 유동성&채권형펀드 비중이 70%보다 낮을 경우 
        /// 주식형펀드(2) 금액을 채권형펀드(1)로 옮김
        /// </summary>
        protected override void DoRebalancing()
        {
            if (p.CurrentAge == (65 - 1) && p.CurrentMth % 12 == 0)
            {
                vLife.IsRebalancing = true;
                base.DoRebalancing();
            }
            else
                vLife.IsRebalancing = false;
        }

        /// <summary>
        /// 완납시점 가입금액의 1%를 보너스로 투입
        /// </summary>
        protected virtual void SetBonus()
        {
            if (p.CurrentMth == Pol.Rec.PremYr * 12)
            {
                vLife.Bonus = Pol.Rec.ContAmt * 0.01;
                AddUpToNavByPremAllo(vLife.Bonus);
            }
            else
                vLife.Bonus = 0.0;
        }

        protected virtual void SetClaim() { }

        protected override double GetGmxbClaimDec()
        {
            return vLife.GmxbClaim * vLife.MortPeople;
        }

        protected override double GetGmxbClaimDecPV()
        {
            return GetGmxbClaimDec() * vLife.DiscountedFactorEOP;
        }

    }

    public partial class ModelLifeFixed : ModelLife
    {
        private ProjVarsLife vLifeFixed;

        public ModelLifeFixed(RunSettings rs, Policy p, ScenarioManager sm, ProjVarsLife vLifeFixed_) : base(rs, p, sm, vLifeFixed_)
        {
            vLifeFixed = vLifeFixed_;
        }

        protected override void SetClaim()
        {
            double sumNav = vLifeFixed.Navs.Sum();

            if (sumNav < SmallDouble) vLifeFixed.GmxbClaim = Pol.Rec.ContAmt;
            else vLifeFixed.GmxbClaim = 0.0;
        }
    }

    public partial class ModelLifeIncreasing : ModelLife
    {
        public class ProjVarsLifeIncreasing : ProjVarsLife
        {
            public double DeathBenefit;
        }

        private ProjVarsLifeIncreasing vLifeInc;

        public ModelLifeIncreasing(RunSettings rs, Policy p, ScenarioManager sm, ProjVarsLifeIncreasing vLifeInc_) : base(rs, p, sm, vLifeInc_)
        {
            vLifeInc = vLifeInc_;
        }

        protected override void SetRiskCharge()
        {
            SetDeathBenefit();

            // 월대체 : 위험보험료, 사업비2         
            double totNav = vLifeInc.Navs.Sum();
            double riskPrem = Math.Max(0.0, (vLifeInc.DeathBenefit - totNav)) * vLifeInc.MortR;

            AddUpToNavByNavAllo(-riskPrem);
        }

        /// <summary>
        /// 특정 나이 이후부터 사망보험금이 연간 1%씩 상승
        /// </summary>
        protected void SetDeathBenefit()
        {
            if (p.ProjMth == 1) vLifeInc.DeathBenefit = Pol.Rec.ContAmt;

            if (p.CurrentAge >= Pol.Rec.StartAgeOfSomething && (p.CurrentMth % 12 == 1))
            {
                int yr = p.CurrentAge - Pol.Rec.StartAgeOfSomething + 1;
                vLifeInc.DeathBenefit = Pol.Rec.ContAmt * (1.0 + yr * 0.01);
            }
        }

        protected override void SetClaim()
        {
            double sumNav = vLifeInc.Navs.Sum();

            if (sumNav < SmallDouble) vLifeInc.GmxbClaim = vLifeInc.DeathBenefit;
            else vLifeInc.GmxbClaim = 0.0;
        }
    }
}
