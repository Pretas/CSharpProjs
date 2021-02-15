using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiabilityCalc
{
    public interface IModel
    { }

    public interface ICFTable
    {
        ICFTable GetCF(IModel model, Policy p, ScenarioManager sm);

        //string TableName { get; }
        //void SetTableName(string tn);

        void AddUp(ICFTable cf);
    }

    public class CFTable1 : ICFTable
    {
        public int Member1;
        public string Member2;
        public double Member3;

        //private string _tableName = "N/A";
        //public string TableName { get => _tableName; }

        public ICFTable GetCF(IModel model, Policy p, ScenarioManager sm)
        {
            CFTable1 cf = new CFTable1();

            //if (model.GetType() == typeof(Model1))
            //{
            //    Model1 modelFrom = model as Model1;
            //    //cf.Member1 = modelFrom.member1;
            //    //cf.Member2 = modelFrom.member2;
            //    //cf.Member3 = modelFrom.member3;
            //}

            return cf;
        }

        public void AddUp(CFTable1 cf)
        {
            CFTable1 modelFrom = cf as CFTable1;
            Member1 = modelFrom.Member1;
            Member2 = modelFrom.Member2;
            Member3 = modelFrom.Member3;
        }

        public void AddUp(ICFTable cf)
        {
            Member1 = 0;
            Member2 = "0";
            Member3 = 0.0;
        }
    }

    public enum GroupingType
    { BySG = 0, ByScen = 1, BySGByScen = 2, ByPol = 3, ByMth = 4 }


    public interface IMonthlyValues
    {
        void SetValues(IModel model, Policy p, ScenarioManager sm);
        string GetValues();
    }

    public class MonthlyValues1 : IMonthlyValues
    {
        int V1;
        string V2;
        double V3;
        double[] NavBegin;
        double[] NavEnd;

        public void SetValues(IModel model, Policy p, ScenarioManager sm)
        {
            //if (model is Model1)
            //{
            //    Model1 md = model as Model1;

            //    //V1 = md.member1;
            //    //V2 = md.member2;
            //    //V3 = md.member3;

            //    NavBegin = new double[10];
            //    NavEnd = new double[10];
            //}
        }

        public string GetValues()
        {
            return string.Format(@"{0},{1},{2}", V1, V2, V3);
        }
    }

    //public class GroupingDataLayout
    //{
    //    public GroupingType GroupingT;
    //    public ICFTable[] GroupedData;
    //    public Dictionary<string, int> GroupNameToNo;

    //    public void Init(GroupingType gt, ) 

    //    public void SetCF()
    //    {

    //    }
    //}

    // 그루핑 데이터, 단건에 대한 정보
    public class Accumlator
    {
        // 베이스가 되는 타입
        public Type CFTableType { get; private set; }
        public ICFTable Cf { get; private set; }

        public bool GroupsBySG { get; private set; } = true;
        public bool GroupsByScen { get; private set; } = true;
        public bool GroupsByPol { get; private set; } = false;
        public bool GroupsByMth { get; private set; } = false;

        // 그루핑된 데이터 저장소
        ICFTable[] DtBySG;
        ICFTable[] DtByScen;
        ICFTable[] DtByPol;
        ICFTable[] DtByMth;

        Dictionary<string, int> SgToNo;

        Type MVType;
        List<IMonthlyValues> monthlyValuesList;

        public void Init(Type cfType, Type mvType, List<string> sgList = null)
        {
            CFTableType = cfType;

            MVType = mvType;

            if (GroupsBySG)
            {
                SgToNo = new Dictionary<string, int>();
                for (int i = 0; i < sgList.Count; i++) SgToNo.Add(sgList[i], i);

                DtBySG = new ICFTable[sgList.Count];
                for (int i = 0; i < sgList.Count; i++)
                {
                    DtByScen[i] = (ICFTable)Activator.CreateInstance(CFTableType);
                }
            }

            if (GroupsByScen)
            {
                DtByScen = new ICFTable[100];
                for (int i = 0; i < 100; i++)
                {
                    DtByScen[i] = (ICFTable)Activator.CreateInstance(CFTableType);
                }
            }
        }

        public void SetCF(IModel model, Policy p, ScenarioManager sm)
        {
            if (GroupsBySG)
            {
                int no = 1;  //SgToNo[p.SubGroup];
                DtBySG[no].AddUp(Cf.GetCF(model, p, sm));
            }

            if (GroupsByScen)
            {
                int no = 1; // SgToNo[p.SubGroup];
                DtBySG[no].AddUp(Cf.GetCF(model, p, sm));
            }

            if (GroupsByPol) { }

            if (GroupsByMth) { }

            if (ShouldGetMonthlyValues(p))
            {
                IMonthlyValues mv = (IMonthlyValues)Activator.CreateInstance(MVType);
                mv.SetValues(model, p, sm);
                monthlyValuesList.Add(mv);
            }
        }

        private bool ShouldGetMonthlyValues(Policy p)
        {
            return true; // p.contNo == "1"; }
        }
    }
}
