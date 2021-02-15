using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiabilityCalc
{
    public class HullWhite
    {
        public double a;
        public double sigma;

        public HullWhite(double a, double sigma)
        {
            this.a = a;
            this.sigma = sigma;
        }

        public double Alpha(double forward, double t)
        {
            return forward + 0.5 * Math.Pow(sigma / a * (1.0 - Math.Exp(-a * t)), 2.0);
        }

        public double A(double forward, double pt, double pT, double t, double T)
        {
            double term1 = Math.Pow(sigma * B(t, T), 2.0);
            double term2 = Math.Exp(-2.0 * a * t);
            double term3 = Math.Exp(B(t, T) * forward - 0.25 * term1 / a * (1.0 - term2));

            return  pT / pt * term3;
        }

        public double B(double t, double T)
        {
            return (1.0 - Math.Exp(-a * (T - t))) / a;
        }

        public double P(double r, double forward, double pt, double pT, double t, double T)
        {
            return A(forward, pt, pT, t, T) * Math.Exp(-B(t, T) * r);
        }
    }
}