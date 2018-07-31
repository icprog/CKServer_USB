using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

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

            public void init()
            {
                this.RecvCounts = 0;
                this.ComPareLenth = 0;
                this.GetComPareDataTag = true;
                this.ErrorRow = 0;
                this.ErrorColumn = 0;
                this.RealBuf = new List<byte>();
                this.DelayBuf = new List<byte>();
                this.RecvBuf = new List<byte>();
                this.ChanName = "Undefined";
                this.Lock = new ReaderWriterLockSlim();
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
            RecvLVDS_Chan1.init();
            RecvLVDS_Chan2.init();
            RecvLVDS_Chan3.init();
            RecvLVDS_Chan4.init();
            RecvLVDS_Chan5.init();
            RecvLVDS_Chan6.init();
            RecvLVDS_Chan7.init();
            RecvLVDS_Chan8.init();

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
            //new Thread(() => { RealTime_ComPare(1); }).Start();
            //new Thread(() => { RealTime_ComPare(2); }).Start();
            //new Thread(() => { RealTime_ComPare(3); }).Start();
            //new Thread(() => { RealTime_ComPare(4); }).Start();
            //  new Thread(() => { RealTime_ComPare(5); }).Start();
            //  new Thread(() => { RealTime_ComPare(6); }).Start();
            //new Thread(() => { RealTime_ComPare(7); }).Start();
            //new Thread(() => { RealTime_ComPare(8); }).Start();

            new Thread(() => { RealTime_ComPare2(ref RecvLVDS_Chan1); }).Start();
            new Thread(() => { RealTime_ComPare2(ref RecvLVDS_Chan2); }).Start();
            new Thread(() => { RealTime_ComPare2(ref RecvLVDS_Chan3); }).Start();
            new Thread(() => { RealTime_ComPare2(ref RecvLVDS_Chan4); }).Start();
            new Thread(() => { RealTime_ComPare2(ref RecvLVDS_Chan5); }).Start();
            new Thread(() => { RealTime_ComPare2(ref RecvLVDS_Chan6); }).Start();
            new Thread(() => { RealTime_ComPare2(ref RecvLVDS_Chan7); }).Start();
            new Thread(() => { RealTime_ComPare2(ref RecvLVDS_Chan8); }).Start();
        }

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
                dr["比对长度"] = 20;
                dr["出错行"] = 0;
                dr["出错列"] = 0;
                dt_LVDS.Rows.Add(dr);
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
                    byte[] CADU = new byte[1024];

                    for (int i = 0; i < 1024; i++) CADU[i] = TempRecvChan.RecvBuf[i];

                    TempRecvChan.Lock.EnterReadLock();
                    TempRecvChan.RecvBuf.RemoveRange(0, 1024);
                    TempRecvChan.Lock.ExitReadLock();

                    TempRecvChan.RecvCounts += 1024;
                    byte Tag = (byte)(CADU[9] & 0x80);

                    byte[] data = new byte[862];//数据域-数据区
                    Array.Copy(CADU, 34, data, 0, 862);


                    int Erow = TempRecvChan.RecvCounts / 1024; ;
                    int Ecol = 0;
                    int ETemprow = 0;

                    if (Tag == 0x80)//回放数据--延时遥测
                    {
                        for (int i = 0; i < 862; i++) TempRecvChan.DelayBuf.Add(data[i]);
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
                        for (int i = 0; i < 862; i++) TempRecvChan.RealBuf.Add(data[i]);
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


        public static void RealTime_ComPare(int ChanNo)
        {
            Trace.WriteLine("RealTime_ComPare:" + ChanNo.ToString());
            RecvLVDS_Struct TempRecvChan;
            switch (ChanNo)
            {
                case 1:
                    TempRecvChan = RecvLVDS_Chan1;
                    break;
                case 2:
                    TempRecvChan = RecvLVDS_Chan2;
                    break;
                case 3:
                    TempRecvChan = RecvLVDS_Chan3;
                    break;
                case 4:
                    TempRecvChan = RecvLVDS_Chan4;
                    break;
                case 5:
                    TempRecvChan = RecvLVDS_Chan5;
                    break;
                case 6:
                    TempRecvChan = RecvLVDS_Chan6;
                    break;
                case 7:
                    TempRecvChan = RecvLVDS_Chan7;
                    break;
                case 8:
                    TempRecvChan = RecvLVDS_Chan8;
                    break;
                default:
                    TempRecvChan = RecvLVDS_Chan1;
                    break;
            }

            while (LVDS_ComPareTag)
            {
                if (TempRecvChan.RecvBuf.Count() >= 1024)
                {
                    byte[] CADU = new byte[1024];

                    for (int i = 0; i < 1024; i++) CADU[i] = TempRecvChan.RecvBuf[i];
                    lock (TempRecvChan.RecvBuf)
                        TempRecvChan.RecvBuf.RemoveRange(0, 1024);

                    TempRecvChan.RecvCounts += 1024;

                    byte Tag = (byte)(CADU[9] & 0x80);

                    byte[] data = new byte[886];//数据域
                    int CPlen = TempRecvChan.ComPareLenth;
                    byte[] CPdata = new byte[CPlen];//比对数组长度


                    int Erow = TempRecvChan.RecvCounts / 1024; ;
                    int Ecol = 10;
                    int ETemprow = 0;

                    if (Tag == 0x80)//回放数据--延时遥测
                    {
                        for (int i = 0; i < 886; i++) TempRecvChan.DelayBuf.Add(data[i]);
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
                                    Ecol = t + 1 + ETemprow * CPlen;
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
                        for (int i = 0; i < 886; i++) TempRecvChan.RealBuf.Add(data[i]);
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
                                    Ecol = t + 1 + ETemprow * CPlen;
                                    TempRecvChan.ErrorColumn = Ecol;
                                    TempRecvChan.ErrorRow = Erow;
                                }
                            }
                            ETemprow++;
                            TempRecvChan.RealBuf.RemoveRange(0, CPlen);
                        }

                    }






                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

    }
}
