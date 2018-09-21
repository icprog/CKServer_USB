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

        public static List<string> YCKeyList = Function.GetConfigNormal(Data.YCconfigPath, "add");

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


            //   int TotalLen = 0;

            if (YCKeyList.Count >= 1)
            {
                for (int i = 0; i < YCKeyList.Count; i++)
                {
                    DataRow dr = dt_YC1.NewRow();
                    dr["序号"] = i + 1;
                    dr["名称"] = YCKeyList[i];
                    dr["占位"] = Function.GetConfigStr(Data.YCconfigPath, "add", YCKeyList[i], "len");
                    dt_YC1.Rows.Add(dr);

                    DataRow dr2 = dt_YC2.NewRow();
                    dr2["序号"] = i + 1;
                    dr2["名称"] = YCKeyList[i];
                    dr2["占位"] = Function.GetConfigStr(Data.YCconfigPath, "add", YCKeyList[i], "len");
                    dt_YC2.Rows.Add(dr2);

                }
            }

        }

        public static bool Return_YCValue(ref DataTable dt, ref List<byte> YCList_A, int ChanNo, ref string[] dataRe_YCA1, ref string[] dataRe_YCA2)
        {

            DataTable mydt_YC = dt;

            int YCFrame = 16;//16Byte 头2Byte 长度1Byte 11byte数据 2Byte累加和

            int _StartPos = YCList_A.IndexOf(0x14);

            if (_StartPos >= 0 && YCList_A.Count >= (_StartPos + YCFrame))
            {
                if (YCList_A[_StartPos + 1] == 0x6f && YCList_A[_StartPos + 2] == 0x0D)
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
                    MyLog.Info(tempstr);

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
                            //   mydt_YC.Rows[j]["解析值"] = "0x" + t.ToString("x").PadLeft(padleft, '0');

                            string fmul = Function.GetConfigStr(Data.YCconfigPath, "add", YCKeyList[j], "fmul");
                            switch (fmul)
                            {
                                case "允许/禁止":
                                    if (t == 1)
                                    {
                                        mydt_YC.Rows[j]["解析值"] = "允许";
                                    }
                                    else
                                    {
                                        mydt_YC.Rows[j]["解析值"] = "禁止";
                                    }
                                    break;
                                case "正确/错误":
                                    if (t == 0x00)
                                    {
                                        mydt_YC.Rows[j]["解析值"] = "正确";
                                    }
                                    else if (t == 0x11)
                                    {
                                        mydt_YC.Rows[j]["解析值"] = "错误";
                                    }
                                    else
                                    {
                                        mydt_YC.Rows[j]["解析值"] = "出现异常值";
                                    }
                                    break;
                                case "码速率状态":
                                    if (t == 0x100)
                                    {
                                        mydt_YC.Rows[j]["解析值"] = "30Mbps";
                                    }
                                    else if (t == 0x011)
                                    {
                                        mydt_YC.Rows[j]["解析值"] = "15Mbps";
                                    }
                                    else if (t == 0x010)
                                    {
                                        mydt_YC.Rows[j]["解析值"] = "10Mbps";
                                    }
                                    else if (t == 0x001)
                                    {
                                        mydt_YC.Rows[j]["解析值"] = "2Mbps";
                                    }
                                    else if (t == 0x000)
                                    {
                                        mydt_YC.Rows[j]["解析值"] = "1Mbps";
                                    }
                                    else
                                    {
                                        mydt_YC.Rows[j]["解析值"] = "出现异常值";
                                    }
                                    break;
                                case "捕获/未捕获":
                                    if (t == 1)
                                    {
                                        mydt_YC.Rows[j]["解析值"] = "捕获";
                                    }
                                    else
                                    {
                                        mydt_YC.Rows[j]["解析值"] = "未捕获";
                                    }
                                    break;
                                case "锁定/失锁":
                                    if (t == 1)
                                    {
                                        mydt_YC.Rows[j]["解析值"] = "锁定";
                                    }
                                    else
                                    {
                                        mydt_YC.Rows[j]["解析值"] = "失锁";
                                    }
                                    break;
                                case "工作/不工作":
                                    if (t == 1)
                                    {
                                        mydt_YC.Rows[j]["解析值"] = "工作";
                                    }
                                    else
                                    {
                                        mydt_YC.Rows[j]["解析值"] = "不工作";
                                    }
                                    break;
                                case "orignal*50Hz":
                                    int data = (int)t;
                                    if (data >> 15 == 0x1)//最高为为1
                                    {
                                        data = ~data + 1;
                                        float show_data1 = data;
                                        show_data1 = show_data1 / 20;
                                        mydt_YC.Rows[j]["解析值"] = "-" + show_data1.ToString("f2") + "kHz";
                                    }
                                    else
                                    {
                                        float show_data2 = t;
                                        show_data2 = show_data2 / 20;
                                        mydt_YC.Rows[j]["解析值"] = "+" + show_data2.ToString("f2") + "kHz";
                                    }
                                    break;
                                case "orignal*0.5Hz":
                                    float show_data3 = t;
                                    show_data3 = show_data3 / 2;
                                    mydt_YC.Rows[j]["解析值"] = "+" + show_data3.ToString("f1") + "dB";
                                    break;
                                case "orignal":
                                    mydt_YC.Rows[j]["解析值"] = "0x" + t.ToString("x").PadLeft(padleft, '0');
                                    break;
                                default:
                                    break;
                            }
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
                return false;
            }

            return true;

        }




    }
}
