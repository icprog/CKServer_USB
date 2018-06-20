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

        public DateTime startDT;
        // private DataTable dt_AD = new DataTable();
        //  private DataTable dt_OC = new DataTable();

        private DataTable dtModifyDA1 = new DataTable();
        private DataTable dtModifyDA2 = new DataTable();

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
                    Data.OnlyID = key;
                }
            }

        }
        public MainForm()
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                SetDevice(false);
                barStaticItem1.Caption = "存储路径" + Data.Path;
                InitDataTable();//初始化datadable
                Function.Init();//初始化DA参数      
                Data.init();

                Data.Status_FF00.Chanbtn = btn_Long_FF00;
                Data.Status_FF00.init();

                Data.Status_FF01.Chanbtn = btn_Long_FF01;
                Data.Status_FF01.init();

                Data.Status_FF02.Chanbtn = btn_Long_FF02;
                Data.Status_FF02.init();

                Data.Status_FF03.Chanbtn = btn_Long_FF03;
                Data.Status_FF03.init();


                Data.Status_FF08.Chanbtn = btn_Long_FF08;
                Data.Status_FF08.init();


                Data.Status_1D00.Chanbtn = btn_Short_1D00;
                Data.Status_1D00.init();

                Data.Status_1D01.Chanbtn = btn_Short_1D01;
                Data.Status_1D01.init();

                Data.Status_1D02.Chanbtn = btn_Short_1D02;
                Data.Status_1D02.init();

                Data.Status_1D03.Chanbtn = btn_Short_1D03;
                Data.Status_1D03.init();

                Data.Status_1D08.Chanbtn = btn_Short_1D08;
                Data.Status_1D08.init();

                Data.Status_1D0F.Chanbtn = btn_Short_1D0F;
                Data.Status_1D0F.init();


                //初始化DA的128个通道
                for (int i = 0; i < 128; i++)
                {
                    Func_DA.clcDAValue(0, i, 0);//DA子板0
                    Func_DA.clcDAValue(1, i, 0);//DA子板1
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void InitDataTable()
        {
            try
            {
                Func_AD.Init_Table();
                dataGridView_AD.DataSource = Func_AD.dt_AD;
                dataGridView_AD.AllowUserToAddRows = false;

                Func_OC.Init_Table();
                dataGridView_OC_Out.DataSource = Func_OC.dt_OC_Out;
                dataGridView_OC_Out.AllowUserToAddRows = false;

                dataGridView_OC_In1.DataSource = Func_OC.dt_OC_In1;
                dataGridView_OC_In2.DataSource = Func_OC.dt_OC_In2;
                dataGridView_OC_In3.DataSource = Func_OC.dt_OC_In3;
                dataGridView_OC_In1.AllowUserToAddRows = false;
                dataGridView_OC_In2.AllowUserToAddRows = false;
                dataGridView_OC_In3.AllowUserToAddRows = false;

                Func_DA.Init_Table();
                dataGridView_DA1.DataSource = Func_DA.dt_DA1;
                dataGridView_DA2.DataSource = Func_DA.dt_DA2;
                dataGridView_DA1.AllowUserToAddRows = false;
                dataGridView_DA2.AllowUserToAddRows = false;

                dtModifyDA1.Columns.Add("ID", typeof(Int32));
                dtModifyDA1.Columns.Add("a", typeof(Int32));
                dtModifyDA1.Columns.Add("b", typeof(Int32));
                for (int i = 0; i < 128; i++)
                {
                    DataRow dr = dtModifyDA1.NewRow();
                    dr["ID"] = i + 1;
                    dr["a"] = 130;
                    dr["b"] = 15600;
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
                    dr["b"] = 15600;
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

                //         Data.SaveConfig(Data.DAconfigPath, "DAModify_Board1_A" + (i).ToString(), dtModifyDA1.Rows[i]["a"].ToString());
                //        Data.SaveConfig(Data.DAconfigPath, "DAModify_Board1_B" + (i).ToString(), dtModifyDA1.Rows[i]["b"].ToString());

                Data.DA2_value_a[i] = (int)dtModifyDA2.Rows[i]["a"];
                Data.DA2_value_b[i] = (int)dtModifyDA2.Rows[i]["b"];

                //               Data.SaveConfig(Data.DAconfigPath, "DAModify_Board2_A" + (i).ToString(), dtModifyDA2.Rows[i]["a"].ToString());
                //             Data.SaveConfig(Data.DAconfigPath, "DAModify_Board2_B" + (i).ToString(), dtModifyDA2.Rows[i]["b"].ToString());
            }
            //       MessageBox.Show("参数修正已成功，可以关闭此窗口！");
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

            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

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

            for (int i = 0; i < 64; i++)
            {
                Func_AD.dt_AD.Rows[i]["测量值"] = dataRe_AD[i];
            }

            DisplayStatusLed(ref Data.Status_FF01);
            DisplayStatusLed(ref Data.Status_FF08);
            DisplayStatusLed(ref Data.Status_1D00);
            DisplayStatusLed(ref Data.Status_1D0F);
        }

        /// <summary>
        /// 显示Led，根据需要增减
        /// </summary>
        private void DisplayStatusLed(ref Data.ChanStatus_Struct myStatus)
        {
            if (myStatus.ChanActive)
            {
                myStatus.ChanActive = false;
                myStatus.Chanbtn.ImageOptions.LargeImage = Properties.Resources.green;
            }
            else
            {
                myStatus.Chanbtn.ImageOptions.LargeImage = Properties.Resources.red;
            }

            //if (Data.Status_FF01.ChanActive)
            //{
            //    Data.Status_FF01.ChanActive = false;
            //    Data.Status_FF01.Chanbtn.ImageOptions.LargeImage = Properties.Resources.green;
            //}
            //else
            //{
            //    Data.Status_FF01.Chanbtn.ImageOptions.LargeImage = Properties.Resources.red;
            //}

            //if (Data.Status_FF08.ChanActive)
            //{
            //    Data.Status_FF08.ChanActive = false;
            //    Data.Status_FF08.Chanbtn.ImageOptions.LargeImage = Properties.Resources.green;
            //}
            //else
            //{
            //    Data.Status_FF08.Chanbtn.ImageOptions.LargeImage = Properties.Resources.red;
            //}

            //if (Data.Status_1D08.ChanActive)
            //{
            //    Data.Status_1D08.ChanActive = false;
            //    Data.Status_1D08.Chanbtn.ImageOptions.LargeImage = Properties.Resources.green;
            //}
            //else
            //{
            //    Data.Status_1D08.Chanbtn.ImageOptions.LargeImage = Properties.Resources.red;
            //}

            //if (Data.Status_1D0F.ChanActive)
            //{
            //    Data.Status_1D0F.ChanActive = false;
            //    Data.Status_1D0F.Chanbtn.ImageOptions.LargeImage = Properties.Resources.green;
            //}
            //else
            //{
            //    Data.Status_1D0F.Chanbtn.ImageOptions.LargeImage = Properties.Resources.red;
            //}
        }

        private void btn_Modify_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            if (dockPanel_422.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Visible)
            {
                dockPanel_422.Show();
                MyLog.Info("显示422界面！");
            }
            else
            {
                dockPanel_422.Hide();
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
            }
            else
            {
                btn_Start.Caption = "一键开始";
                btn_Start.ImageOptions.LargeImage = CKServer.Properties.Resources.Start_btn;
                _BoxIsStarted = false;
                Thread.Sleep(200);

                FileThread.FileClose();


            }
        }

        public byte[] Recv_MidBuf_8K = new byte[8192];//8K中间缓存
        public int Pos_Recv_MidBuf_8K = 0;//中间缓存数据存储到哪个位置
        private void RecvFun(int key)
        {
            Trace.WriteLine("开启线程接收USB数据");
            CyUSBDevice MyDevice = USB.MyDeviceList[key];

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

            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x00)
            {
                Data.Status_FF00.ChanActive = true;
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                SaveFile.Lock_2.EnterWriteLock();
                SaveFile.DataQueue_SC2.Enqueue(bufsav);
                SaveFile.Lock_2.ExitWriteLock();
            }
            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x01)
            {
                Data.Status_FF01.ChanActive = true;
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                SaveFile.Lock_3.EnterWriteLock();
                SaveFile.DataQueue_SC3.Enqueue(bufsav);
                SaveFile.Lock_3.ExitWriteLock();
            }
            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x02)
            {
                Data.Status_FF02.ChanActive = true;
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                SaveFile.Lock_4.EnterWriteLock();
                SaveFile.DataQueue_SC4.Enqueue(bufsav);
                SaveFile.Lock_4.ExitWriteLock();
            }
            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x03)
            {
                Data.Status_FF03.ChanActive = true;
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                SaveFile.Lock_5.EnterWriteLock();
                SaveFile.DataQueue_SC5.Enqueue(bufsav);
                SaveFile.Lock_5.ExitWriteLock();
            }
            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x08)
            {
                Data.Status_FF08.ChanActive = true;
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
                        lock (ADList)
                        {//将AD数据放入ADList,在另一个线程中解析
                            for (int j = 0; j < buf1D0x.Length; j++) ADList.Add(buf1D0x[j]);
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
                    }
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x08)//1D08:AD数据
                    {
                        Data.Status_1D08.ChanActive = true;

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
                        Data.Status_1D0F.ChanActive = true;
                    }
                    else
                    {
                        Trace.WriteLine("FF08通道出错!");
                    }
                }
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
                    MyLog.Info("指令输出:" + Func_OC.dt_OC_Out.Rows[e.RowIndex]["名称"]);



                }
            }
        }

        private void buttonEdit1_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            ButtonEdit editor = (ButtonEdit)sender;
            EditorButton Button = e.Button;
            String msgstr = "未选择文件";
            String ConfigPath = "PATH_DAT_01";
            byte FrameHeadLastByte = 0x00;
            switch (editor.Name)
            {
                case "buttonEdit1":
                    msgstr = "通道1";
                    ConfigPath = "PATH_DAT_01";
                    FrameHeadLastByte = 0x00;
                    break;
                case "buttonEdit2":
                    msgstr = "通道2";
                    ConfigPath = "PATH_DAT_02";
                    FrameHeadLastByte = 0x01;
                    break;
                case "buttonEdit3":
                    msgstr = "通道3";
                    ConfigPath = "PATH_DAT_03";
                    FrameHeadLastByte = 0x02;
                    break;
                case "buttonEdit4":
                    msgstr = "通道4";
                    ConfigPath = "PATH_DAT_04";
                    FrameHeadLastByte = 0x03;
                    break;
                default:
                    msgstr = "通道1";
                    ConfigPath = "PATH_DAT_01";
                    FrameHeadLastByte = 0x00;
                    break;
            }

            if (Button.Caption == "SelectBin")
            {
                openFileDialog1.InitialDirectory = Data.Path;
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
                len[0] = (byte)((byte)(fileBytes & 0xff00) >> 8);
                len[1] = (byte)(fileBytes & 0xff);
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

                if (USB.MyDeviceList[Data.OnlyID] != null)
                {
                    USB.SendData(Data.OnlyID, FinalSendBytes);
                }
                else
                {
                    MyLog.Error("向设备" + msgstr + "注入码表失败，请检查设置及连接！");
                }


            }
        }

        private void CheckEnable_LVDS_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            DevExpress.XtraBars.BarCheckItem chk = (DevExpress.XtraBars.BarCheckItem)sender;
            DevExpress.XtraBars.Docking.DockPanel panel;
            switch (chk.Name)
            {
                case "CheckEnable_LVDS":
                    panel = this.dockPanel_LVDS;
                    break;
                case "CheckEnable_422":
                    panel = this.dockPanel_422;
                    break;
                case "CheckEnable_AD":
                    panel = this.dockPanel_AD;
                    break;
                case "CheckEnable_OC_Out":
                    panel = this.dockPanel_OC_Out;
                    break;
                case "CheckEnable_OC_In":
                    panel = this.dockPanel_OC_In;
                    break;
                case "CheckEnable_LOG":
                    panel = this.dockPanel_LOG;
                    break;
                case "CheckEnable_DA":
                    panel = this.dockPanel_DA;
                    break;
                case "CheckEnable_DAMODIFY":
                    panel = this.dockPanel_DAMODIFY;
                    break;
                default:
                    panel = this.dockPanel_LVDS;
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
            System.Diagnostics.Process.Start("Explorer", Program.GetStartupPath() + @"LogData\");
        }

        private void btn_DAOut_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            List<byte[]> mylist = new List<byte[]>();
            mylist.Add(Func_DA.DAByteA);
            mylist.Add(Func_DA.DAByteB);
            mylist.Add(Func_DA.DAByteC);
            mylist.Add(Func_DA.DAByteD);

            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 80; i++)
                {
                    double value = 0;
                    if (j == 0) value = (double)Func_DA.dt_DA1.Rows[i]["电压"];
                    if (j == 1) value = (double)Func_DA.dt_DA2.Rows[i]["电压"];
                    if (value < 0 || value > 10)
                    {
                        //Deal with exception
                        MyLog.Error("有一路DA值不合理，请检查！！");
                    }
                    else
                    {
                        Func_DA.clcDAValue(j, i, value);
                    }
                }

                for (int i = 0; i < 4; i++)
                {
                    byte[] DASend = new byte[128 + 8];
                    DASend[0] = 0x1D;
                    //1D20 1D21 1D22 1D23对应4个DA芯片
                    //1D24 1D25 1D26 1D27对应4个DA芯片
                    DASend[1] = (byte)(0x0 + i + 4 * j);
                    DASend[2] = 0x00;
                    DASend[3] = 0x80;//0x0080 = 128

                    Array.Copy(mylist[i], 0, DASend, 4, 128);

                    DASend[132] = 0xC0;
                    DASend[133] = 0xDE;
                    DASend[134] = 0xC0;
                    DASend[135] = 0xDE;

                    if ((i + 4 * j) == 7)
                    {
                        USB.SendCMD(Data.OnlyID, 0x82, 0x01);
                        USB.SendCMD(Data.OnlyID, 0x82, 0x00);
                    }
                    else
                    {
                        USB.SendCMD(Data.OnlyID, 0x81, (byte)((0x01) << (i + 4 * j)));
                        USB.SendCMD(Data.OnlyID, 0x81, 0x00);
                    }


                    USB.SendData(Data.DAid, DASend);
                }

            }
        }

        private void btn_da_add_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (null != barEdit_mdy.EditValue)
            {
                string ValueSet = barEdit_mdy.EditValue.ToString();
                double value = double.Parse(ValueSet);
                foreach (DataRow dr in Func_DA.dt_DA1.Rows)
                {
                    double bef_value = (double)dr["电压"];
                    double aft_value = bef_value + value;
                    if (aft_value >= 0 && aft_value <= 10)
                        dr["电压"] = aft_value;
                    else
                        dr["电压"] = 10;
                }

                foreach (DataRow dr in Func_DA.dt_DA2.Rows)
                {
                    double bef_value = (double)dr["电压"];
                    double aft_value = bef_value + value;
                    if (aft_value >= 0 && aft_value <= 10)
                        dr["电压"] = aft_value;
                    else
                        dr["电压"] = 10;
                }
            }
        }

        private void btn_da_dec_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (null != barEdit_mdy.EditValue)
            {
                string ValueSet = barEdit_mdy.EditValue.ToString();
                double value = double.Parse(ValueSet);
                foreach (DataRow dr in Func_DA.dt_DA1.Rows)
                {
                    double bef_value = (double)dr["电压"];
                    double aft_value = bef_value - value;
                    if (aft_value >= 0 && aft_value <= 10)
                        dr["电压"] = aft_value;
                    else
                        dr["电压"] = 0;
                }

                foreach (DataRow dr in Func_DA.dt_DA2.Rows)
                {
                    double bef_value = (double)dr["电压"];
                    double aft_value = bef_value - value;
                    if (aft_value >= 0 && aft_value <= 10)
                        dr["电压"] = aft_value;
                    else
                        dr["电压"] = 0;
                }
            }
        }

        private void barEdit_set_EditValueChanged(object sender, EventArgs e)
        {
            if (null != barEdit_set.EditValue)
            {
                string ValueSet = barEdit_set.EditValue.ToString();
                double value = double.Parse(ValueSet);

                foreach (DataRow dr in Func_DA.dt_DA1.Rows)
                {
                    dr["电压"] = value;
                }

                foreach (DataRow dr in Func_DA.dt_DA2.Rows)
                {
                    dr["电压"] = value;
                }
            }

        }

        private void dataGridView_DA1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            if (e.RowIndex >= 0)
            {
                string temp = dgv.Rows[e.RowIndex].Cells[2].FormattedValue.ToString();
                bool ret = false;
                double value = 0;
                ret = double.TryParse(temp, out value);
                if (ret)
                {
                    if (value <= 0 || value > 5)
                    {
                        dgv.Rows[e.RowIndex].Cells[2].Value = "5";
                    }
                    //      Func_DA.dt_DA1.Rows[e.RowIndex]["电压"] = dgv.Rows[e.RowIndex].Cells[2].Value;
                }
                else
                {
                    dgv.Rows[e.RowIndex].Cells[2].Value = "5";
                }
            }
        }

        private void dataGridView_DA2_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            if (e.RowIndex >= 0)
            {
                string temp = dgv.Rows[e.RowIndex].Cells[2].FormattedValue.ToString();
                bool ret = false;
                double value = 0;
                ret = double.TryParse(temp, out value);
                if (ret)
                {
                    if (value <= 0 || value > 5)
                    {
                        dgv.Rows[e.RowIndex].Cells[2].Value = "5";
                    }
                    //     Func_DA.dt_DA2.Rows[e.RowIndex]["电压"] = dgv.Rows[e.RowIndex].Cells[2].Value;
                }
                else
                {
                    dgv.Rows[e.RowIndex].Cells[2].Value = "5";
                }
            }
        }

        private void barEdit_OCout_set_EditValueChanged(object sender, EventArgs e)
        {
            if (null != barEdit_OCout_set.EditValue)
            {
                string ValueSet = barEdit_OCout_set.EditValue.ToString();
                int value = int.Parse(ValueSet);

                foreach (DataRow dr in Func_OC.dt_OC_Out.Rows)
                {
                    dr["脉宽"] = value;
                }

            }
        }

        private void btn_oc_add_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (null != barEdit_OCout_mdy.EditValue)
            {
                string ValueSet = barEdit_OCout_mdy.EditValue.ToString();
                int value = int.Parse(ValueSet);
                foreach (DataRow dr in Func_OC.dt_OC_Out.Rows)
                {
                    int bef_value = (int)dr["脉宽"];
                    int aft_value = bef_value + value;
                    if (aft_value >= 0 && aft_value <= 65535)
                        dr["脉宽"] = aft_value;
                    else
                        dr["脉宽"] = 10;
                }

            }
        }

        private void btn_oc_dec_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (null != barEdit_OCout_mdy.EditValue)
            {
                string ValueSet = barEdit_OCout_mdy.EditValue.ToString();
                int value = int.Parse(ValueSet);
                foreach (DataRow dr in Func_OC.dt_OC_Out.Rows)
                {
                    int bef_value = (int)dr["脉宽"];
                    int aft_value = bef_value - value;
                    if (aft_value >= 0 && aft_value <= 65535)
                        dr["脉宽"] = aft_value;
                    else
                        dr["脉宽"] = 10;
                }
            }
        }
    }
}
