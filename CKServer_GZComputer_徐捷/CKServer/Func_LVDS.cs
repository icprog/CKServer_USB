using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CKServer
{
    class Func_LVDS
    {
        public static DataTable dt_LVDS_01 = new DataTable();

        public static List<DataTable> dt_LVDS_List = new List<DataTable>();

        public static int LVDS_ComPare_Chan = 0;

        public static bool LVDS_ComPareTag = false;
        public static Dictionary<byte, RecvChan_VCID_Struct> myVcidDic1 = new Dictionary<byte, RecvChan_VCID_Struct>();

        public static int[] CPLenList = new int[32] { 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20,
        20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20};


        public static List<byte> NeedDispatchBuf = new List<byte>(100 * 1024 * 1024);
        public static ReaderWriterLockSlim DispatchLock = new ReaderWriterLockSlim();

        public struct RecvChan_VCID_Struct
        {
            public string ChanName;//通道名称

            //    public int RecvFrames;//帧计数
            public byte VCID;//VCID
            public byte Tag;//实时延时

            public List<byte> RealBuf;
            public int Real_ErrorRow;
            public int Real_ErrorCol;
            public int Real_RecvCounts;//收到数据
            public int Real_CPLen;

            public List<byte> DelayBuf;
            public int Delay_ErrorRow;
            public int Delay_ErrorCol;
            public int Delay_RecvCounts;//收到数据
            public int Delay_CPLen;

            public bool GetComPareDataTag;


            public void init(byte vcid, int real_cplen, int delay_cplen)
            {
                this.VCID = vcid;
                this.Real_RecvCounts = 0;
                this.Delay_RecvCounts = 0;
                this.GetComPareDataTag = true;

                this.Real_ErrorRow = 0;
                this.Real_ErrorCol = 0;
                this.Real_CPLen = real_cplen;
                this.Delay_ErrorRow = 0;
                this.Delay_ErrorCol = 0;
                this.Delay_CPLen = delay_cplen;

                this.RealBuf = new List<byte>(20 * 1024 * 1024);
                this.DelayBuf = new List<byte>(20 * 1024 * 1024);
                this.ChanName = "Undefined";
            }

            public void init2(int real_cplen, int delay_cplen)
            {
                this.Real_RecvCounts = 0;
                this.Delay_RecvCounts = 0;
                this.GetComPareDataTag = true;

                this.Real_ErrorRow = 0;
                this.Real_ErrorCol = 0;
                this.Real_CPLen = real_cplen;
                this.Delay_ErrorRow = 0;
                this.Delay_ErrorCol = 0;
                this.Delay_CPLen = delay_cplen;
            }
        }

        public static RecvChan_VCID_Struct VCID_01 = new RecvChan_VCID_Struct();
        public static RecvChan_VCID_Struct VCID_02 = new RecvChan_VCID_Struct();
        public static RecvChan_VCID_Struct VCID_03 = new RecvChan_VCID_Struct();
        public static RecvChan_VCID_Struct VCID_04 = new RecvChan_VCID_Struct();
        public static RecvChan_VCID_Struct VCID_05 = new RecvChan_VCID_Struct();
        public static RecvChan_VCID_Struct VCID_06 = new RecvChan_VCID_Struct();
        public static RecvChan_VCID_Struct VCID_07 = new RecvChan_VCID_Struct();
        public static RecvChan_VCID_Struct VCID_08 = new RecvChan_VCID_Struct();
        public static RecvChan_VCID_Struct VCID_09 = new RecvChan_VCID_Struct();
        public static RecvChan_VCID_Struct VCID_10 = new RecvChan_VCID_Struct();
        public static RecvChan_VCID_Struct VCID_11 = new RecvChan_VCID_Struct();
        public static RecvChan_VCID_Struct VCID_12 = new RecvChan_VCID_Struct();
        public static RecvChan_VCID_Struct VCID_13 = new RecvChan_VCID_Struct();
        public static RecvChan_VCID_Struct VCID_14 = new RecvChan_VCID_Struct();
        public static RecvChan_VCID_Struct VCID_15 = new RecvChan_VCID_Struct();
        public static RecvChan_VCID_Struct VCID_16 = new RecvChan_VCID_Struct();


        public static void Init_Table()
        {
            try
            {
                dt_LVDS_01.Columns.Add("序号", typeof(Int32));
                dt_LVDS_01.Columns.Add("通道名称", typeof(String));
                dt_LVDS_01.Columns.Add("实时/延时", typeof(String));
                dt_LVDS_01.Columns.Add("VCID", typeof(String));
                dt_LVDS_01.Columns.Add("帧计数", typeof(Int32));
                dt_LVDS_01.Columns.Add("收到数据", typeof(Int32));
                dt_LVDS_01.Columns.Add("比对长度", typeof(Int32));
                dt_LVDS_01.Columns.Add("出错行", typeof(Int32));
                dt_LVDS_01.Columns.Add("出错列", typeof(Int32));

                for (int i = 0; i < 32; i++)
                {
                    DataRow dr = dt_LVDS_01.NewRow();
                    dr["序号"] = (i / 2 + 1).ToString();
                    dr["通道名称"] = Function.GetConfigStr(Data.LVDSconfigPath, "LVDS_Channel_0", "VCID_Channel_" + (i / 2 + 1).ToString(), "name");
                    if (i % 2 == 0) dr["实时/延时"] = "实时";
                    if (i % 2 == 1) dr["实时/延时"] = "延时";
                    dr["VCID"] = Function.GetConfigStr(Data.LVDSconfigPath, "LVDS_Channel_0", "VCID_Channel_" + (i / 2 + 1).ToString(), "vcid");
                    dr["帧计数"] = 0;
                    dr["收到数据"] = 0;
                    dr["比对长度"] = int.Parse(Function.GetConfigStr(Data.LVDSconfigPath, "LVDS_Channel_0", "VCID_Channel_" + (i / 2 + 1).ToString(), "CPLen"));
                    dr["出错行"] = 0;
                    dr["出错列"] = 0;
                    dt_LVDS_01.Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message + "Frome LVDS init table");
            }
        }

        public static void Run()
        {

            myVcidDic1.Clear();
            
            myVcidDic1.Add(0x01, VCID_01);
            myVcidDic1.Add(0x02, VCID_02);
            myVcidDic1.Add(0x03, VCID_03);
            myVcidDic1.Add(0x04, VCID_04);
            myVcidDic1.Add(0x05, VCID_05);
            myVcidDic1.Add(0x06, VCID_06);
            myVcidDic1.Add(0x07, VCID_07);
            myVcidDic1.Add(0x08, VCID_08);
            myVcidDic1.Add(0x09, VCID_09);
            myVcidDic1.Add(0x0a, VCID_10);
            myVcidDic1.Add(0x0b, VCID_11);
            myVcidDic1.Add(0x0c, VCID_12);
            myVcidDic1.Add(0x0d, VCID_13);
            myVcidDic1.Add(0x0e, VCID_14);
            myVcidDic1.Add(0x0f, VCID_15);
            myVcidDic1.Add(0x10, VCID_16);



            VCID_01.init2(CPLenList[0], CPLenList[1]);
            VCID_02.init2(CPLenList[2], CPLenList[3]);
            VCID_03.init2(CPLenList[4], CPLenList[5]);
            VCID_04.init2(CPLenList[6], CPLenList[7]);
            VCID_05.init2(CPLenList[8], CPLenList[9]);
            VCID_06.init2(CPLenList[10], CPLenList[11]);

            VCID_07.init2(CPLenList[12], CPLenList[13]);
            VCID_08.init2(CPLenList[14], CPLenList[15]);
            VCID_09.init2(CPLenList[16], CPLenList[17]);
            VCID_10.init2(CPLenList[18], CPLenList[19]);
            VCID_11.init2(CPLenList[20], CPLenList[21]);
            VCID_12.init2(CPLenList[22], CPLenList[23]);

            VCID_13.init2(CPLenList[24], CPLenList[25]);
            VCID_14.init2(CPLenList[26], CPLenList[27]);
            VCID_15.init2(CPLenList[28], CPLenList[29]);
            VCID_16.init2(CPLenList[30], CPLenList[31]);


            LVDS_ComPareTag = true;

            NeedDispatchBuf.Clear();

            new Thread(() => { DisPatch_VCIDFrame(ref NeedDispatchBuf, myVcidDic1); }).Start();

            //new Thread(() => { RealTime_ComPare(ref VCID_01); }).Start();
            //new Thread(() => { RealTime_ComPare(ref VCID_02); }).Start();
            //new Thread(() => { RealTime_ComPare(ref VCID_03); }).Start();
            //new Thread(() => { RealTime_ComPare(ref VCID_04); }).Start();
            //new Thread(() => { RealTime_ComPare(ref VCID_05); }).Start();
            //new Thread(() => { RealTime_ComPare(ref VCID_06); }).Start();

            //new Thread(() => { RealTime_ComPare(ref VCID_07); }).Start();
            //new Thread(() => { RealTime_ComPare(ref VCID_08); }).Start();

            //new Thread(() => { RealTime_ComPare(ref VCID_09); }).Start();
            //new Thread(() => { RealTime_ComPare(ref VCID_10); }).Start();
            //new Thread(() => { RealTime_ComPare(ref VCID_11); }).Start();
            //new Thread(() => { RealTime_ComPare(ref VCID_12); }).Start();

            //new Thread(() => { RealTime_ComPare(ref VCID_13); }).Start();
            //new Thread(() => { RealTime_ComPare(ref VCID_14); }).Start();
            //new Thread(() => { RealTime_ComPare(ref VCID_15); }).Start();
            //new Thread(() => { RealTime_ComPare(ref VCID_16); }).Start();

        }

        public static void Stop()
        {
            LVDS_ComPareTag = false;
        }
        public static void Init()
        {


            VCID_01.init(0x1, CPLenList[0], CPLenList[1]);
            VCID_02.init(0x2, CPLenList[2], CPLenList[3]);
            VCID_03.init(0x3, CPLenList[4], CPLenList[5]);
            VCID_04.init(0x4, CPLenList[6], CPLenList[7]);
            VCID_05.init(0x5, CPLenList[8], CPLenList[9]);
            VCID_06.init(0x6, CPLenList[10], CPLenList[11]);

            VCID_07.init(0x7, CPLenList[12], CPLenList[13]);
            VCID_08.init(0x8, CPLenList[14], CPLenList[15]);
            VCID_09.init(0x9, CPLenList[16], CPLenList[17]);
            VCID_10.init(0x10, CPLenList[18], CPLenList[19]);
            VCID_11.init(0x11, CPLenList[20], CPLenList[21]);
            VCID_12.init(0x12, CPLenList[22], CPLenList[23]);

            VCID_13.init(0x13, CPLenList[24], CPLenList[25]);
            VCID_14.init(0x14, CPLenList[26], CPLenList[27]);
            VCID_15.init(0x15, CPLenList[28], CPLenList[29]);
            VCID_16.init(0x16, CPLenList[30], CPLenList[31]);



        }

        public static bool FirstFindTag = true;
        public static List<byte> APIDList;
        public static Dictionary<byte, BinaryWriter> myDictionary;
        public static void DisPatch_VCIDFrame(ref List<byte> DispatchBuf, Dictionary<byte, RecvChan_VCID_Struct> myVcidDic)
        {
            APIDList = new List<byte>();
            myDictionary = new Dictionary<byte, BinaryWriter>();

            Trace.WriteLine("DisPatch_VCIDFrame Thread in!");
            String Name = Function.GetConfigStr(Data.LVDSconfigPath, "LVDS_Channel_" + LVDS_ComPare_Chan.ToString(), "VCID_Channel_" + (LVDS_ComPare_Chan + 1).ToString(), "name");

            FirstFindTag = true;

            while (LVDS_ComPareTag)
            {
                if (DispatchBuf.Count() >= 2048)
                {
                    DispatchLock.EnterReadLock();
                    if (FirstFindTag)
                    {
                        FirstFindTag = false;
                        int t1 = DispatchBuf.IndexOf(0x1A);

                        if (DispatchBuf[t1 + 1] == 0xcf && DispatchBuf[t1 + 2] == 0xfc && DispatchBuf[t1 + 3] == 0x1d)
                        {
                            if (t1 != 0)
                            {
                                DispatchBuf.RemoveRange(0, t1);
                                MyLog.Error("帧头非1ACFFC1D,偏了" + t1.ToString() + "个数！已自动同步！");
                            }

                        }
                        

                    }
                    byte[] CADU = DispatchBuf.Take(1024).ToArray();
                    DispatchBuf.RemoveRange(0, 1024);
                    DispatchLock.ExitReadLock();
                    if (CADU[0] == 0x1A && CADU[1] == 0xcf)
                    {
                        byte Tag = (byte)(CADU[9] & 0x80);//延时-80H；实时-00H
                        byte Vcid = (byte)(CADU[5] & 0x3f);//VCID虚拟信道标识符

                        byte[] data = new byte[862];//数据域-数据区
                        Array.Copy(CADU, 34, data, 0, 862);

                        if (APIDList.IndexOf(Vcid) < 0)
                        {
                            APIDList.Add(Vcid);
                            String Path = Program.GetStartupPath() + @"存储数据\LVDS机箱数据\" + Name + @"\" + Convert.ToString(Vcid, 2).PadLeft(6, '0') + @"\";
                            if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);
                            DirectoryInfo folder = new DirectoryInfo(Path);
                            try
                            {
                                foreach (FileInfo tempfile in folder.GetFiles("*.*"))
                                {
                                    string name = tempfile.Name;
                                    if (tempfile.Length == 0)
                                    {
                                        Trace.WriteLine("删除文件" + tempfile.FullName);
                                        File.Delete(tempfile.FullName);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MyLog.Error(ex.Message);
                            }

                            string timestr = string.Format("{0}-{1:D2}-{2:D2} {3:D2}：{4:D2}：{5:D2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                            string filename = Path + timestr + ".dat";
                            FileStream file = new FileStream(filename, FileMode.Create);
                            BinaryWriter bw1 = new BinaryWriter(file);
                            myDictionary.Add(Vcid, bw1);

                            //new Thread(() => { RealTime_ComPare(ref VCID_01); }).Start();

                            switch (Vcid)
                            {
                                case 0x1:
                                    new Thread(() => { RealTime_ComPare(ref VCID_01); }).Start();
                                    break;
                                case 0x2:
                                    new Thread(() => { RealTime_ComPare(ref VCID_02); }).Start();
                                    break;
                                case 0x3:
                                    new Thread(() => { RealTime_ComPare(ref VCID_03); }).Start();
                                    break;
                                case 0x4:
                                    new Thread(() => { RealTime_ComPare(ref VCID_04); }).Start();
                                    break;
                                case 0x5:
                                    new Thread(() => { RealTime_ComPare(ref VCID_05); }).Start();
                                    break;
                                case 0x6:
                                    new Thread(() => { RealTime_ComPare(ref VCID_06); }).Start();
                                    break;
                                case 0x7:
                                    new Thread(() => { RealTime_ComPare(ref VCID_07); }).Start();
                                    break;
                                case 0x8:
                                    new Thread(() => { RealTime_ComPare(ref VCID_08); }).Start();
                                    break;
                                case 0x9:
                                    new Thread(() => { RealTime_ComPare(ref VCID_09); }).Start();
                                    break;
                                case 0xa:
                                    new Thread(() => { RealTime_ComPare(ref VCID_10); }).Start();
                                    break;
                                case 0xb:
                                    new Thread(() => { RealTime_ComPare(ref VCID_11); }).Start();
                                    break;
                                case 0xc:
                                    new Thread(() => { RealTime_ComPare(ref VCID_12); }).Start();
                                    break;
                                case 0xd:
                                    new Thread(() => { RealTime_ComPare(ref VCID_13); }).Start();
                                    break;
                                case 0xe:
                                    new Thread(() => { RealTime_ComPare(ref VCID_14); }).Start();
                                    break;
                                case 0xf:
                                    new Thread(() => { RealTime_ComPare(ref VCID_15); }).Start();
                                    break;
                                case 0x10:
                                    new Thread(() => { RealTime_ComPare(ref VCID_16); }).Start();
                                    break;
                            }
                        }

                        if (myDictionary.ContainsKey(Vcid))
                        {
                            myDictionary[Vcid].Write(CADU);
                            myDictionary[Vcid].Flush();
                        }
                        else
                        {
                            Trace.WriteLine("未找到APID对应的File文件!!Error!!!");
                        }

                        if (Tag == 0x00 && Vcid < 0x10)
                        {
                            lock (myVcidDic[Vcid].RealBuf)
                                myVcidDic[Vcid].RealBuf.AddRange(data);

                            switch (Vcid)
                            {
                                case 0x1:
                                    VCID_01.Real_RecvCounts += 1;
                                    break;
                                case 0x2:
                                    VCID_02.Real_RecvCounts += 1;
                                    break;
                                case 0x3:
                                    VCID_03.Real_RecvCounts += 1;
                                    break;
                                case 0x4:
                                    VCID_04.Real_RecvCounts += 1;
                                    break;
                                case 0x5:
                                    VCID_05.Real_RecvCounts += 1;
                                    break;
                                case 0x6:
                                    VCID_06.Real_RecvCounts += 1;
                                    break;
                                case 0x7:
                                    VCID_07.Real_RecvCounts += 1;
                                    break;
                                case 0x8:
                                    VCID_08.Real_RecvCounts += 1;
                                    break;
                                case 0x9:
                                    VCID_09.Real_RecvCounts += 1;
                                    break;
                                case 0xa:
                                    VCID_10.Real_RecvCounts += 1;
                                    break;
                                case 0xb:
                                    VCID_11.Real_RecvCounts += 1;
                                    break;
                                case 0xc:
                                    VCID_12.Real_RecvCounts += 1;
                                    break;
                                case 0xd:
                                    VCID_13.Real_RecvCounts += 1;
                                    break;
                                case 0xe:
                                    VCID_14.Real_RecvCounts += 1;
                                    break;
                                case 0xf:
                                    VCID_15.Real_RecvCounts += 1;
                                    break;
                                case 0x10:
                                    VCID_16.Real_RecvCounts += 1;
                                    break;
                            }
                        }
                        else if (Tag == 0x80 && Vcid < 0x10)//回放数据--延时遥测
                        {
                            lock (myVcidDic[Vcid].DelayBuf)
                                myVcidDic[Vcid].DelayBuf.AddRange(data);

                            switch (Vcid)
                            {
                                case 0x1:
                                    VCID_01.Delay_RecvCounts += 1;
                                    break;
                                case 0x2:
                                    VCID_02.Delay_RecvCounts += 1;
                                    break;
                                case 0x3:
                                    VCID_03.Delay_RecvCounts += 1;
                                    break;
                                case 0x4:
                                    VCID_04.Delay_RecvCounts += 1;
                                    break;
                                case 0x5:
                                    VCID_05.Delay_RecvCounts += 1;
                                    break;
                                case 0x6:
                                    VCID_06.Delay_RecvCounts += 1;
                                    break;
                                case 0x7:
                                    VCID_07.Delay_RecvCounts += 1;
                                    break;
                                case 0x8:
                                    VCID_08.Delay_RecvCounts += 1;
                                    break;
                                case 0x9:
                                    VCID_09.Delay_RecvCounts += 1;
                                    break;
                                case 0xa:
                                    VCID_10.Delay_RecvCounts += 1;
                                    break;
                                case 0xb:
                                    VCID_11.Delay_RecvCounts += 1;
                                    break;
                                case 0xc:
                                    VCID_12.Delay_RecvCounts += 1;
                                    break;
                                case 0xd:
                                    VCID_13.Delay_RecvCounts += 1;
                                    break;
                                case 0xe:
                                    VCID_14.Delay_RecvCounts += 1;
                                    break;
                                case 0xf:
                                    VCID_15.Delay_RecvCounts += 1;
                                    break;
                                case 0x10:
                                    VCID_16.Delay_RecvCounts += 1;
                                    break;
                            }
                        }
                        else
                        {
                            MyLog.Error("实时延时标志位或VCID出错");
                        }
                    }
                    else
                    {
                        MyLog.Error("比对数据偏头，请重新测试");
                        break;
                    }
                }
                else
                {
                    Thread.Sleep(10);
                }
            }

            foreach (var item in myDictionary)
            {
                item.Value.Close();
            }
            myDictionary.Clear();




        }

        public static void RealTime_ComPare(ref RecvChan_VCID_Struct TempVCID)
        {
            Trace.WriteLine("RealTime_ComPare2 in");

            int Real_CPlen = TempVCID.Real_CPLen;
            byte[] Real_CPdata = new byte[Real_CPlen];//比对数组长度

            int Delay_CPlen = TempVCID.Delay_CPLen;
            byte[] Delay_CPdata = new byte[Delay_CPlen];

            int Recv_Real_Bytes = 0;//收到字节数，用来换算多少行
            int Recv_Delay_Bytes = 0;

            while (LVDS_ComPareTag)
            {
                if (TempVCID.RealBuf.Count() >= 1024 || TempVCID.DelayBuf.Count() >= 1024)
                {
                    if (TempVCID.RealBuf.Count() >= 1024)//Tag==0x00,实时遥测
                    {
                        if (TempVCID.GetComPareDataTag)
                        {
                            for (int j = 0; j < Real_CPlen; j++) Real_CPdata[j] = TempVCID.RealBuf[j];
                            TempVCID.GetComPareDataTag = false;
                            Recv_Real_Bytes += Real_CPlen;
                        }
                        //循环处理数据域data
                        while (TempVCID.RealBuf.Count() >= Real_CPlen)
                        {
                            for (int t = 0; t < Real_CPlen; t++)
                            {
                                if (TempVCID.RealBuf[t] != Real_CPdata[t])
                                {
                                    TempVCID.Real_ErrorCol = Recv_Real_Bytes % 862 + t;
                                    TempVCID.Real_ErrorRow = Recv_Real_Bytes / 862;
                                    String loginfo = "--Error 实时 VCID:" + Convert.ToString(TempVCID.VCID, 2).PadLeft(6, '0') + "--Row:" + TempVCID.Real_ErrorRow.ToString() + "--Column:" + TempVCID.Real_ErrorCol.ToString();
                                    SaveFile.Lock_asyn_1.EnterWriteLock();
                                    SaveFile.DataQueue_asyn_1.Enqueue(loginfo);
                                    SaveFile.Lock_asyn_1.ExitWriteLock();
                                }
                                Real_CPdata[t] = TempVCID.RealBuf[t];
                            }
                            Recv_Real_Bytes += Real_CPlen;

                            lock (TempVCID.RealBuf)
                                TempVCID.RealBuf.RemoveRange(0, Real_CPlen);
                        }
                    }

                    if (TempVCID.DelayBuf.Count() >= 1024)
                    {
                        if (TempVCID.GetComPareDataTag)
                        {
                            for (int j = 0; j < Delay_CPlen; j++) Delay_CPdata[j] = TempVCID.DelayBuf[j];
                            TempVCID.GetComPareDataTag = false;
                            Recv_Delay_Bytes += Delay_CPlen;
                        }
                        //循环处理数据域data
                        while (TempVCID.DelayBuf.Count() >= Delay_CPlen)
                        {
                            for (int t = 0; t < Delay_CPlen; t++)
                            {
                                if (TempVCID.DelayBuf[t] != Delay_CPdata[t])
                                {
                                    TempVCID.Delay_ErrorCol = Recv_Delay_Bytes % 862 + t;
                                    TempVCID.Delay_ErrorRow = Recv_Delay_Bytes / 862;

                                    String loginfo = "Error 延时 VCID:" + Convert.ToString(TempVCID.VCID, 2).PadLeft(6, '0') + "--Row:" + TempVCID.Delay_ErrorRow.ToString() + "--Column:" + TempVCID.Delay_ErrorCol.ToString();
                                    SaveFile.Lock_asyn_1.EnterWriteLock();
                                    SaveFile.DataQueue_asyn_1.Enqueue(loginfo);
                                    SaveFile.Lock_asyn_1.ExitWriteLock();
                                }
                            }
                            Recv_Delay_Bytes += Delay_CPlen;
                            lock (TempVCID.DelayBuf)
                                TempVCID.DelayBuf.RemoveRange(0, Delay_CPlen);
                        }
                    }
                }
                else
                {
                    Thread.Sleep(10);
                }
            }

            Trace.WriteLine("RealTime_ComPare2 out");
        }

    }
}
