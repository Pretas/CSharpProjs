using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiabilityCalc
{
    public class InterestRates
    {
        public readonly static int MaxYear = 20;
        public readonly static int SourceCurveTenorCnt = 14;
        public readonly static List<double> SourceCurveTenors = new List<double> { 1.0 / 365.0, 0.25, 0.5, 0.75, 1.0, 2.0, 3.0, 4.0, 5.0, 7.0, 10.0, 12.0, 15.0, 20.0 }; //14개
        public readonly static int BootStrappedCnt = MaxYear * 4 + 1;
        public readonly static double dt = 1.0 / 12.0;

        public List<double> SourceCurve { get; private set; }
        public List<double> Terms { get; private set; }
        public List<double> BootstrappedZeros { get; private set; }

        public List<double> ZeroCurve { get; private set; }
        public List<double> ForwardCurve { get; private set; }

        public int EndMth { get; private set; }

        public InterestRates(List<double> sourceCurve, int endMth)
        {
            if (sourceCurve.Count != 14) throw new Exception("should have 14 rates");

            SourceCurve = sourceCurve;
            EndMth = endMth;

            // 1/365, 0.25, ..., 20 까지 81개 term 정의
            Terms = new List<double> { 1.0 / 365.0 };
            for (int i = 1; i <= MaxYear * 4; i++) Terms.Add(i * 0.25);

            Bootstrap();

            SetCurves();
        }

        /// <summary>
        /// 붓스트래핑, BootstrappedZeros 세팅
        /// </summary>
        private void Bootstrap()
        {
            BootstrappedZeros = new List<double>();
            var interpolated = MathNet.Numerics.Interpolate.Linear(SourceCurveTenors, SourceCurve);

            double df = 0.0;
            double dfSum = 0.0;

            foreach (var term in Terms)
            {
                if (term == 1.0 / 365.0)
                {
                    // 1일
                    double df0 = 1.0 / (1.0 + SourceCurve[0] * term);
                    BootstrappedZeros.Add(-Math.Log(df0) / term);
                }
                else if (term == 0.25)
                {
                    // 3개월
                    df = 1.0 / (1.0 + SourceCurve[1] * term);
                    BootstrappedZeros.Add(-Math.Log(df) / term);
                    dfSum = df;
                }
                else
                {
                    double rt = interpolated.Interpolate(term);
                    df = (1.0 - rt * dfSum / 4.0) / (1.0 + rt / 4.0);
                    BootstrappedZeros.Add(-Math.Log(df) / term);
                    dfSum += df;
                }
            }
        }

        /// <summary>
        /// 제로, 포워드 세팅
        /// </summary>
        private void SetCurves()
        {
            ZeroCurve = new List<double>();
            ForwardCurve = new List<double>();

            var interpolated = MathNet.Numerics.Interpolate.Linear(Terms, BootstrappedZeros);
            
            // zero
            for (int mth = 0; mth < EndMth; mth++)
            {
                double t = Convert.ToDouble(mth) * dt;

                double zero = interpolated.Interpolate(Math.Min(t, MaxYear));
                double df = Math.Exp(-zero * mth);
                ZeroCurve.Add(zero);
            }

            // forward
            for (int mth = 1; mth < EndMth; mth++)
            {
                double t = Convert.ToDouble(mth) * dt;

                double dfBef = Math.Exp(-ZeroCurve[mth - 1] * (t - dt));
                double df = Math.Exp(-ZeroCurve[mth] * t);

                double forward = Math.Log(dfBef / df) / dt;
                ForwardCurve.Add(forward);
            }

            // 개수를 zero와 맞추기 위해
            ForwardCurve.Add(ForwardCurve.Last());
        }
    }
}
