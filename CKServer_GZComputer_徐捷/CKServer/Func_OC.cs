using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CKServer
{
    class Func_OC
    {
        public static DataTable dt_OC_Out = new DataTable();

        public static DataTable dt_OC_In1 = new DataTable();
        public static DataTable dt_OC_In2 = new DataTable();
        public static DataTable dt_OC_In3 = new DataTable();

        public static void Init_Table()
        {
            dt_OC_In1.Columns.Add("序号", typeof(Int32));
            dt_OC_In1.Columns.Add("名称", typeof(String));
            dt_OC_In1.Columns.Add("计数", typeof(Int32));
            dt_OC_In1.Columns.Add("脉宽", typeof(Int32));
            for(int i=0;i<160;i++)
            {
                DataRow dr = dt_OC_In1.NewRow();
                dr["序号"] = i + 1;
                //            dr["名称"] = Function.GetConfigStr(Data.OCconfigPath, "add", "OC_Channel_" + i.ToString(), "name");
                dr["名称"] = "OC通道"+(i+1).ToString();
                dt_OC_In1.Rows.Add(dr);
            }

            dt_OC_In2.Columns.Add("序号", typeof(Int32));
            dt_OC_In2.Columns.Add("名称", typeof(String));
            dt_OC_In2.Columns.Add("计数", typeof(Int32));
            dt_OC_In2.Columns.Add("脉宽", typeof(Int32));
            for (int i = 0; i < 160; i++)
            {
                DataRow dr = dt_OC_In2.NewRow();
                dr["序号"] = i + 161;
                //            dr["名称"] = Function.GetConfigStr(Data.OCconfigPath, "add", "OC_Channel_" + i.ToString(), "name");
                dr["名称"] = "OC通道" + (i+161).ToString();
                dt_OC_In2.Rows.Add(dr);
            }

            dt_OC_In3.Columns.Add("序号", typeof(Int32));
            dt_OC_In3.Columns.Add("名称", typeof(String));
            dt_OC_In3.Columns.Add("计数", typeof(Int32));
            dt_OC_In3.Columns.Add("脉宽", typeof(Int32));
            for (int i = 0; i < 160; i++)
            {
                DataRow dr = dt_OC_In3.NewRow();
                dr["序号"] = i + 321;
                //            dr["名称"] = Function.GetConfigStr(Data.OCconfigPath, "add", "OC_Channel_" + i.ToString(), "name");
                dr["名称"] = "OC通道" + (i + 321).ToString();
                dt_OC_In3.Rows.Add(dr);
            }

            dt_OC_Out.Columns.Add("序号", typeof(Int32));
            dt_OC_Out.Columns.Add("名称", typeof(String));
            dt_OC_Out.Columns.Add("脉宽", typeof(int));

            for (int i = 0; i < 32; i++)
            {
                DataRow dr = dt_OC_Out.NewRow();
                dr["序号"] = i + 1;
                dr["名称"] = Function.GetConfigStr(Data.OCconfigPath, "add", "OC_Channel_" + i.ToString(), "name");
                dr["脉宽"] = Function.GetConfigStr(Data.OCconfigPath, "add", "OC_Channel_" + i.ToString(), "value");
                dt_OC_Out.Rows.Add(dr);
            }
        }

        
    }
}
