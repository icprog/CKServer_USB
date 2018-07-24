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

        public static int OCOutChanNums = 22;

        public static void Init_Table()
        {
            dt_OC_In1.Columns.Add("序号", typeof(Int32));
            dt_OC_In1.Columns.Add("名称", typeof(String));
            dt_OC_In1.Columns.Add("计数", typeof(Int32));
            dt_OC_In1.Columns.Add("脉宽", typeof(Int32));
            for(int i=0;i<162;i++)
            {
                DataRow dr = dt_OC_In1.NewRow();
                dr["序号"] = i + 1;
                dr["名称"] = Function.GetConfigStr(Data.OCconfigPath, "OCborad_in1", "OC_Channel_" + i.ToString(), "name");
                dr["计数"] = 0;
                dr["脉宽"] = 0;
                dt_OC_In1.Rows.Add(dr);
            }

            dt_OC_In2.Columns.Add("序号", typeof(Int32));
            dt_OC_In2.Columns.Add("名称", typeof(String));
            dt_OC_In2.Columns.Add("计数", typeof(Int32));
            dt_OC_In2.Columns.Add("脉宽", typeof(Int32));
            for (int i = 0; i < 162; i++)
            {
                DataRow dr = dt_OC_In2.NewRow();
                dr["序号"] = i + 1;
                dr["名称"] = Function.GetConfigStr(Data.OCconfigPath, "OCborad_in2", "OC_Channel_" + i.ToString(), "name");
                dr["计数"] = 0;
                dr["脉宽"] = 0;
                dt_OC_In2.Rows.Add(dr);
            }

            dt_OC_In3.Columns.Add("序号", typeof(Int32));
            dt_OC_In3.Columns.Add("名称", typeof(String));
            dt_OC_In3.Columns.Add("计数", typeof(Int32));
            dt_OC_In3.Columns.Add("脉宽", typeof(Int32));
            for (int i = 0; i < 162; i++)
            {
                DataRow dr = dt_OC_In3.NewRow();
                dr["序号"] = i +1;
                dr["名称"] = Function.GetConfigStr(Data.OCconfigPath, "OCborad_in3", "OC_Channel_" + i.ToString(), "name");
                dr["计数"] = 0;
                dr["脉宽"] = 0;
                dt_OC_In3.Rows.Add(dr);
            }

            dt_OC_Out.Columns.Add("序号", typeof(Int32));
            dt_OC_Out.Columns.Add("名称", typeof(String));
            dt_OC_Out.Columns.Add("脉宽", typeof(int));


            for (int i = 0; i < OCOutChanNums; i++)
            {
                DataRow dr = dt_OC_Out.NewRow();
                dr["序号"] = i + 1;
                dr["名称"] = Function.GetConfigStr(Data.OCconfigPath, "add", "OC_Channel_" + i.ToString(), "name");
                dr["脉宽"] = Function.GetConfigStr(Data.OCconfigPath, "add", "OC_Channel_" + i.ToString(), "value");
                dt_OC_Out.Rows.Add(dr);
            }
        }

        public static void Send2OCBD(byte[] OCData)
        {
            byte[] Send2OCBd = new byte[37];
            Send2OCBd[0] = 0x1D;
            Send2OCBd[1] = 0x00;
            Send2OCBd[2] = 0x00;
            Send2OCBd[3] = 0x1D;//24+5 1D000025是机箱帧头
            Send2OCBd[4] = 0xBD;
            Send2OCBd[5] = 0x1D;
            Send2OCBd[6] = 0x00;
            Send2OCBd[7] = 0x0C;//BD1D000C代表OC数据格式
            Send2OCBd[8] = 0x00;//00/01/02/03代表通道，此处使用OC
            OCData.CopyTo(Send2OCBd, 9);
            Send2OCBd[33] = 0xC0;
            Send2OCBd[34] = 0xDE;
            Send2OCBd[35] = 0xC0;
            Send2OCBd[36] = 0xDE;

            USB.SendCMD(Data.OCid, 0x81, 0x1);
            USB.SendCMD(Data.OCid, 0x81, 0x0);
            USB.SendData(Data.OCid, Send2OCBd);

        }


        public static bool Return_OCValue(ref List<byte> OCList, ref int[] dataRe_OC)
        {
            int OCFrame = 656;//162channel 648bytes,+4head,+4end
            int NChans = 162 * 4;//162channel=648bytes
            int _StartPos = OCList.IndexOf(0xB0);

            if (_StartPos >= 0 && OCList.Count >= (_StartPos + OCFrame))
            {
                if (OCList[_StartPos + 1] == 0xFA && OCList[_StartPos + OCFrame - 4] == 0xE0 && OCList[_StartPos + OCFrame - 3] == 0xF5)
                {
                    for (int t = 0; t < NChans/2; t++)
                    {
                        dataRe_OC[t] = OCList[_StartPos + 4 + 2*t]*256+ OCList[_StartPos + 4 + 2*t+1];
                    }
                    lock (OCList)
                    {
                        OCList.RemoveRange(0, _StartPos + OCFrame);
                    }
                }
                return true;
            }
            else
            {
                if (OCList.Count >= 8192)
                {
                    lock (OCList)
                    {
                        OCList.Clear();
                    }
                }
                return false;
            }

        }

    }
}
