using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CKServer
{
    class Function_LVDS
    {
        public static DataTable dt_LVDS = new DataTable();
        public static int LVDSNums = 8;

        public static int[] RecvCountsList = new int[8];
        public static int[] ComPareLenthList = new int[8];
        public static int[] ErrorRowList = new int[8];
        public static int[] ErrorColumnList = new int[8];

        public static void Init_Table()
        {
            dt_LVDS.Columns.Add("序号", typeof(Int32));
            dt_LVDS.Columns.Add("名称", typeof(String));
            dt_LVDS.Columns.Add("收到数据", typeof(Int32));
            dt_LVDS.Columns.Add("比对长度", typeof(Int32));
            dt_LVDS.Columns.Add("出错行", typeof(Int32));
            dt_LVDS.Columns.Add("出错列", typeof(Int32));

            for (int i = 0; i < LVDSNums; i++)
            {
                DataRow dr = dt_LVDS.NewRow();
                dr["序号"] = i + 1;
                dr["名称"] = Function.GetConfigStr(Data.LVDSconfigPath, "add", "LVDS_Channel_" + i.ToString(), "name");
                dr["收到数据"] = 0;
                dr["比对长度"] = 0;
                dr["出错行"] = 0;
                dr["出错列"] = 0;
                dt_LVDS.Rows.Add(dr);
            }
        }


        public void RealTime_ComPare()
        {
            byte[] CADU = new byte[1024];

            byte[] data = new byte[886];//数据域


            //循环处理数据域data

        }

    }
}
