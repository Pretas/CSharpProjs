using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiabilityCalc
{
    public class ScenParams
    {
        public List<double> SourceCurve { get; set; }
        public double HW_a { get; set; }
        public double HW_sigma { get; set; }
        public double BondSigma { get; set; }
        public double EqSigma { get; set; }        
    }

    public class ScenarioManager
    {
        public List<ScenType> ScenTypeList = new List<ScenType> { ScenType.Dscrt, ScenType.FI, ScenType.EQ };

        // dic<시나리오번호, 딕<자산군 번호, double[1200개월]>>
        private Dictionary<int, Dictionary<ScenType, double[]>> ScenData;

        public int CurrentScenNo { get; private set; }
        public Dictionary<ScenType, double[]> CurrentScenData { get; private set; }
        
        public Dictionary<int, double> FundCosts { get; private set; } = new Dictionary<int, double>();

        public int ScenEndMth { get; private set; } = 1200;
        public ScenParams ScenP { get; private set; }

        public ScenarioManager(ScenParams p, int endMth)
        {
            ScenP = p;
            ScenEndMth = endMth;

            SetFundCost();
        }

        private void SetFundCost()
        {
            FundCosts[0] = 0.0010;  // mmf
            FundCosts[1] = 0.0030;  // fi
            FundCosts[2] = 0.0100;  // eq
        }

        /// <summary>
        /// 랜덤 즉시 생성하여 실행
        /// </summary>
        /// <param name="scenCnt"></param>
        /// <param name="startScenNo"></param>
        public void SetScenDataFromRandomGen(int scenCnt)
        {
            var intRates = new InterestRates(ScenP.SourceCurve, ScenEndMth);
            var hw = new HullWhite(ScenP.HW_a, ScenP.HW_sigma);
            var scen = new ScnearioGen(intRates, hw, ScenP.EqSigma, ScenEndMth);

            ScenData = new Dictionary<int, Dictionary<ScenType, double[]>>();

            for (int i = 0; i <= scenCnt; i++)
            {
                double[] rdIR;
                double[] rdEQ;
                scen.GetRandom(out rdIR, out rdEQ);
                ScenData.Add(i, scen.GetScen(rdIR, rdEQ));
            }
        }

        /// <summary>
        /// 랜덤을 파일에서 가져와 실행
        /// </summary>
        /// <param name="scenCnt"></param>
        /// <param name="startScenNo"></param>
        public void SetScenDataFromFile(int scenCnt)
        {
            var intRates = new InterestRates(ScenP.SourceCurve, ScenEndMth);
            var hw = new HullWhite(ScenP.HW_a, ScenP.HW_sigma);
            var scen = new ScnearioGen(intRates, hw, ScenP.EqSigma, ScenEndMth);

            var currentDir = Directory.GetCurrentDirectory();
            var rdIrList = Tools.LibFile.ReadCSV(currentDir + @"\ScenarioGenerator\RandomSetIR.csv");
            var rdEqList = Tools.LibFile.ReadCSV(currentDir + @"\ScenarioGenerator\RandomSetEQ.csv");

            ScenData = new Dictionary<int, Dictionary<ScenType, double[]>>();

            for (int i = 1; i <= scenCnt; i++)
            {
                var rdIR = Array.ConvertAll(rdIrList[i-1], item => Convert.ToDouble(item));
                var rdEQ = Array.ConvertAll(rdEqList[i-1], item => Convert.ToDouble(item));

                double a = rdIR.Sum() + rdEQ.Sum();

                ScenData.Add(i, scen.GetScen(rdIR, rdEQ));
            }
        }

        public void SetCurrentScen(int scenNo)
        {
            CurrentScenNo = scenNo;
            CurrentScenData = ScenData[scenNo];            
        }
    }
}
