using CyUSB;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CKServer
{
    public partial class MainForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        LoadFPGAForm loadfpgaform = new LoadFPGAForm();
        DAmodify myDAmodifyForm = new DAmodify();
        SaveFile FileThread;

        RS422FrameProduceForm myRs422FrameProduceForm;
        SetComPareFrame myComPareFrame;

        public DateTime startDT;
        // private DataTable dt_AD = new DataTable();
        //  private DataTable dt_OC = new DataTable();

        private DataTable dtModifyDA1 = new DataTable();
        private DataTable dtModifyDA2 = new DataTable();

        //应用数据的字节1和字节2--A机
        String A_Byte1_b76 = "10";
        String A_Byte1_b543 = "100";
        String A_Byte1_b210 = "100";
        String A_Byte2_b76 = "10";

        //应用数据的字节1和字节2--B机
        String B_Byte1_b76 = "10";
        String B_Byte1_b543 = "100";
        String B_Byte1_b210 = "100";
        String B_Byte2_b76 = "10";

        void UsbDevices_DeviceAttached(object sender, EventArgs e)
        {
            SetDevice(false);
        }

        /*Summary
        This is the event handler for device removal. This method resets the device count and searches for the device with VID-PID 04b4-1003
        */
        void UsbDevices_DeviceRemoved(object sender, EventArgs e)
        {
            USBEventArgs evt = (USBEventArgs)e;
            USBDevice RemovedDevice = evt.Device;

            string RemovedDeviceName = evt.FriendlyName;
            MyLog.Error(RemovedDeviceName + "板卡断开");

            int key = int.Parse(evt.ProductID.ToString("x4").Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            USB.MyDeviceList[key] = null;

        }

        /*Summary
        Search the device with VID-PID 04b4-00F1 and if found, select the end point
        */
        private void SetDevice(bool bPreserveSelectedDevice)
        {
            int nDeviceList = USB.usbDevices.Count;
            for (int nCount = 0; nCount < nDeviceList; nCount++)
            {
                USBDevice fxDevice = USB.usbDevices[nCount];
                String strmsg;
                strmsg = "(0x" + fxDevice.VendorID.ToString("X4") + " - 0x" + fxDevice.ProductID.ToString("X4") + ") " + fxDevice.FriendlyName;

                int key = int.Parse(fxDevice.ProductID.ToString("x4").Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                if (USB.MyDeviceList[key] == null)
                {
                    USB.MyDeviceList[key] = (CyUSBDevice)fxDevice;

                    MyLog.Info(USB.MyDeviceList[key].FriendlyName + ConfigurationManager.AppSettings[USB.MyDeviceList[key].FriendlyName] + "板卡连接");

                }
            }

        }
        public MainForm()
        {
            InitializeComponent();

            startDT = System.DateTime.Now;
            Data.Path = Program.GetStartupPath();
            //启动日志
            MyLog.richTextBox1 = richTextBox1;
            MyLog.path = Program.GetStartupPath() + @"LogData\";
            MyLog.lines = 50;
            MyLog.start();

            // Create the list of USB devices attached to the CyUSB3.sys driver.
            USB.usbDevices = new USBDeviceList(CyConst.DEVICES_CYUSB);

            //Assign event handlers for device attachment and device removal.
            USB.usbDevices.DeviceAttached += new EventHandler(UsbDevices_DeviceAttached);
            USB.usbDevices.DeviceRemoved += new EventHandler(UsbDevices_DeviceRemoved);

            USB.Init();

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SetDevice(false);
            barStaticItem1.Caption = "存储路径" + Data.Path;
            InitDataTable();//初始化datadable
            Function.Init();//初始化DA参数

            myRs422FrameProduceForm = new RS422FrameProduceForm(this);
            myComPareFrame = new SetComPareFrame(this);

            dockPanel_RegCtl.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Hidden;
        }

        private void InitDataTable()
        {
            try
            {
                Func_LVDS.Init_Table();
                dataGridView_LvdsCP.DataSource = Func_LVDS.dt_LVDS_CP;
                dataGridView_LvdsCP.AllowUserToAddRows = false;

                dataGridView_LvdsResult.DataSource = Func_LVDS.dt_LVDS_Result;
                dataGridView_LvdsResult.AllowUserToAddRows = false;


                Register.Init();
                dataGridView_Reg.DataSource = Register.dt_Reg;
                dataGridView_Reg.AllowUserToAddRows = false;

                Func_AD.Init_Table();
                dataGridView_AD.DataSource = Func_AD.dt_ADShow;
                dataGridView_AD.AllowUserToAddRows = false;

                Func_OC.Init_Table();
                dataGridView_OC.DataSource = Func_OC.dt_OC;
                dataGridView_OC.AllowUserToAddRows = false;

                Func_YC.Init_Table();
                dataGridView_YC1.DataSource = Func_YC.dt_YC1;
                dataGridView_YC1.AllowUserToAddRows = false;
                dataGridView_YC2.DataSource = Func_YC.dt_YC2;
                dataGridView_YC2.AllowUserToAddRows = false;

                dtModifyDA1.Columns.Add("ID", typeof(Int32));
                dtModifyDA1.Columns.Add("a", typeof(Int32));
                dtModifyDA1.Columns.Add("b", typeof(Int32));
                for (int i = 0; i < 128; i++)
                {
                    DataRow dr = dtModifyDA1.NewRow();
                    dr["ID"] = i + 1;
                    dr["a"] = 130;
                    dr["b"] = 7800;
                    dtModifyDA1.Rows.Add(dr);
                }
                dataGridView1.DataSource = dtModifyDA1;
                dataGridView1.AllowUserToAddRows = false;

                dtModifyDA2.Columns.Add("ID", typeof(Int32));
                dtModifyDA2.Columns.Add("a", typeof(Int32));
                dtModifyDA2.Columns.Add("b", typeof(Int32));
                for (int i = 0; i < 128; i++)
                {
                    DataRow dr = dtModifyDA2.NewRow();
                    dr["ID"] = i + 1;
                    dr["a"] = 130;
                    dr["b"] = 7800;
                    dtModifyDA2.Rows.Add(dr);
                }
                dataGridView2.DataSource = dtModifyDA2;
                dataGridView2.AllowUserToAddRows = false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("初始化DA：" + ex.Message);
            }

        }

        private void UpdateProgress(ProgressBarControl pbc, int value)
        {
            pbc.Increment(value);
        }

        private void barButtonItem4_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (loadfpgaform != null)
            {
                loadfpgaform.Activate();
            }
            else
            {
                loadfpgaform = new LoadFPGAForm();
            }
            loadfpgaform.ShowDialog();
        }

        private void barButtonItem6_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void barButtonItem37_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void btn_VOut_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            for (int i = 0; i < 8; i++)
            {
                if (i < 7)
                {
                    byte value = (byte)(0x01 << i);
                    //边沿出发，发送一次脉冲
                    USB.SendCMD(Data.OnlyID, 0x8D, value);
                    USB.SendCMD(Data.OnlyID, 0x8D, 0x0);
                    Thread.Sleep(10);
                }
                else if (i == 7)
                {
                    USB.SendCMD(Data.OnlyID, 0x8E, 0x01);
                    USB.SendCMD(Data.OnlyID, 0x8E, 0x00);
                    Thread.Sleep(10);
                }
                else
                {
                    //扩展
                }
            }
        }



        private void btn_modify_reset_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 128; i++)
            {
                dtModifyDA1.Rows[i][1] = Data.GetConfig(Data.DAconfigPath, "DAModify_Board1_A" + (i).ToString());
                dtModifyDA1.Rows[i][2] = Data.GetConfig(Data.DAconfigPath, "DAModify_Board1_B" + (i).ToString());

                dtModifyDA2.Rows[i][1] = Data.GetConfig(Data.DAconfigPath, "DAModify_Board2_A" + (i).ToString());
                dtModifyDA2.Rows[i][2] = Data.GetConfig(Data.DAconfigPath, "DAModify_Board2_B" + (i).ToString());
            }
        }

        private void btn_modify_ok_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 128; i++)
            {
                Data.DA1_value_a[i] = (int)dtModifyDA1.Rows[i]["a"];
                Data.DA1_value_b[i] = (int)dtModifyDA1.Rows[i]["b"];

                Data.SaveConfig(Data.DAconfigPath, "DAModify_Board1_A" + (i).ToString(), dtModifyDA1.Rows[i]["a"].ToString());
                Data.SaveConfig(Data.DAconfigPath, "DAModify_Board1_B" + (i).ToString(), dtModifyDA1.Rows[i]["b"].ToString());

                Data.DA2_value_a[i] = (int)dtModifyDA2.Rows[i]["a"];
                Data.DA2_value_b[i] = (int)dtModifyDA2.Rows[i]["b"];

                Data.SaveConfig(Data.DAconfigPath, "DAModify_Board2_A" + (i).ToString(), dtModifyDA2.Rows[i]["a"].ToString());
                Data.SaveConfig(Data.DAconfigPath, "DAModify_Board2_B" + (i).ToString(), dtModifyDA2.Rows[i]["b"].ToString());
            }
            MessageBox.Show("参数修正已成功，可以关闭此窗口！");
        }

        private void btn_modify_load_Click(object sender, EventArgs e)
        {
            DataTable dt;
            SimpleButton btn = (SimpleButton)sender;
            switch (btn.Name)
            {
                case "btn_modify_load1":
                    dt = dtModifyDA1;
                    break;
                case "btn_modify_load2":
                    dt = dtModifyDA2;
                    break;
                default:
                    dt = dtModifyDA1;
                    break;
            }

            String Path = Program.GetStartupPath() + @"参数修正码本\";
            openFileDialog1.InitialDirectory = Path;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                MyLog.Info("载入DA修正码本成功！");

                string[] content = File.ReadAllLines(openFileDialog1.FileName);
                string[] temp = new string[3];

                if (content.Length >= 128)
                {
                    for (int i = 0; i < 128; i++)
                    {
                        temp = content[i].Split(',');

                        dt.Rows[i]["a"] = double.Parse(temp[1].Trim());
                        dt.Rows[i]["b"] = double.Parse(temp[2].Trim());
                        //dataGridView1.Rows[i].Cells[1].Value = double.Parse(temp[1].Trim());
                        //dataGridView1.Rows[i].Cells[2].Value = double.Parse(temp[2].Trim());           
                    }

                }
            }
        }

        private void btn_modify_save_Click(object sender, EventArgs e)
        {
            DataTable dt;
            SimpleButton btn = (SimpleButton)sender;
            switch (btn.Name)
            {
                case "btn_modify_save1":
                    dt = dtModifyDA1;
                    break;
                case "btn_modify_save2":
                    dt = dtModifyDA2;
                    break;
                default:
                    dt = dtModifyDA1;
                    break;
            }

            String Path = Program.GetStartupPath() + @"参数修正码本\";
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            saveFileDialog1.InitialDirectory = Path;

            saveFileDialog1.Filter = "文本文件(*.txt)|*.txt|All files(*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                String ModifyStr = "";

                for (int i = 0; i < 128; i++)
                {
                    ModifyStr += dt.Rows[i][0] + "," + dt.Rows[i][1] + "," + dt.Rows[i][2] + "\r\n";
                }
                string localFilePath = saveFileDialog1.FileName.ToString(); //获得文件路径 

                FileStream file0 = new FileStream(localFilePath, FileMode.Create);
                StreamWriter sw = new StreamWriter(file0);
                sw.WriteLine(ModifyStr);
                sw.Flush();
                sw.Close();
                file0.Close();
                MessageBox.Show("存储文件成功！", "保存文件");
            }
        }

        #region Status_CheckedChanged用户配置选项
        private void Status_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (CheckEnable_LocalTime.Checked == true)
            {
                barStaticItem3.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
            }
            else
            {
                barStaticItem3.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            }

            if (CheckEnable_RunTime.Checked == true)
            {
                barStaticItem4.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
            }
            else
            {
                barStaticItem4.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            }

            if (CheckEnable_DiskSpace.Checked == true)
            {
                barStaticItem2.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
            }
            else
            {
                barStaticItem2.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            }

            if (CheckEnable_Path.Checked == true)
            {
                barStaticItem1.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
            }
            else
            {
                barStaticItem1.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            }
        }
        #endregion


        private void timer1_Tick(object sender, EventArgs e)
        {
            barStaticItem2.Caption = "剩余空间" + DiskInfo.GetFreeSpace(Data.Path[0].ToString()) + "MB";
            barStaticItem3.Caption = "当前时间：" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ";

            TimeSpan ts = DateTime.Now.Subtract(startDT);

            barStaticItem4.Caption = "已运行：" + ts.Days.ToString() + "天" +
                ts.Hours.ToString() + "时" +
                ts.Minutes.ToString() + "分" +
                ts.Seconds.ToString() + "秒";

            for (int i = 0; i < 4; i++)
            {
                Func_AD.dt_ADShow.Rows[i]["测量值"] = dataRe_AD[i + 38];
            }

            Func_LVDS.dt_LVDS_Result.Rows[0]["总字节数"] = Func_LVDS.TotalNums1;
            Func_LVDS.dt_LVDS_Result.Rows[0]["错误字节"] = Func_LVDS.ErrorNums1;
            Func_LVDS.dt_LVDS_Result.Rows[0]["误码率"] = Func_LVDS.ErPert1;

            Func_LVDS.dt_LVDS_Result.Rows[1]["总字节数"] = Func_LVDS.TotalNums2;
            Func_LVDS.dt_LVDS_Result.Rows[1]["错误字节"] = Func_LVDS.ErrorNums2;
            Func_LVDS.dt_LVDS_Result.Rows[1]["误码率"] = Func_LVDS.ErPert2;
        }

        private void btn_Modify_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            if (dockPanel_422_A.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Visible)
            {
                dockPanel_422_A.Show();
                MyLog.Info("显示422界面！");
            }
            else
            {
                dockPanel_422_A.Hide();
                MyLog.Info("隐藏422界面！");
            }

        }

        bool _BoxIsStarted;
        private void btn_Start_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (btn_Start.Caption == "一键开始")
            {
                btn_Start.Caption = "一键停止";
                btn_Start.ImageOptions.LargeImage = CKServer.Properties.Resources.Stop_btn;
                _BoxIsStarted = false;
                if (USB.MyDeviceList[Data.OnlyID] != null)
                {
                    CyControlEndPoint CtrlEndPt = null;
                    CtrlEndPt = USB.MyDeviceList[Data.OnlyID].ControlEndPt;
                    if (CtrlEndPt != null)
                    {
                        USB.SendCMD(Data.OnlyID, 0x80, 0x00);
                        USB.SendCMD(Data.OnlyID, 0x80, 0x01);
                        USB.MyDeviceList[Data.OnlyID].Reset();

                        Register.Byte80H = (byte)(Register.Byte80H | 0x04);
                        USB.SendCMD(Data.OnlyID, 0x80, Register.Byte80H);
                        _BoxIsStarted = true;
                    }
                    else
                    {
                        MyLog.Error("设备硬件初始化失败!!");
                        _BoxIsStarted = false;
                    }
                }

                FileThread = new SaveFile();
                if (_BoxIsStarted)
                {
                    FileThread.FileInit();
                    FileThread.FileSaveStart();
                }

                new Thread(() => { RecvFun(Data.OnlyID); }).Start();
                ADList.Clear();
                new Thread(() => { DealWithADFun(); }).Start();

                YCList_A.Clear();
                YCList_B.Clear();
                new Thread(() => { DealWithYCFun(); }).Start();

                if (radioButton1.Checked)
                    Func_LVDS.StartComPare();//开启比对
            }
            else
            {
                btn_Start.Caption = "一键开始";
                btn_Start.ImageOptions.LargeImage = CKServer.Properties.Resources.Start_btn;
                _BoxIsStarted = false;
                Thread.Sleep(200);

                FileThread.FileClose();

                Func_LVDS._StartCompare = false;


            }
        }


        private void RecvFun(int key)
        {
            Trace.WriteLine("开启线程接收USB数据");
            CyUSBDevice MyDevice = USB.MyDeviceList[key];

            byte[] Recv_MidBuf_8K = new byte[8192];//8K中间缓存
            int Pos_Recv_MidBuf_8K = 0;//中间缓存数据存储到哪个位置

            Func_LVDS.DataQueue1.Clear();
            Func_LVDS.DataQueue2.Clear();

            while (_BoxIsStarted)
            {
                if (MyDevice.BulkInEndPt != null)
                {
                    byte[] RecvBoxBuf = new byte[4096];
                    int RecvBoxLen = 4096;
                    MyDevice.BulkInEndPt.XferData(ref RecvBoxBuf, ref RecvBoxLen);

                    if (RecvBoxLen > 0)
                    {
                        byte[] tempbuf = new byte[RecvBoxLen];
                        Array.Copy(RecvBoxBuf, tempbuf, RecvBoxLen);
                        //存储源码
                        SaveFile.Lock_1.EnterWriteLock();
                        SaveFile.DataQueue_SC1.Enqueue(tempbuf);
                        SaveFile.Lock_1.ExitWriteLock();

                        Array.Copy(tempbuf, 0, Recv_MidBuf_8K, Pos_Recv_MidBuf_8K, tempbuf.Length);
                        Pos_Recv_MidBuf_8K += tempbuf.Length;

                        while (Pos_Recv_MidBuf_8K >= 4096)
                        {
                            if (Recv_MidBuf_8K[0] == 0xff && (0x0 <= Recv_MidBuf_8K[1]) && (Recv_MidBuf_8K[1] < 0x11))
                            {
                                DealWithLongFrame(ref Recv_MidBuf_8K, ref Pos_Recv_MidBuf_8K);
                            }
                            else
                            {
                                MyLog.Error("收到异常帧！");
                                Array.Clear(Recv_MidBuf_8K, 0, Pos_Recv_MidBuf_8K);
                                Pos_Recv_MidBuf_8K = 0;
                            }
                        }
                    }
                    else if (RecvBoxLen == 0)
                    {
                        //  Trace.WriteLine("收到0包-----0000000000");
                    }
                    else
                    {
                        Trace.WriteLine("USB接收数据异常，居然收到了小于0的数！！");
                        //MyLog.Error("USB接收数据异常，居然收到了小于0的数！！");
                    }
                }
            }
        }

        int ThisCount = 0;
        int LastCount = 0;
        void DealWithLongFrame(ref byte[] TempBuf, ref int TempTag)
        {
            ThisCount = TempBuf[2] * 256 + TempBuf[3];
            if (LastCount != 0 && ThisCount != 0 && (ThisCount - LastCount != 1))
            {
                MyLog.Error("出现漏帧情况！！");
                Trace.WriteLine("出现漏帧情况:" + LastCount.ToString("x4") + "--" + ThisCount.ToString("x4"));
            }
            LastCount = ThisCount;

            byte[] buf_LongFrame = new byte[4096];
            Array.Copy(TempBuf, 0, buf_LongFrame, 0, 4096);

            Array.Copy(TempBuf, 4096, TempBuf, 0, TempTag - 4096);
            TempTag -= 4096;

            //if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x00)
            //{
            //    byte[] bufsav = new byte[4092];
            //    Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
            //    SaveFile.Lock_2.EnterWriteLock();
            //    SaveFile.DataQueue_SC2.Enqueue(bufsav);
            //    SaveFile.Lock_2.ExitWriteLock();
            //}
            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x01)
            {
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                SaveFile.Lock_2.EnterWriteLock();
                SaveFile.DataQueue_SC2.Enqueue(bufsav);
                SaveFile.Lock_2.ExitWriteLock();

                Func_LVDS.Lock1.EnterWriteLock();
                for (int i = 0; i < 4092; i++) Func_LVDS.DataQueue1.Enqueue(bufsav[i]);
                Func_LVDS.Lock1.ExitWriteLock();

            }
            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x02)
            {
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                SaveFile.Lock_3.EnterWriteLock();
                SaveFile.DataQueue_SC3.Enqueue(bufsav);
                SaveFile.Lock_3.ExitWriteLock();

                Func_LVDS.Lock2.EnterWriteLock();
                for (int i = 0; i < 4092; i++) Func_LVDS.DataQueue2.Enqueue(bufsav[i]);
                Func_LVDS.Lock2.ExitWriteLock();

            }
            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x03)
            {
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                SaveFile.Lock_5.EnterWriteLock();
                SaveFile.DataQueue_SC5.Enqueue(bufsav);
                SaveFile.Lock_5.ExitWriteLock();
            }
            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x08)
            {
                //FF08为短帧通道
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                SaveFile.Lock_6.EnterWriteLock();
                SaveFile.DataQueue_SC6.Enqueue(bufsav);
                SaveFile.Lock_6.ExitWriteLock();

                for (int i = 0; i < 6; i++)
                {
                    if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x00)//1D00：第1路422
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        SaveFile.Lock_7.EnterWriteLock();
                        SaveFile.DataQueue_SC7.Enqueue(buf1D0x);
                        SaveFile.Lock_7.ExitWriteLock();
                        lock (YCList_A)
                        {//将422的A遥测数据放入YCList_A,在另一个线程中解析
                            for (int j = 0; j < buf1D0x.Length; j++) YCList_A.Add(buf1D0x[j]);
                        }
                    }
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x01)//1D01:第2路422
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        SaveFile.Lock_8.EnterWriteLock();
                        SaveFile.DataQueue_SC8.Enqueue(buf1D0x);
                        SaveFile.Lock_8.ExitWriteLock();
                        lock (YCList_B)
                        {//将422的A遥测数据放入YCList_A,在另一个线程中解析
                            for (int j = 0; j < buf1D0x.Length; j++) YCList_B.Add(buf1D0x[j]);
                        }
                    }
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x08)//1D08:AD数据
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        SaveFile.Lock_9.EnterWriteLock();
                        SaveFile.DataQueue_SC9.Enqueue(buf1D0x);
                        SaveFile.Lock_9.ExitWriteLock();
                        lock (ADList)
                        {//将AD数据放入ADList,在另一个线程中解析
                            for (int j = 0; j < buf1D0x.Length; j++) ADList.Add(buf1D0x[j]);
                        }
                    }
                    //else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x03)
                    //{
                    //    int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                    //    byte[] buf1D0x = new byte[num];
                    //    Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                    //    SaveFile.Lock_10.EnterWriteLock();
                    //    SaveFile.DataQueue_SC10.Enqueue(buf1D0x);
                    //    SaveFile.Lock_10.ExitWriteLock();
                    //}
                    //else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x04)
                    //{
                    //    int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                    //    byte[] buf1D0x = new byte[num];
                    //    Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                    //    SaveFile.Lock_11.EnterWriteLock();
                    //    SaveFile.DataQueue_SC11.Enqueue(buf1D0x);
                    //    SaveFile.Lock_11.ExitWriteLock();
                    //}
                    //else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x05)
                    //{
                    //    int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                    //    byte[] buf1D0x = new byte[num];
                    //    Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                    //    SaveFile.Lock_12.EnterWriteLock();
                    //    SaveFile.DataQueue_SC12.Enqueue(buf1D0x);
                    //    SaveFile.Lock_12.ExitWriteLock();
                    //}
                    //else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x06)
                    //{
                    //    int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                    //    byte[] buf1D0x = new byte[num];
                    //    Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                    //    SaveFile.Lock_13.EnterWriteLock();
                    //    SaveFile.DataQueue_SC13.Enqueue(buf1D0x);
                    //    SaveFile.Lock_13.ExitWriteLock();
                    //}
                    //else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x07)
                    //{
                    //    int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                    //    byte[] buf1D0x = new byte[num];
                    //    Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                    //    SaveFile.Lock_14.EnterWriteLock();
                    //    SaveFile.DataQueue_SC14.Enqueue(buf1D0x);
                    //    SaveFile.Lock_14.ExitWriteLock();
                    //}
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x0f)
                    {
                        //空闲帧
                    }
                    else
                    {
                        Trace.WriteLine("FF08通道出错!");
                    }
                }
            }
        }

        List<byte> YCList_A = new List<byte>();
        string[] dataRe_YCA1 = new string[64];
        string[] dataRe_YCA2 = new string[64];//64大于实际路数

        List<byte> YCList_B = new List<byte>();
        string[] dataRe_YCB1 = new string[64];
        string[] dataRe_YCB2 = new string[64];//64大于实际路数

        private void DealWithYCFun()
        {
            while (_BoxIsStarted)
            {
                bool ret1 = Func_YC.Return_YCValue(ref YCList_A, 1, ref dataRe_YCA1, ref dataRe_YCA2);
                bool ret2 = Func_YC.Return_YCValue(ref YCList_B, 2, ref dataRe_YCB1, ref dataRe_YCB2);
                if (ret1 == false && ret2 == false) Thread.Sleep(500);
            }
        }



        List<byte> ADList = new List<byte>();
        double[] dataRe_AD = new double[64];
        private void DealWithADFun()
        {
            //获得AD校准参数
            double[] md = new double[64];
            for (int j = 0; j < 64; j++) md[j] = double.Parse(Function.GetConfigStr(Data.ADconfigPath, "add", "AD_Channel_" + j.ToString(), "value"));

            while (_BoxIsStarted)
            {
                bool ret = Func_AD.Return_ADValue(ref ADList, ref dataRe_AD, md);
                if (!ret) Thread.Sleep(500);
            }
        }

        //private void DealWithADFun()
        //{
        //    //获得AD校准参数
        //    double[] md = new double[64];
        //    for (int j = 0; j < 64; j++) md[j] = double.Parse(Function.GetConfigStr(Data.ADconfigPath, "add", "AD_Channel_" + j.ToString(), "value"));

        //    while (_BoxIsStarted)
        //    {
        //        int ADFrame = 132;//64channel=128bytes,+2head,+2end
        //        int _StartPos = ADList.IndexOf(0xB0);
        //        if (_StartPos >= 0 && ADList.Count >= (_StartPos + ADFrame))
        //        {
        //            if (ADList[_StartPos + 1] == 0xFA && ADList[_StartPos + ADFrame - 2] == 0xE0 && ADList[_StartPos + ADFrame - 1] == 0xF5)
        //            {
        //                byte[] buf = new byte[128];
        //                for (int t = 0; t < 128; t++)
        //                {
        //                    buf[t] = ADList[_StartPos + 2 + t];
        //                }
        //                for (int k = 0; k < 63; k++)
        //                {
        //                    int temp = (buf[2 * k] & 0x7f) * 256 + buf[2 * k + 1];
        //                    double t = (double)(5 * temp) / (double)32767;
        //                    t = t * 3 / md[k];
        //                    if ((buf[2 * k] & 0x80) == 0x80)
        //                        dataRe_AD[k] = -t;
        //                    else
        //                        dataRe_AD[k] = t;
        //                }
        //                lock (ADList)
        //                {
        //                    ADList.RemoveRange(0, _StartPos + ADFrame);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            Thread.Sleep(500);
        //            if (ADList.Count >= 8192)
        //            {
        //                lock (ADList)
        //                {
        //                    ADList.Clear();
        //                }
        //            }
        //        }
        //    }
        //    Trace.WriteLine("退出 DealWithADFun：");
        //}

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _BoxIsStarted = false;
                Thread.Sleep(200);
                FileThread.FileClose();

            }
            catch (Exception ex)
            {

            }
        }

        private void dataGridView5_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (e.ColumnIndex == 0)//button列
                {
                    if (e.RowIndex <= 6)
                    {
                        USB.SendCMD(Data.OnlyID, 0x8D, (byte)(0x1 << e.RowIndex));
                        USB.SendCMD(Data.OnlyID, 0x8D, 0x0);
                    }
                    else if (e.RowIndex == 7)
                    {
                        USB.SendCMD(Data.OnlyID, 0x8E, 0x1);
                        USB.SendCMD(Data.OnlyID, 0x8E, 0x0);
                    }
                    else
                    {
                        //扩展
                    }
                    MyLog.Info("指令输出:" + Func_OC.dt_OC.Rows[e.RowIndex]["名称"]);



                }
            }
        }

        private void buttonEdit1_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            ButtonEdit editor = (ButtonEdit)sender;
            EditorButton Button = e.Button;
            TextBox mytxtbox;
            GroupBox mygroupBox;
            System.Windows.Forms.ComboBox myFreqEdit;
            String msgstr = "未选择文件";
            String ConfigPath = "PATH_DAT_01";
            byte FrameHeadLastByte = 0x08;
            switch (editor.Name)
            {
                case "buttonEdit1":
                    msgstr = "通道1";
                    ConfigPath = "PATH_DAT_01";
                    FrameHeadLastByte = 0x08;
                    mytxtbox = textBox_Send_1;
                    myFreqEdit = comboBox1;
                    mygroupBox = groupBox1;
                    break;
                case "buttonEdit2":
                    msgstr = "通道2";
                    ConfigPath = "PATH_DAT_02";
                    FrameHeadLastByte = 0x09;
                    mytxtbox = textBox_Send_2;
                    myFreqEdit = comboBox2;
                    mygroupBox = groupBox2;
                    break;
                default:
                    msgstr = "通道1";
                    ConfigPath = "PATH_DAT_01";
                    FrameHeadLastByte = 0x08;
                    mytxtbox = textBox_Send_1;
                    myFreqEdit = comboBox1;
                    mygroupBox = groupBox1;
                    break;
            }

            if (Button.Caption == "SelectBin")
            {
                String Path = Data.Path + @"注入码本\";
                if (!Directory.Exists(Path))
                    Directory.CreateDirectory(Path);
                openFileDialog1.InitialDirectory = Path;
                string tmpFilter = openFileDialog1.Filter;
                string title = openFileDialog1.Title;
                openFileDialog1.Title = "选择要注入的码表文件";
                openFileDialog1.Filter = "dat files (*.dat)|*.dat|All files (*.*) | *.*";

                if (openFileDialog1.ShowDialog() == DialogResult.OK) //selecting bitstream
                {
                    editor.Text = openFileDialog1.FileName;
                    Refresh();
                    MyLog.Info("选取" + msgstr + "文件成功");
                    Function.SetConfigValue(ConfigPath, editor.Text);

                    FileStream file = new FileStream(editor.Text, FileMode.Open, FileAccess.Read);

                    //int fileBytes = (int)file.Length + 8;//为何要+8??
                    int fileBytes = (int)file.Length;
                    byte[] read_file_buf = new byte[fileBytes];
                    for (int i = 0; i < fileBytes; i++) read_file_buf[i] = 0xff;
                    file.Read(read_file_buf, 0, fileBytes);

                    //1D0x + 长度2Bytes + 数据+(填写00填满32位) + 4 * C0DEC0DE
                    byte[] FinalSendBytes = new byte[fileBytes + 20];
                    byte[] head = new byte[2] { 0x1D, FrameHeadLastByte };
                    byte[] len = new byte[2] { 0, 0 };
                    len[0] = (byte)(fileBytes / 256);
                    len[1] = (byte)(fileBytes % 256);
                    byte[] end = new byte[16] { 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE };

                    head.CopyTo(FinalSendBytes, 0);
                    len.CopyTo(FinalSendBytes, 2);
                    read_file_buf.CopyTo(FinalSendBytes, 4);


                    int AddToFour = fileBytes % 4;
                    if (AddToFour != 0)
                    {
                        byte[] add_buf = new byte[4 - AddToFour];//4-余数才是要补的数据
                        for (int t = 0; t < add_buf.Count(); t++)
                        {
                            add_buf[t] = 0x0;
                        }
                        add_buf.CopyTo(FinalSendBytes, fileBytes + 4);
                        end.CopyTo(FinalSendBytes, fileBytes + 4 + add_buf.Count());
                    }
                    else
                    {
                        end.CopyTo(FinalSendBytes, fileBytes + 4);
                    }

                    file.Close();

                    string temp = null;
                    for (int i = 0; i < FinalSendBytes.Length; i++) temp += FinalSendBytes[i].ToString("x2");
                    mytxtbox.AppendText(temp);
                    mygroupBox.Text = mygroupBox.Text + "(" + (FinalSendBytes.Length - 20).ToString() + ")";
                }
                else
                {
                    openFileDialog1.Filter = tmpFilter;
                    openFileDialog1.Title = title;
                    return;
                }
            }
            else
            {
                if (Button.Caption == "开始")
                {
                    String freqstr = myFreqEdit.Text;
                    int freqint = 0;
                    bool ret = int.TryParse(freqstr.Substring(0, 2), out freqint);
                    if (ret)
                    {
                        byte freqcode = (byte)(60 / freqint);

                        if (USB.MyDeviceList[Data.OnlyID] != null)
                        {
                            USB.SendCMD(Data.OnlyID, (byte)(0x85 + (FrameHeadLastByte - 0x08)), freqcode);

                            USB.SendCMD(Data.OnlyID, 0x83, (byte)(0x01 << (byte)(FrameHeadLastByte - 0x08)));
                            USB.SendCMD(Data.OnlyID, 0x83, 0x00);

                            byte[] SendBuf = Function.StrToHexByte(mytxtbox.Text);

                            USB.SendData(Data.OnlyID, SendBuf);

                            Button.Image = CKServer.Properties.Resources.remove_16x16;
                            myFreqEdit.Enabled = false;
                            Button.Caption = "停止";
                        }
                        else
                        {
                            MyLog.Error("向设备" + msgstr + "注入码表失败，请检查设置及连接！");
                        }
                    }
                    else
                    {
                        MessageBox.Show("输入正确的频率！！");
                    }

                }
                else
                {
                    Button.Caption = "开始";
                    myFreqEdit.Enabled = true;
                    Button.Image = Properties.Resources.download_16x16;

                    USB.SendCMD(Data.OnlyID, 0x83, (byte)(0x01 << (byte)(FrameHeadLastByte - 0x08)));
                    USB.SendCMD(Data.OnlyID, 0x83, 0x00);

                }

            }
        }

        private void CheckEnable_LVDS_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            DevExpress.XtraBars.BarCheckItem chk = (DevExpress.XtraBars.BarCheckItem)sender;
            DevExpress.XtraBars.Docking.DockPanel panel;
            switch (chk.Name)
            {
                case "CheckEnable_LVDS_Send":
                    panel = this.dockPanel_LVDS_Send;
                    break;
                case "CheckEnable_LVDS_Recv":
                    panel = this.dockPanel_LVDS_Recv;
                    break;
                case "CheckEnable_422_A":
                    panel = this.dockPanel_422_A;
                    break;
                case "CheckEnable_422_B":
                    panel = this.dockPanel_422_B;
                    break;
                case "CheckEnable_AD":
                    panel = this.dockPanel_AD;
                    break;
                case "CheckEnable_OC":
                    panel = this.dockPanel_OC;
                    break;
                case "CheckEnable_LOG":
                    panel = this.dockPanel_LOG;
                    break;
                case "CheckEnable_ManualYK":
                    panel = this.dockPanel1;
                    break;
                default:
                    panel = this.dockPanel_LVDS_Send;
                    break;


            }
            if (chk.Checked == true)
            {
                panel.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
            }
            else
            {
                panel.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Hidden;
            }

        }

        private void btn_OpenPath_Log_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            System.Diagnostics.Process.Start("Explorer", Program.GetStartupPath() + @"LogData\");
        }

        private void btn_OpenPath_Storage_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            System.Diagnostics.Process.Start("Explorer", Program.GetStartupPath() + @"接收机箱数据\");
        }

        private void btn_RegSet_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (null != barEditI_RegAddr.EditValue)
            {
                string temp_addr = barEditI_RegAddr.EditValue.ToString();
                string temp_value = barEditI_RegValue.EditValue.ToString();
                byte addr = 0x00;
                byte value = 0x80;

                try
                {
                    addr = Convert.ToByte(temp_addr, 16);
                    value = Convert.ToByte(temp_value, 16);

                    if (addr < 0xff && addr >= 0x80 && value < 0x80 && value >= 0x0)
                    {
                        if (!USB.SendCMD(Data.Cardid, addr, value)) MessageBox.Show("设备未连接，请检查连接！");
                    }
                    else
                    {
                        MessageBox.Show("地址设置在0x80~0xff,参数设置在0x00~0x7f之间，请输入有效值!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("地址设置在0x80~0xff,参数设置在0x00~0x7f之间!\n" + ex.Message);
                }
            }
        }

        private void btn_RegCtl_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            dockPanel_RegCtl.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
        }

        private void dataGridView_Reg_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (e.ColumnIndex > 3 && (string)Register.dt_Reg.Rows[e.RowIndex][e.ColumnIndex] != "/")
                {
                    string temp_addr = (string)Register.dt_Reg.Rows[e.RowIndex]["地址"];
                    byte addr = Convert.ToByte(temp_addr, 16);

                    string TextName = (string)Register.dt_Reg.Rows[e.RowIndex][e.ColumnIndex];
                    string[] tempList = TextName.Split(':');
                    int bitpos = Register.dt_Reg.Columns.Count - e.ColumnIndex - 1;//共11列的时候，Count=11,最后一列的ColumnIndex=10,所以要多-1

                    if (tempList[0] == "0")
                    {
                        Register.dt_Reg.Rows[e.RowIndex][e.ColumnIndex] = "1:" + tempList[1];
                        Register.RegDictionary[addr] = (byte)(Register.RegDictionary[addr] | (byte)(0x01 << bitpos));
                    }
                    else
                    {
                        Register.dt_Reg.Rows[e.RowIndex][e.ColumnIndex] = "0:" + tempList[1];
                        Register.RegDictionary[addr] = (byte)(Register.RegDictionary[addr] & (byte)(0x7f - (byte)(0x01 << bitpos)));
                    }
                    Trace.WriteLine(addr.ToString("x2") + ":" + Register.RegDictionary[addr].ToString("x2"));
                    USB.SendCMD(Data.Cardid, addr, Register.RegDictionary[addr]);
                }
            }

        }

        private void btn_Start422_A_Click(object sender, EventArgs e)
        {
            byte[] SendData = new byte[6] { 0xeb, 0x90, 0x05, 0x02, 0x01, 0x82 };
            byte[] end = new byte[16] { 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE };
            byte[] FinalSend = new byte[28];

            FinalSend[0] = 0x1D;
            FinalSend[1] = 0x00;
            FinalSend[2] = 0x00;
            FinalSend[3] = 0x06;
            SendData.CopyTo(FinalSend, 4);

            FinalSend[10] = 0x00;
            FinalSend[11] = 0x00;

            end.CopyTo(FinalSend, 12);

            USB.SendData(Data.OnlyID, FinalSend);
        }

        public string Rs422_Channel_Name;           //
        private void btn_Send422_A_Click(object sender, EventArgs e)
        {
            byte[] SendBuf = Function.StrToHexByte(textBox_Send422_A.Text);

            if (SendBuf.Length > 10)
                USB.SendData(Data.OnlyID, SendBuf);
            else
                MyLog.Error("输入正确的遥控注数数据！");
        }

        private void textBox_Send422_A_Click(object sender, EventArgs e)
        {
            Rs422_Channel_Name = ((TextBox)sender).Name;
            if (myRs422FrameProduceForm != null)
            {
                myRs422FrameProduceForm.Activate();
            }
            else
            {
                myRs422FrameProduceForm = new RS422FrameProduceForm(this);
            }
            myRs422FrameProduceForm.ShowDialog();
        }

        private void btn_Start422_B_Click(object sender, EventArgs e)
        {
            byte[] SendData = new byte[6] { 0xeb, 0x90, 0x05, 0x02, 0x01, 0x82 };
            byte[] end = new byte[16] { 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE };
            byte[] FinalSend = new byte[28];

            FinalSend[0] = 0x1D;
            FinalSend[1] = 0x01;
            FinalSend[2] = 0x00;
            FinalSend[3] = 0x06;
            SendData.CopyTo(FinalSend, 4);

            FinalSend[10] = 0x00;
            FinalSend[11] = 0x00;

            end.CopyTo(FinalSend, 12);

            USB.SendData(Data.OnlyID, FinalSend);
        }

        private void btn_Send422_B_Click(object sender, EventArgs e)
        {
            Rs422_Channel_Name = ((TextBox)sender).Name;
            if (myRs422FrameProduceForm != null)
            {
                myRs422FrameProduceForm.Activate();
            }
            else
            {
                myRs422FrameProduceForm = new RS422FrameProduceForm(this);
            }
            myRs422FrameProduceForm.ShowDialog();
        }


        private void barButton_AStart_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            byte[] SendData = new byte[6] { 0xeb, 0x90, 0x05, 0x02, 0x01, 0x82 };
            byte[] end = new byte[16] { 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE };
            byte[] FinalSend = new byte[28];

            FinalSend[0] = 0x1D;
            FinalSend[1] = 0x00;
            FinalSend[2] = 0x00;
            FinalSend[3] = 0x06;
            SendData.CopyTo(FinalSend, 4);

            FinalSend[10] = 0x00;
            FinalSend[11] = 0x00;

            end.CopyTo(FinalSend, 12);

            USB.SendData(Data.OnlyID, FinalSend);
        }

        private void barButton_BStart_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            byte[] SendData = new byte[6] { 0xeb, 0x90, 0x05, 0x02, 0x01, 0x82 };
            byte[] end = new byte[16] { 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE };
            byte[] FinalSend = new byte[28];

            FinalSend[0] = 0x1D;
            FinalSend[1] = 0x01;
            FinalSend[2] = 0x00;
            FinalSend[3] = 0x06;
            SendData.CopyTo(FinalSend, 4);

            FinalSend[10] = 0x00;
            FinalSend[11] = 0x00;

            end.CopyTo(FinalSend, 12);

            USB.SendData(Data.OnlyID, FinalSend);
        }

        private void bar_Send422_A_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            string YKcode = "";
            string YKstr = barEdit_YKSelect_A.EditValue.ToString();
            switch (YKstr)
            {
                case "刷新允许":
                    YKcode = "0101";
                    break;
                case "刷新禁止":
                    YKcode = "0202";
                    break;
                case "收发通信机发射开":
                    YKcode = "0303";
                    break;
                case "收发通信机发射关":
                    YKcode = "0404";
                    break;
                case "发射码速率1Mbps模式":
                    YKcode = "1010";
                    break;
                case "发射码速率2Mbps模式":
                    YKcode = "1111";
                    break;
                case "发射码速率10Mbps模式":
                    YKcode = "1212";
                    break;
                case "发射码速率15Mbps模式":
                    YKcode = "1313";
                    break;
                case "发射码速率30Mbps模式":
                    YKcode = "1414";
                    break;
                case "接收码速率1Mbps模式":
                    YKcode = "2020";
                    break;
                case "接收码速率2Mbps模式":
                    YKcode = "2121";
                    break;
                case "接收码速率10Mbps模式":
                    YKcode = "2222";
                    break;
                case "接收码速率15Mbps模式":
                    YKcode = "2323";
                    break;
                case "接收码速率30Mbps模式":
                    YKcode = "2424";
                    break;
                default:
                    YKcode = "0202";
                    break;
            }

            //EB90020c163CC0000005000002020202
            string header = "eb90020c";
            string body = "163cc00000050000" + YKcode;
            string hcrc = "0000";

            int crc = 0;
            int count = body.Length / 4;
            if (body.Length % 4 == 0)
            {
                for (int m = 0; m < body.Length / 4; m++)
                {
                    int temp = Convert.ToInt32(body.Substring(m * 4, 4), 16);
                    crc ^= temp;
                }
                hcrc = crc.ToString("x4");
            }

            byte[] SendData = Function.StrToHexByte(header + body + hcrc);

            //1D0x + 长度2Bytes + 数据+(填写00填满32位) + 4 * C0DEC0DE
            byte[] FinalSend = new byte[SendData.Length + 20];
            byte[] head = new byte[2] { 0x1D, 0x00 };
            byte[] len = new byte[2] { 0, 0 };
            len[0] = (byte)((byte)(SendData.Length & 0xff00) >> 8);
            len[1] = (byte)(SendData.Length & 0xff);
            byte[] end = new byte[16] { 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE };

            head.CopyTo(FinalSend, 0);
            len.CopyTo(FinalSend, 2);
            SendData.CopyTo(FinalSend, 4);
            end.CopyTo(FinalSend, 4 + SendData.Length);


            if (FinalSend.Length > 10)
                USB.SendData(Data.OnlyID, FinalSend);
            else
                MyLog.Error("输入正确的遥控注数数据！");
        }

        private void bar_Send422_B_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            string YKcode = "";
            string YKstr = barEdit_YKSelect_B.EditValue.ToString();
            switch (YKstr)
            {
                case "刷新允许":
                    YKcode = "0101";
                    break;
                case "刷新禁止":
                    YKcode = "0202";
                    break;
                case "收发通信机发射开":
                    YKcode = "0303";
                    break;
                case "收发通信机发射关":
                    YKcode = "0404";
                    break;
                case "发射码速率1Mbps模式":
                    YKcode = "1010";
                    break;
                case "发射码速率2Mbps模式":
                    YKcode = "1111";
                    break;
                case "发射码速率10Mbps模式":
                    YKcode = "1212";
                    break;
                case "发射码速率15Mbps模式":
                    YKcode = "1313";
                    break;
                case "发射码速率30Mbps模式":
                    YKcode = "1414";
                    break;
                case "接收码速率1Mbps模式":
                    YKcode = "2020";
                    break;
                case "接收码速率2Mbps模式":
                    YKcode = "2121";
                    break;
                case "接收码速率10Mbps模式":
                    YKcode = "2222";
                    break;
                case "接收码速率15Mbps模式":
                    YKcode = "2323";
                    break;
                case "接收码速率30Mbps模式":
                    YKcode = "2424";
                    break;
                default:
                    YKcode = "0202";
                    break;
            }

            //EB90020c163CC0000005000002020202
            string header = "eb90020c";
            string body = "163cc00000050000" + YKcode;
            string hcrc = "0000";

            int crc = 0;
            int count = body.Length / 4;
            if (body.Length % 4 == 0)
            {
                for (int m = 0; m < body.Length / 4; m++)
                {
                    int temp = Convert.ToInt32(body.Substring(m * 4, 4), 16);
                    crc ^= temp;
                }
                hcrc = crc.ToString("x4");
            }

            byte[] SendData = Function.StrToHexByte(header + body + hcrc);

            //1D0x + 长度2Bytes + 数据+(填写00填满32位) + 4 * C0DEC0DE
            byte[] FinalSend = new byte[SendData.Length + 20];
            byte[] head = new byte[2] { 0x1D, 0x01 };
            byte[] len = new byte[2] { 0, 0 };
            len[0] = (byte)((byte)(SendData.Length & 0xff00) >> 8);
            len[1] = (byte)(SendData.Length & 0xff);
            byte[] end = new byte[16] { 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE };

            head.CopyTo(FinalSend, 0);
            len.CopyTo(FinalSend, 2);
            SendData.CopyTo(FinalSend, 4);
            end.CopyTo(FinalSend, 4 + SendData.Length);


            if (FinalSend.Length > 10)
                USB.SendData(Data.OnlyID, FinalSend);
            else
                MyLog.Error("输入正确的遥控注数数据！");
        }

        private void btn_SetLvdsComPare_Frame_Click(object sender, EventArgs e)
        {
            int head = Convert.ToInt32((string)Func_LVDS.dt_LVDS_CP.Rows[0]["设定值"], 16);
            Func_LVDS.ComPareBuf[0] = (byte)((head & 0xff000000) >> 24);
            Func_LVDS.ComPareBuf[1] = (byte)((head & 0xff0000) >> 16);
            Func_LVDS.ComPareBuf[2] = (byte)((head & 0xff00) >> 8);
            Func_LVDS.ComPareBuf[3] = (byte)(head & 0xff);

            //版本号，源飞行器
            byte b1 = Convert.ToByte((string)Func_LVDS.dt_LVDS_CP.Rows[1]["设定值"], 16);
            byte b2 = Convert.ToByte((string)Func_LVDS.dt_LVDS_CP.Rows[2]["设定值"], 16);
            Func_LVDS.ComPareBuf[4] = (byte)(((b1 << 6) & 0xc0) | (b2 >> 2));

            //VCID
            byte b3 = Convert.ToByte((string)Func_LVDS.dt_LVDS_CP.Rows[3]["设定值"], 16);
            Func_LVDS.ComPareBuf[5] = (byte)(((b2 << 6) & 0xc0) | (b3 >> 2));

            //VCDU计数
            int VCDUCounts = Convert.ToInt32((string)Func_LVDS.dt_LVDS_CP.Rows[4]["设定值"], 16);
            Func_LVDS.ComPareBuf[6] = (byte)((VCDUCounts & 0xff0000) >> 16);
            Func_LVDS.ComPareBuf[7] = (byte)((VCDUCounts & 0xff00) >> 8);
            Func_LVDS.ComPareBuf[8] = (byte)(VCDUCounts & 0xff);

            //信号域
            Func_LVDS.ComPareBuf[9] = Convert.ToByte((string)Func_LVDS.dt_LVDS_CP.Rows[5]["设定值"], 16);

            //密钥区
            string key = (string)Func_LVDS.dt_LVDS_CP.Rows[6]["设定值"];
            key = key.PadLeft(24, '0');
            for (int i = 0; i < 12; i++)
            {
                Func_LVDS.ComPareBuf[10 + i] = Convert.ToByte(key.Substring(2 * i, 2), 16);
            }

            //目标飞行器
            Func_LVDS.ComPareBuf[22] = Convert.ToByte((string)Func_LVDS.dt_LVDS_CP.Rows[7]["设定值"], 16);

            //备用
            Func_LVDS.ComPareBuf[23] = Convert.ToByte((string)Func_LVDS.dt_LVDS_CP.Rows[8]["设定值"], 16);


            //数据域
            byte[] data = Function.StrToHexByte(textBox_lvdsFrame_Data.Text);

            for (int i = 0; i < 872; i = i + data.Length)
            {
                data.CopyTo(Func_LVDS.ComPareBuf, 24 + i);
            }
            
        }



        private void buttonEdit3_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            String Path = Data.Path + @"接收机箱数据\";
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);
            openFileDialog1.InitialDirectory = Path;
            string tmpFilter = openFileDialog1.Filter;
            string title = openFileDialog1.Title;
            openFileDialog1.Title = "选择要比对的码表文件";
            openFileDialog1.Filter = "dat files (*.dat)|*.dat|All files (*.*) | *.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK) //selecting bitstream
            {
                buttonEdit3.Text = openFileDialog1.FileName;
                FileStream file = new FileStream(openFileDialog1.FileName, FileMode.Open, FileAccess.Read);

                //int fileBytes = (int)file.Length + 8;//为何要+8??
                int fileBytes = (int)file.Length;
                byte[] read_file_buf = new byte[fileBytes];
                for (int i = 0; i < fileBytes; i++) read_file_buf[i] = 0xff;
                file.Read(read_file_buf, 0, fileBytes);

                file.Close();
                barbtn_setFrame.Enabled = true;
            }
            else
            {
                barbtn_setFrame.Enabled = false;
            }

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if(radioButton2.Checked)
            {
                buttonEdit3.Enabled = true;
            }
            else
            {
                buttonEdit3.Enabled = false;
            }
        }

        private void barbtn_StartComP_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (barbtn_StartComP.Caption == "开始比对")
            {
                barbtn_StartComP.Caption = "停止比对";
                barbtn_StartComP.ImageOptions.LargeImage = CKServer.Properties.Resources.stop_32x32;
            }
            else
            {
                barbtn_StartComP.Caption = "开始比对";
                barbtn_StartComP.ImageOptions.LargeImage = CKServer.Properties.Resources.play_32x32;
            }
        }

        private void barbtn_setFrame_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            int head = Convert.ToInt32((string)Func_LVDS.dt_LVDS_CP.Rows[0]["设定值"], 16);
            Func_LVDS.ComPareBuf[0] = (byte)((head & 0xff000000) >> 24);
            Func_LVDS.ComPareBuf[1] = (byte)((head & 0xff0000) >> 16);
            Func_LVDS.ComPareBuf[2] = (byte)((head & 0xff00) >> 8);
            Func_LVDS.ComPareBuf[3] = (byte)(head & 0xff);

            //版本号，源飞行器
            byte b1 = Convert.ToByte((string)Func_LVDS.dt_LVDS_CP.Rows[1]["设定值"], 16);
            byte b2 = Convert.ToByte((string)Func_LVDS.dt_LVDS_CP.Rows[2]["设定值"], 16);
            Func_LVDS.ComPareBuf[4] = (byte)(((b1 << 6) & 0xc0) | (b2 >> 2));

            //VCID
            byte b3 = Convert.ToByte((string)Func_LVDS.dt_LVDS_CP.Rows[3]["设定值"], 16);
            Func_LVDS.ComPareBuf[5] = (byte)(((b2 << 6) & 0xc0) | (b3 >> 2));

            //VCDU计数
            int VCDUCounts = Convert.ToInt32((string)Func_LVDS.dt_LVDS_CP.Rows[4]["设定值"], 16);
            Func_LVDS.ComPareBuf[6] = (byte)((VCDUCounts & 0xff0000) >> 16);
            Func_LVDS.ComPareBuf[7] = (byte)((VCDUCounts & 0xff00) >> 8);
            Func_LVDS.ComPareBuf[8] = (byte)(VCDUCounts & 0xff);

            //信号域
            Func_LVDS.ComPareBuf[9] = Convert.ToByte((string)Func_LVDS.dt_LVDS_CP.Rows[5]["设定值"], 16);

            //密钥区
            string key = (string)Func_LVDS.dt_LVDS_CP.Rows[6]["设定值"];
            key = key.PadLeft(24, '0');
            for (int i = 0; i < 12; i++)
            {
                Func_LVDS.ComPareBuf[10 + i] = Convert.ToByte(key.Substring(2 * i, 2), 16);
            }

            //目标飞行器
            Func_LVDS.ComPareBuf[22] = Convert.ToByte((string)Func_LVDS.dt_LVDS_CP.Rows[7]["设定值"], 16);

            //备用
            Func_LVDS.ComPareBuf[23] = Convert.ToByte((string)Func_LVDS.dt_LVDS_CP.Rows[8]["设定值"], 16);


            //数据域
            byte[] data = Function.StrToHexByte(textBox_lvdsFrame_Data.Text);

            for (int i = 0; i < 872; i = i + data.Length)
            {
                data.CopyTo(Func_LVDS.ComPareBuf, 24 + i);
            }

        }

        private void barbtn_savFrame_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            byte[] SvaeBuf = new byte[896];

            int head = Convert.ToInt32((string)Func_LVDS.dt_LVDS_CP.Rows[0]["设定值"], 16);
            SvaeBuf[0] = (byte)((head & 0xff000000) >> 24);
            SvaeBuf[1] = (byte)((head & 0xff0000) >> 16);
            SvaeBuf[2] = (byte)((head & 0xff00) >> 8);
            SvaeBuf[3] = (byte)(head & 0xff);

            //版本号，源飞行器
            byte b1 = Convert.ToByte((string)Func_LVDS.dt_LVDS_CP.Rows[1]["设定值"], 16);
            byte b2 = Convert.ToByte((string)Func_LVDS.dt_LVDS_CP.Rows[2]["设定值"], 16);
            SvaeBuf[4] = (byte)(((b1 << 6) & 0xc0) | (b2 >> 2));

            //VCID
            byte b3 = Convert.ToByte((string)Func_LVDS.dt_LVDS_CP.Rows[3]["设定值"], 16);
            SvaeBuf[5] = (byte)(((b2 << 6) & 0xc0) | (b3 >> 2));

            //VCDU计数
            int VCDUCounts = Convert.ToInt32((string)Func_LVDS.dt_LVDS_CP.Rows[4]["设定值"], 16);
            SvaeBuf[6] = (byte)((VCDUCounts & 0xff0000) >> 16);
            SvaeBuf[7] = (byte)((VCDUCounts & 0xff00) >> 8);
            SvaeBuf[8] = (byte)(VCDUCounts & 0xff);

            //信号域
            SvaeBuf[9] = Convert.ToByte((string)Func_LVDS.dt_LVDS_CP.Rows[5]["设定值"], 16);

            //密钥区
            string key = (string)Func_LVDS.dt_LVDS_CP.Rows[6]["设定值"];
            key = key.PadLeft(24, '0');
            for (int i = 0; i < 12; i++)
            {
                SvaeBuf[10 + i] = Convert.ToByte(key.Substring(2 * i, 2), 16);
            }

            //目标飞行器
            SvaeBuf[22] = Convert.ToByte((string)Func_LVDS.dt_LVDS_CP.Rows[7]["设定值"], 16);

            //备用
            SvaeBuf[23] = Convert.ToByte((string)Func_LVDS.dt_LVDS_CP.Rows[8]["设定值"], 16);


            //数据域
            byte[] data = Function.StrToHexByte(textBox_lvdsFrame_Data.Text);

            for (int i = 0; i < 872; i = i + data.Length)
            {
                data.CopyTo(SvaeBuf, 24 + i);
            }


            String Path = Program.GetStartupPath() + @"码本文件\";
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            saveFileDialog2.InitialDirectory = Path;

            saveFileDialog2.Filter = "dat文件(*.dat)|*.dat|All files(*.*)|*.*";
            saveFileDialog2.FilterIndex = 1;
            saveFileDialog2.RestoreDirectory = true;

            if (saveFileDialog2.ShowDialog() == DialogResult.OK)
            {
                string localFilePath = saveFileDialog2.FileName.ToString(); //获得文件路径 
                FileStream file0 = new FileStream(localFilePath, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(file0);
                bw.Write(SvaeBuf);

                bw.Flush();
                bw.Close();
                file0.Close();
                MessageBox.Show("存储dat文件成功！", "保存文件");

            }
        }
    }
}
