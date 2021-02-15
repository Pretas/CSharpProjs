using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiabilityCalc
{
    public enum EnumProdSeg
    {
        Annuity = 0,
        Life
    }

    public class Product
    {
        public int ProdCode { get; protected set; }
        public bool HasBonus { get; protected set; } = false;
        public bool HasFundsRebalancing { get; protected set; } = false;
        
        // 사업비
        public double LoadAlpha { get; protected set; }
        public double LoadBeta { get; protected set; }
        public double LoadGamma { get; protected set; }

        // 보증수수료
        public double GmxbFee { get; protected set; }        

        public Product(int code)
        {
            ProdCode = code;

            if (code < 4)
            {
                LoadAlpha = 0.07;
                LoadBeta = 0.03;
                LoadGamma = 0.001;
                GmxbFee = 0.005; //50bp

                if (code == 2 || code == 3) HasFundsRebalancing = true;
                if (code == 1 || code == 3) HasBonus = true;
            }
            else if (code < 8)
            {
                LoadAlpha = 0.10;
                LoadBeta = 0.03;
                LoadGamma = 0.008;
                GmxbFee = 0.010; //100bp

                if (code == 6 || code == 7) HasFundsRebalancing = true;
                if (code == 5 || code == 7) HasBonus = true;
            }
            else if (code < 12)
            {
                LoadAlpha = 0.15;
                LoadBeta = 0.04;
                LoadGamma = 0.005;
                GmxbFee = 0.015;

                if (code == 10 || code == 11) HasFundsRebalancing = true;
                if (code == 9 || code == 11) HasBonus = true;
            }
            else
            {
                LoadAlpha = 0.12;
                LoadBeta = 0.01;
                LoadGamma = 0.007;
                GmxbFee = 0.018;

                if (code == 14 || code == 15) HasFundsRebalancing = true;
                if (code == 13 || code == 15) HasBonus = true;
            }
        }
    }
}
