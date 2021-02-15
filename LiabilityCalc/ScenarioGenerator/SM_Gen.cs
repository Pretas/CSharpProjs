using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiabilityCalc
{
    public enum ScenType
    { Dscrt = 0, FI, EQ }

    public class ScnearioGen
    {
        public readonly double dt = 1/12;

        public int EndMth { get; private set; }

        public InterestRates Curves { get; private set; }
        public HullWhite HW { get; private set; }
        public double EqSigma { get; private set; }

        public MathNet.Numerics.Random.MersenneTwister RD { get; private set; } = new MathNet.Numerics.Random.MersenneTwister();

        public ScnearioGen(InterestRates intR, HullWhite hw, double eqSigma, int endMth)
        {
            Curves = intR;
            HW = hw;
            EqSigma = eqSigma;
            EndMth = endMth;
        }
        
        public Dictionary<ScenType, double[]> GetScen(double[] rdIR, double[] rdEQ)
        {
            double[] dscrt = new double[EndMth];
            double[] eq = new double[EndMth];
            double[] fi = new double[EndMth];

            double x = 0.0;
            double t = 0.0;
            double discFactor = 0.0;
            double forward = 0.0;

            for (int mth = 0; mth < EndMth; mth++)
            {
                t = Convert.ToDouble(mth) * dt;
                forward = Curves.ForwardCurve[mth];

                // 적용할인율
                if (mth > 0) x += -HW.a * x * dt + HW.sigma * Math.Sqrt(dt) * rdIR[mth];
                discFactor = x + HW.Alpha(forward, t);

                // 할인율 시나리오(연속 --> 이산)
                dscrt[mth] = Math.Exp(discFactor) - 1.0;
                
                // 채권, 5년 채권만 편입됨을 가정
                double bondSigma = -HW.sigma * HW.B(t, t + 5);
                fi[mth] = Math.Exp(discFactor - 0.5 * Math.Pow(bondSigma, 2.0) * dt
                    + bondSigma * Math.Sqrt(dt) * rdIR[mth]) - 1.0;

                // 주식
                eq[mth] = Math.Exp((forward - 0.5 * Math.Sqrt(EqSigma)) * dt
                    + EqSigma * Math.Sqrt(EqSigma) * rdEQ[mth]) - 1.0;
            }

            var res = new Dictionary<ScenType, double[]>();
            res.Add(ScenType.Dscrt, dscrt);
            res.Add(ScenType.FI, fi);
            res.Add(ScenType.EQ, eq);

            return res;
        }

        public void GetRandom(out double[] rdIR, out double[] rdEQ)
        {
            var normalD = new MathNet.Numerics.Distributions.Normal();

            rdIR = new double[EndMth];
            rdEQ = new double[EndMth];

            for (int i = 0; i < EndMth; i++)
            {
                rdIR[i] = normalD.InverseCumulativeDistribution(RD.NextDouble());
                rdEQ[i] = normalD.InverseCumulativeDistribution(RD.NextDouble());
            }
        }
    }
}
