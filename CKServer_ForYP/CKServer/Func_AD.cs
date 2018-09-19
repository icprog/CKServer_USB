using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CKServer
{
    class Func_AD
    {
        public static DataTable dt_AD = new DataTable();

        public static int ADNums = 22;

        public static void Init_Table()
        {
            dt_AD.Columns.Add("序号", typeof(Int32));
            dt_AD.Columns.Add("名称", typeof(String));
            dt_AD.Columns.Add("测量值", typeof(Double));
            dt_AD.Columns.Add("单位", typeof(String));
            for (int i = 0; i < ADNums; i++)
            {
                DataRow dr = dt_AD.NewRow();
                dr["序号"] = i + 1;
                dr["名称"] = Function.GetConfigStr(Data.ADconfigPath, "add", "AD_Channel_" + i.ToString(), "name");
                dr["测量值"] = 0;
                dr["单位"] = Function.GetConfigStr(Data.ADconfigPath, "add", "AD_Channel_" + i.ToString(), "dw");
                dt_AD.Rows.Add(dr);
            }
        }

        /// <summary>
        /// 根据输入AD源码解析出实际AD值，解析成功返回true否则false
        /// </summary>
        /// <param name="ADList">包含AD数据的帧</param>
        /// <param name="dataRe_AD">解析后的double类型AD数据</param>
        /// <param name="md">校准参数</param>
        /// <returns></returns>
        public static bool Return_ADValue(ref List<byte> ADList, ref double[] dataRe_AD, double[] md)
        {
            int ADFrame = 256;//64channel=256bytes
            int NChans = 64 * 4;//64channel=256bytes

            int _StartPos = ADList.IndexOf(0xAD);

            if (_StartPos >= 0 && ADList.Count >= (_StartPos + ADFrame))
            {
                if (ADList[_StartPos + 1] == 0x00 && ADList[_StartPos + 4] == 0xAD && ADList[_StartPos + 5] == 0x01)
                {
                    byte[] buf = new byte[NChans];
                    for (int t = 0; t < NChans; t++)
                    {
                        buf[t] = ADList[_StartPos + t];
                    }


                    for (int k = 0; k < ADNums; k++)
                    {
                        int temp = (buf[4 * k + 3] & 0x7f) * 256 + buf[4 * k + 2];

                        double value = temp;

                        value = 10 * (value / 32767);
                        
                        double RealValue = 0;//最终实际电压值，有正负
                        if ((buf[4 * k+3] & 0x80) == 0x80)
                            RealValue = value;
                        else
                            RealValue = -10 + value;

                        if (k < 12)//前12路换算为温度
                        {
                            if (RealValue != 3.3)
                            {
                                double RValue = 0;//根据电压值算出电阻值
                                RValue = 5.1 * RealValue / (3.3 - RealValue);

                                if (RValue > 0)
                                {
                                    double XValue = 0;//用于计算温度，代入公式的x值
                                    XValue = Math.Log10(RValue);

                                    double YTemprature = 0;//最终显示的温度
                                    YTemprature = -1.246 * XValue * XValue * XValue + 10.83 * XValue * XValue - 63.72 * XValue + 64.67;

                                    dataRe_AD[k] = YTemprature;
                                }
                                else
                                {
                                    dataRe_AD[k] = double.PositiveInfinity;
                                    MyLog.Error("当前电阻小于0，未接负载，请注意，请注意，请注意！！");
                                }
                            }
                            else
                            {
                                dataRe_AD[k] = double.PositiveInfinity;
                                MyLog.Error("当前电压3.3V，未接负载，请注意，请注意，请注意！！");
                            }
                        }
                        else//正常显示V
                        {
                            dataRe_AD[k] = RealValue;
                        }
                    }



                    lock (ADList)
                    {
                        ADList.RemoveRange(0, _StartPos + ADFrame);
                    }
                    return true;
                }
                else
                {
                    ADList.RemoveRange(0, _StartPos + 1);
                    return false;
                }
            }
            else
            {
                if (ADList.Count >= 8192)
                {
                    lock (ADList)
                    {
                        ADList.Clear();
                    }
                }
                return false;
            }

        }

    }
}
