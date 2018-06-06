using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CKServer
{
    class Func_DA
    {

        public static DataTable dt_DA = new DataTable();
        public static int DAChanNums = 8;

        public static void Init_Table()
        {
            dt_DA.Columns.Add("序号", typeof(Int32));
            dt_DA.Columns.Add("名称", typeof(String));
            dt_DA.Columns.Add("值", typeof(Double));

            //dt_OC.Columns.Add("脉宽", typeof(int));

            for (int i = 0; i < DAChanNums; i++)
            {
                DataRow dr = dt_DA.NewRow();
                dr["序号"] = i + 1;
                dr["名称"] = Function.GetConfigStr(Data.DAconfigPath, "add", "DA_Channel_" + i.ToString(), "name");
                dr["值"] = Function.GetConfigStr(Data.DAconfigPath, "add", "DA_Channel_" + i.ToString(), "value");
                dt_DA.Rows.Add(dr);
            }
        }
    }
}
