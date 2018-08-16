using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace CKServer
{
    class Func_LVDS
    {
        public static DataTable dt_LVDS = new DataTable();
        public static int LVDSNums = 8;

        public static int[] RecvCountsList = new int[8];
        public static int[] ComPareLenthList = new int[8];
        public static int[] ErrorRowList = new int[8];
        public static int[] ErrorColumnList = new int[8];

        public static bool LVDS_ComPareTag = false;

        public struct RecvLVDS_Struct
        {
            public int RecvCounts;
            public int ComPareLenth;
            public bool GetComPareDataTag;
            public int ErrorRow;
            public int ErrorColumn;
            public List<byte> RealBuf;
            public List<byte> DelayBuf;
            public List<byte> RecvBuf;
            public ReaderWriterLockSlim Lock;
            public string ChanName;
            public bool NeedComPareTag;

            public void init(bool NeedComPareTag)
            {
                this.RecvCounts = 0;
                this.ComPareLenth = 0;
                this.GetComPareDataTag = true;
                this.ErrorRow = 0;
                this.ErrorColumn = 0;
                this.RealBuf = new List<byte>(100*1024*1024);
                this.DelayBuf = new List<byte>(80 * 1024 * 1024);
                this.RecvBuf = new List<byte>(80 * 1024 * 1024);
                this.ChanName = "Undefined";
                this.Lock = new ReaderWriterLockSlim();
                this.NeedComPareTag = NeedComPareTag;

                //    this.RealBuf.Clear();
                //    this.DelayBuf.Clear();
                //    this.RecvBuf.Clear();
            }
        }
        public static RecvLVDS_Struct RecvLVDS_Chan1 = new RecvLVDS_Struct();
        public static RecvLVDS_Struct RecvLVDS_Chan2 = new RecvLVDS_Struct();
        public static RecvLVDS_Struct RecvLVDS_Chan3 = new RecvLVDS_Struct();
        public static RecvLVDS_Struct RecvLVDS_Chan4 = new RecvLVDS_Struct();
        public static RecvLVDS_Struct RecvLVDS_Chan5 = new RecvLVDS_Struct();
        public static RecvLVDS_Struct RecvLVDS_Chan6 = new RecvLVDS_Struct();
        public static RecvLVDS_Struct RecvLVDS_Chan7 = new RecvLVDS_Struct();
        public static RecvLVDS_Struct RecvLVDS_Chan8 = new RecvLVDS_Struct();

        public static void Init()
        {
            bool t1 = Convert.ToBoolean(Function.GetConfigStr(Data.LVDSconfigPath, "add", "LVDS_CompareLen_Chan_1", "compare"));
            bool t2 = Convert.ToBoolean(Function.GetConfigStr(Data.LVDSconfigPath, "add", "LVDS_CompareLen_Chan_2", "compare"));
            bool t3 = Convert.ToBoolean(Function.GetConfigStr(Data.LVDSconfigPath, "add", "LVDS_CompareLen_Chan_3", "compare"));
            bool t4 = Convert.ToBoolean(Function.GetConfigStr(Data.LVDSconfigPath, "add", "LVDS_CompareLen_Chan_4", "compare"));
            bool t5 = Convert.ToBoolean(Function.GetConfigStr(Data.LVDSconfigPath, "add", "LVDS_CompareLen_Chan_5", "compare"));
            bool t6 = Convert.ToBoolean(Function.GetConfigStr(Data.LVDSconfigPath, "add", "LVDS_CompareLen_Chan_6", "compare"));
            bool t7 = Convert.ToBoolean(Function.GetConfigStr(Data.LVDSconfigPath, "add", "LVDS_CompareLen_Chan_7", "compare"));
            bool t8 = Convert.ToBoolean(Function.GetConfigStr(Data.LVDSconfigPath, "add", "LVDS_CompareLen_Chan_8", "compare"));

            RecvLVDS_Chan1.init(t1);
            RecvLVDS_Chan2.init(t2);
            RecvLVDS_Chan3.init(t3);
            RecvLVDS_Chan4.init(t4);
            RecvLVDS_Chan5.init(t5);
            RecvLVDS_Chan6.init(t6);
            RecvLVDS_Chan7.init(t7);
            RecvLVDS_Chan8.init(t8);

            RecvLVDS_Chan1.ChanName = "A1";
            RecvLVDS_Chan2.ChanName = "A2";
            RecvLVDS_Chan3.ChanName = "A3";
            RecvLVDS_Chan4.ChanName = "A4：";
            RecvLVDS_Chan5.ChanName = "B1";
            RecvLVDS_Chan6.ChanName = "B2";
            RecvLVDS_Chan7.ChanName = "B3";
            RecvLVDS_Chan8.ChanName = "B4";

            RecvLVDS_Chan1.ComPareLenth = (int)dt_LVDS.Rows[0]["比对长度"];
            RecvLVDS_Chan2.ComPareLenth = (int)dt_LVDS.Rows[1]["比对长度"];
            RecvLVDS_Chan3.ComPareLenth = (int)dt_LVDS.Rows[2]["比对长度"];
            RecvLVDS_Chan4.ComPareLenth = (int)dt_LVDS.Rows[3]["比对长度"];
            RecvLVDS_Chan5.ComPareLenth = (int)dt_LVDS.Rows[4]["比对长度"];
            RecvLVDS_Chan6.ComPareLenth = (int)dt_LVDS.Rows[5]["比对长度"];
            RecvLVDS_Chan7.ComPareLenth = (int)dt_LVDS.Rows[6]["比对长度"];
            RecvLVDS_Chan8.ComPareLenth = (int)dt_LVDS.Rows[7]["比对长度"];

            LVDS_ComPareTag = true;


            if (RecvLVDS_Chan1.NeedComPareTag) new Thread(() => { RealTime_ComPare2(ref RecvLVDS_Chan1); }).Start();
            if (RecvLVDS_Chan2.NeedComPareTag) new Thread(() => { RealTime_ComPare2(ref RecvLVDS_Chan2); }).Start();
            if (RecvLVDS_Chan3.NeedComPareTag) new Thread(() => { RealTime_ComPare2(ref RecvLVDS_Chan3); }).Start();
            if (RecvLVDS_Chan4.NeedComPareTag) new Thread(() => { RealTime_ComPare2(ref RecvLVDS_Chan4); }).Start();
            if (RecvLVDS_Chan5.NeedComPareTag) new Thread(() => { RealTime_ComPare2(ref RecvLVDS_Chan5); }).Start();
            if (RecvLVDS_Chan6.NeedComPareTag) new Thread(() => { RealTime_ComPare2(ref RecvLVDS_Chan6); }).Start();
            if (RecvLVDS_Chan7.NeedComPareTag) new Thread(() => { RealTime_ComPare2(ref RecvLVDS_Chan7); }).Start();
            if (RecvLVDS_Chan8.NeedComPareTag) new Thread(() => { RealTime_ComPare2(ref RecvLVDS_Chan8); }).Start();

        }

        public static void Init_Table()
        {
            dt_LVDS.Columns.Add("序号", typeof(Int32));
            dt_LVDS.Columns.Add("名称", typeof(String));
            dt_LVDS.Columns.Add("收到数据", typeof(Int32));
            dt_LVDS.Columns.Add("比对长度", typeof(Int32));
            dt_LVDS.Columns.Add("出错行", typeof(Int32));
            dt_LVDS.Columns.Add("出错列", typeof(Int32));

            try
            {
                for (int i = 0; i < LVDSNums; i++)
                {
                    DataRow dr = dt_LVDS.NewRow();
                    dr["序号"] = i + 1;
                    dr["名称"] = Function.GetConfigStr(Data.LVDSconfigPath, "add", "LVDS_Channel_" + i.ToString(), "name");
                    dr["收到数据"] = 0;
                    dr["比对长度"] = int.Parse(Function.GetConfigStr(Data.LVDSconfigPath, "add", "LVDS_CompareLen_Chan_" + (i + 1).ToString(), "value"));
                    dr["出错行"] = 0;
                    dr["出错列"] = 0;
                    dt_LVDS.Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }


        public static void RealTime_ComPare2(ref RecvLVDS_Struct TempRecvChan)
        {
            Trace.WriteLine("RealTime_ComPare2 in");

            int CPlen = TempRecvChan.ComPareLenth;
            byte[] CPdata = new byte[CPlen];//比对数组长度

            while (LVDS_ComPareTag)
            {
                if (TempRecvChan.RecvBuf.Count() >= 1024)
                {
                    byte[] CADU = TempRecvChan.RecvBuf.Take(1024).ToArray();

                    TempRecvChan.Lock.EnterReadLock();
                    
                    TempRecvChan.RecvBuf.RemoveRange(0, 1024);

                    TempRecvChan.Lock.ExitReadLock();

                    TempRecvChan.RecvCounts += 1;//每1K增加计数1
                    byte Tag = (byte)(CADU[9] & 0x80);

                    byte[] data = new byte[862];//数据域-数据区
                    Array.Copy(CADU, 34, data, 0, 862);


                    int Erow = TempRecvChan.RecvCounts;
                    int Ecol = 0;
                    int ETemprow = 0;

                    if (Tag == 0x80)//回放数据--延时遥测
                    {
                        TempRecvChan.DelayBuf.AddRange(data);

                        if (TempRecvChan.GetComPareDataTag)
                        {
                            for (int j = 0; j < CPlen; j++) CPdata[j] = TempRecvChan.DelayBuf[j];
                            TempRecvChan.GetComPareDataTag = false;
                        }
                        //循环处理数据域data
                        while (TempRecvChan.DelayBuf.Count() >= CPlen)
                        {
                            for (int t = 0; t < CPlen; t++)
                            {
                                if (TempRecvChan.DelayBuf[t] != CPdata[t])
                                {
                                    Ecol = t + 1 + 34 + ETemprow * CPlen;
                                    TempRecvChan.ErrorColumn = Ecol;
                                    TempRecvChan.ErrorRow = Erow;
                                }
                            }
                            ETemprow++;
                            TempRecvChan.DelayBuf.RemoveRange(0, CPlen);
                        }
                    }
                    else//Tag==0x00,实时遥测
                    {
                        TempRecvChan.RealBuf.AddRange(data);

                        if (TempRecvChan.GetComPareDataTag)
                        {
                            for (int j = 0; j < CPlen; j++) CPdata[j] = TempRecvChan.RealBuf[j];
                            TempRecvChan.GetComPareDataTag = false;
                        }
                        //循环处理数据域data
                        while (TempRecvChan.RealBuf.Count() >= CPlen)
                        {
                            for (int t = 0; t < CPlen; t++)
                            {
                                if (TempRecvChan.RealBuf[t] != CPdata[t])
                                {
                                    Ecol = t + 1 + 34 + ETemprow * CPlen;
                                    TempRecvChan.ErrorColumn = Ecol;
                                    TempRecvChan.ErrorRow = Erow;

                                    //   ErrorLog.Error(TempRecvChan.ChanName+":" + Erow.ToString()+"行" + Ecol.ToString()+"列");
                                }
                                CPdata[t] = TempRecvChan.RealBuf[t];
                            }
                            ETemprow++;
                            TempRecvChan.RealBuf.RemoveRange(0, CPlen);
                        }

                    }
                }
                else
                {
                    Trace.WriteLine("RealTime_ComPare2 TempRecvChan nums <1024");
                    Thread.Sleep(1000);
                }
            }

            Trace.WriteLine("RealTime_ComPare2 out");
        }

    }
}
