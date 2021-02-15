using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiabilityCalc
{
    public partial class Runner
    {
        public Dictionary<int, double[][]> GetPV(Dictionary<int, double[][]> resByPolMth)
        {
            Dictionary<int, double[][]> returnVals = new Dictionary<int, double[][]>();

            foreach (var kv in resByPolMth)
            {
                int polNo = kv.Key;
                double[][] resOnePol = kv.Value;

                double[][] resOnePolCopied = new double[1][];
                resOnePolCopied[0] = new double[resOnePol[0].Length];

                for (int mth = 0; mth < resOnePol.Length; mth++)
                {
                    for (int i = 0; i < resOnePol[mth].Length; i++)
                    {
                        resOnePolCopied[0][i] += resOnePol[mth][i];
                    }
                }

                returnVals.Add(polNo, resOnePolCopied);
            }

            return returnVals;
        }


        private void InsertResult(string id, Dictionary<int, double[][]> resByPol)
        {
            // 데이터 전환 to list<object[]>
            var lst = new List<object[]>();
            object[] obj;
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            foreach (var kv in resByPol)
            {
                for (int rowNo = 0; rowNo < kv.Value.Length; rowNo++)
                {
                    obj = new object[14];
                    obj[0] = id;
                    obj[1] = now;
                    obj[2] = kv.Key;
                    obj[3] = (RSettings.InsertsByMth ? rowNo + 1 : 0);

                    for (int i = 0; i < kv.Value[rowNo].Length; i++)
                    {
                        obj[4 + i] = kv.Value[rowNo][i];
                    }

                    lst.Add(obj);
                }
            }

            // db입력
            var dbParams = new Tools.DBConnParams();
            dbParams.InitMY();
            Tools.MySqlDBConn dbConn = new Tools.MySqlDBConn(dbParams);
            dbConn.Insert(lst, @"proj_result");
        }

       
        private void AddUP(int polSeq, int batchSeq, double[][] resAverage, ref Dictionary<int, double[][]> resByPol)
        {
            if (batchSeq == 0)
            {
                resByPol[polSeq] = resAverage;
            }
            else
            {
                int rowCnt = resAverage.Length;
                int colCnt = resAverage[0].Length;

                for (int i = 0; i < rowCnt; i++)
                {
                    for (int j = 0; j < colCnt; j++)
                    {
                        resByPol[polSeq][i][j] = resByPol[polSeq][i][j] * batchSeq / (batchSeq + 1)
                            + resAverage[i][j] / (batchSeq + 1);
                    }
                }
            }
        }

        private void AddUP(int scenSeq, ref double[][] resAverage, double[][] res)
        {
            if (scenSeq == 0)
            {
                resAverage = res;
            }
            else
            {
                int rowCnt = resAverage.Length;
                int colCnt = resAverage[0].Length;

                for (int i = 0; i < rowCnt; i++)
                {
                    for (int j = 0; j < colCnt; j++)
                    {
                        resAverage[i][j] = resAverage[i][j] * scenSeq / (scenSeq + 1)
                            + res[i][j] / (scenSeq + 1);
                    }
                }
            }
        }

    }
}
