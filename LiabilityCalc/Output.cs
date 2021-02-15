using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace LiabilityCalc
{
    public class Result : IEquatable<Result>
    {
        public int PolSeq { get; private set; }
        public int ScenSeq { get; private set; }
        public int Mth { get; private set; }

        public double[] Res { get; private set; }
        public int OutLth { get; private set; }

        public Result(int p, int s, int m, int lth = 5)
        {
            PolSeq = p; ScenSeq = s; Mth = m;
            Res = new double[lth];
            OutLth = lth;
        }

        public void SetGroupingKeys(int p, int s, int m) { PolSeq = p; ScenSeq = s; Mth = m; }
        public void SetValue(double[] r)
        {
            for (int i = 0; i < OutLth; i++) Res[i] += r[i];
        }

        public void AddUp(Result resFrom)
        {
            //if (Res == null) resFrom.Res.CopyTo(Res, 0);

            for (int i = 0; i < Res.Length; i++) Res[i] += resFrom.Res[i];
        }

        public bool Equals(Result other)
        {
            var o = other as Result;
            return !(o.PolSeq != PolSeq || o.ScenSeq != ScenSeq || o.Mth != Mth);
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash + 7) * PolSeq.GetHashCode();
            hash = (hash + 7) * ScenSeq.GetHashCode();
            hash = (hash + 7) * Mth.GetHashCode();
            return hash;
        }
    }

    public class OutputManager
    {
        public Action<int,int,int,double[]> ActionSetValues;
        public List<Result> Outs { get; private set; } = new List<Result>();
        public RunSettings RSettings { get; private set; }    
        public Tools.DBConnParams DbParams;

        // contno, scen, mth
        private Result[,,] Outs2;

        private Dictionary<int, int> PolNoMatching;
        private int StartScenNo;

        public OutputManager(RunSettings rs, DBConnParams dbParams, List<int> contNos, int scenStart)
        {
            RSettings = rs;
            DbParams = dbParams;

            // 향후 결과 입력시 사용 : dic<contno, idx> : 계약번호로 인덱스를 찾기위해
            PolNoMatching = new Dictionary<int, int>();
            for (int i = 0; i < contNos.Count(); i++) PolNoMatching.Add(contNos[i], i);

            // 향후 결과 입력시 사용 : 시나리오 시작점
            StartScenNo = scenStart;
            
            InitializeOutputSpace();

            SetGroupingFunction();
        }

        private void InitializeOutputSpace()
        {
            int p_cnt = PolNoMatching.Count();
            int s_cnt = RSettings.ScenCntOneBatch;

            // 런결과 공간할당
            if (RSettings.InsertsByPol)
            {
                if (RSettings.InsertsByScen) { Outs2 = new Result[p_cnt, s_cnt, 1]; }
                else if (RSettings.InsertsByMth) { Outs2 = new Result[p_cnt, 1, 1200]; }
                else Outs2 = new Result[p_cnt, 1, 1];
            }
            else
            {
                if (RSettings.InsertsByScen) { Outs2 = new Result[1, s_cnt, 1]; }
                else if (RSettings.InsertsByMth) { Outs2 = new Result[1, 1, 1200]; }
                else throw new Exception("Failed to initialize output space");
            }

            // 런결과 각각을 0으로 초기화
            // PolNoMatching : dic<Contno, idx>
            foreach (var kv in PolNoMatching)
            {
                for (int j = 0; j < Outs2.GetLength(1); j++)
                {
                    for (int k = 0; k < Outs2.GetLength(2); k++)
                    {
                        // 그룹핑하지 않을 경우 0을 넣음
                        int p = (Outs2.GetLength(0) == 1 ? 0 : kv.Key);
                        int s = (Outs2.GetLength(1) == 1 ? 0 : j + StartScenNo);
                        int m = (Outs2.GetLength(2) == 1 ? 0 : k + 1);

                        Outs2[kv.Value, j, k] = new Result(p, s, m);
                    }
                }
            }
        }

        private void SetGroupingFunction()
        {
            // 런결과 공간할당
            if (RSettings.InsertsByPol)
            {
                if (RSettings.InsertsByScen)  ActionSetValues = SetByPolScen; 
                else if (RSettings.InsertsByMth) ActionSetValues = SetByPolMth;
                else ActionSetValues = SetByPol;
            }
            else
            {
                if (RSettings.InsertsByScen) ActionSetValues = SetByScen;
                else if (RSettings.InsertsByMth) ActionSetValues = SetByMth;
                else throw new Exception("Failed to initialize output space");
            }
        }

        private void SetByPol(int p, int s, int m, double[] r) { Outs2[p, 0, 0].SetValue(r); }
        private void SetByPolScen(int p, int s, int m, double[] r) { Outs2[p, s, 0].SetValue(r); }
        private void SetByPolMth(int p, int s, int m, double[] r) { Outs2[p, 0, m].SetValue(r); }
        private void SetByScen(int p, int s, int m, double[] r) { Outs2[0, s, 0].SetValue(r); }
        private void SetByMth(int p, int s, int m, double[] r) { Outs2[0, 0, m].SetValue(r); }
        
        public void SetMthlyValues(int contNo, int currentScenNo, int mth, double[] resLine)
        {
            ActionSetValues(PolNoMatching[contNo], currentScenNo - StartScenNo, mth, resLine);
        }

        public void InsertToDB(string id, int rank)
        {
            // 데이터 전환 to list<object[]>
            var lst = new List<object[]>();
            object[] obj;
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            double scenDiv = RSettings.InsertsByScen ? 1.0 : RSettings.ScenCntOneBatch;

            foreach (var row in Outs2)
            {
                obj = new object[11];
                obj[0] = id;
                obj[1] = now;
                obj[2] = rank;
                obj[3] = row.PolSeq;
                obj[4] = row.ScenSeq;
                obj[5] = row.Mth;

                for (int i = 0; i < row.Res.Length; i++)
                {
                    obj[6 + i] = row.Res[i] / scenDiv;
                }

                lst.Add(obj);
            }

            // db입력
            IDbConnection dbConn = new Tools.MsSqlDbConn(DbParams);
            dbConn.Insert(lst, @"proj_result");
        }

        //public OutputManager(RunSettings rs, Tools.DBConnParams dbParams, List<int> list)
        //{
        //    if (rs.InsertsByPol)
        //    {
        //        if (rs.InsertsByScen) { GetDataWithGrouping = GetByPolScen; }
        //        else if (rs.InsertsByMth) { GetDataWithGrouping = GetByPolMth; }
        //        else GetDataWithGrouping = GetByPol;
        //    }
        //    else
        //    {
        //        if (rs.InsertsByScen) { GetDataWithGrouping = GetByScen; }
        //        else if (rs.InsertsByMth) { GetDataWithGrouping = GetByMth; }
        //        else throw new Exception("Failed to get groupping Func");
        //    }

        //    DbParams = dbParams;
        //}

        //public void SetMthlyValues(int contNo, int currentScenNo, Result res)
        //{
        //    if (rs.InsertsByPol)
        //    {
        //        if (rs.InsertsByScen) { Outs2[,,].AddUp( =  = new Result[polCnt, scenCnt, 1]; }
        //        else if (rs.InsertsByMth) { Outs2 = new Result[polCnt, 1, 1200]; }
        //        else Outs2 = new Result[polCnt, 1, 1];
        //    }
        //    else
        //    {
        //        if (rs.InsertsByScen) { Outs2 = new Result[1, scenCnt, 1]; }
        //        else if (rs.InsertsByMth) { Outs2 = new Result[1, 1, 1200]; }
        //        else throw new Exception("Failed to get groupping Func");
        //    }

        //    if (rs.InsertsByPol)
        //    {
        //        if (rs.InsertsByScen) {  GetDataWithGrouping = GetByPolScen; }
        //        else if (rs.InsertsByMth) { GetDataWithGrouping = GetByPolMth; }
        //        else GetDataWithGrouping = GetByPol;
        //    }
        //    else
        //    {
        //        if (rs.InsertsByScen) { GetDataWithGrouping = GetByScen; }
        //        else if (rs.InsertsByMth) { GetDataWithGrouping = GetByMth; }
        //        else throw new Exception("Failed to get groupping Func");
        //    }


        //    throw new NotImplementedException();
        //}

        //public void SetValues(Result resFrom)
        //{
        //    GetDataWithGrouping(resFrom);

        //    if (Outs.Contains(resFrom))
        //    {
        //        Outs[Outs.FindIndex(x => x.Equals(resFrom))].AddUp(resFrom);
        //    }
        //    else
        //    {
        //        Outs.Add(resFrom);
        //    }
        //}
    }
}
