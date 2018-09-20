using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace CKServer
{
    class Func_YC
    {
        public static DataTable dt_YC1 = new DataTable();
        public static DataTable dt_YC2 = new DataTable();

        public static void Init_Table()
        {
            dt_YC1.Columns.Add("序号", typeof(string));
            dt_YC1.Columns.Add("名称", typeof(string));
            dt_YC1.Columns.Add("占位", typeof(string));
            dt_YC1.Columns.Add("值", typeof(string));
            dt_YC1.Columns.Add("解析值", typeof(string));

            dt_YC2.Columns.Add("序号", typeof(string));
            dt_YC2.Columns.Add("名称", typeof(string));
            dt_YC2.Columns.Add("占位", typeof(string));
            dt_YC2.Columns.Add("值", typeof(string));
            dt_YC2.Columns.Add("解析值", typeof(string));

            List<string> List = Function.GetConfigNormal(Data.YCconfigPath, "add");
            int TotalLen = 0;
            if (List.Count >= 1)
            {
                for (int i = 0; i < List.Count; i++)
                {
                    DataRow dr = dt_YC1.NewRow();
                    dr["序号"] = i + 1;
                    dr["名称"] = List[i];
                    dr["占位"] = Function.GetConfigStr(Data.YCconfigPath, "add", List[i], "len");
                    dt_YC1.Rows.Add(dr);

                    DataRow dr2 = dt_YC2.NewRow();
                    dr2["序号"] = i + 1;
                    dr2["名称"] = List[i];
                    dr2["占位"] = Function.GetConfigStr(Data.YCconfigPath, "add", List[i], "len");
                    dt_YC2.Rows.Add(dr2);

                }
            }

        }

        public static bool Return_YCValue(ref List<byte> YCList_A,int ChanNo, ref string[] dataRe_YCA1,ref string[] dataRe_YCA2)
        {

            DataTable mydt_YC;
            switch(ChanNo)
            {
                case 1:
                    mydt_YC = dt_YC1;
                    break;
                case 2:
                    mydt_YC = dt_YC2;
                    break;
                default:
                    mydt_YC = dt_YC1;
                    break;
            }

            int YCFrame = 16;//16Byte 头2Byte 长度1Byte 11byte数据 2Byte累加和

            int _StartPos = YCList_A.IndexOf(0x14);

            if (_StartPos >= 0 && YCList_A.Count >= (_StartPos + YCFrame))
            {
                if (YCList_A[_StartPos + 1] == 0x6f && YCList_A[_StartPos + 2]==0x0D)
                {
                    string tempstr = "";//将YC数据转化为二进制string
                    for (int t = 0; t < 11; t++)
                    {
                        tempstr += Convert.ToString(YCList_A[_StartPos + 3 + t], 2).PadLeft(8, '0');
                    }                   
                    lock (YCList_A)
                    {
                        YCList_A.RemoveRange(0, _StartPos + YCFrame);
                    }

                    try
                    {                       
                        for (int j = 0; j < mydt_YC.Rows.Count; j++)
                        {
                            int len = int.Parse((string)mydt_YC.Rows[j]["占位"]);

                            long t = Convert.ToInt64(tempstr.Substring(0, len), 2);

                            int padleft = int.Parse((string)mydt_YC.Rows[j]["占位"]);

                            if (padleft == 8 || padleft == 16 || padleft == 32 || padleft == 48)
                            {
                                padleft = 2 * (padleft / 8);
                            }
                            else
                            {
                                padleft = 2 * (padleft / 8) + 2;
                            }

                            mydt_YC.Rows[j]["值"] = "0x" + t.ToString("x").PadLeft(padleft, '0');
                            mydt_YC.Rows[j]["解析值"] = "0x" + t.ToString("x").PadLeft(padleft, '0');

                            //dataRe_YCA1[j] = "0x" + t.ToString("x").PadLeft(padleft, '0');
                            //dataRe_YCA1[j] = "0x" + t.ToString("x").PadLeft(padleft, '0');
                            return true;
                        }

                    }
                    catch (Exception ex)
                    {
                        MyLog.Error("Return_YCValue:" + ex.Message);
                        return false;
                    }
                }
            }
            else
            {
                if (YCList_A.Count >= 1024)
                {
                    lock (YCList_A)
                    {
                        YCList_A.Clear();
                    }
                }
            }

            return false;

        }         

        


    }
}
