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

        public DateTime startDT;
        private DataTable dt_AD = new DataTable();
        private DataTable dt_OC = new DataTable();

        private DataTable dtModifyDA1 = new DataTable();
        private DataTable dtModifyDA2 = new DataTable();

        SaveFile FileThread;

        bool _BoxIsStarted;

        Image imgError = Properties.Resources.error;
        Image imgGreen = Properties.Resources.green;
        Image imgGray = Properties.Resources.gray;

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

                try
                {
                    int key = int.Parse(fxDevice.ProductID.ToString("x4").Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    if (USB.MyDeviceList[key] == null)
                    {
                        USB.MyDeviceList[key] = (CyUSBDevice)fxDevice;

                        MyLog.Info(USB.MyDeviceList[key].FriendlyName + ConfigurationManager.AppSettings[USB.MyDeviceList[key].FriendlyName] + "板卡连接");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("监测到异常USB设备连接！" + ex.Message);
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
            InitDataTable();
            Function.Init();//初始化DA参数

        }

        private void InitDataTable()
        {
            try
            {
                Func_AD2_BIT.Init_Table();
                dataGridView_BIT1.DataSource = Func_AD2_BIT.dt_AD1;
                dataGridView_BIT2.DataSource = Func_AD2_BIT.dt_AD2;


                Func_AD.Init_Table();
                dataGridView4.DataSource = Func_AD.dt_AD;
                dataGridView4.AllowUserToAddRows = false;

                Func_OC.Init_Table();
                dataGridView5.DataSource = Func_OC.dt_OC_Out;
                dataGridView5.AllowUserToAddRows = false;


                dataGridView5.Rows[4].Visible = false;
                dataGridView5.Rows[9].Visible = false;

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
            for (int i = 0; i < 10; i++)
            {
                int[] ValueTable = new int[6] { 5, 5, 5, 5, 5, 5 };

                int transValue = (int)dt_OC.Rows[i]["脉宽"];

                if (transValue <= 0 || transValue > 2000000)
                {
                    MyLog.Error("脉宽必须设置在0-2000000us之间");
                }

                byte addr1 = (byte)(0x81 + 4 * i);
                byte addr2 = (byte)(0x82 + 4 * i);
                byte addr3 = (byte)(0x83 + 4 * i);
                byte addr4 = (byte)(0x84 + 4 * i);


                byte b1 = (byte)(transValue & 0x7f);
                byte b2 = (byte)((transValue & 0x3f80) >> 7);
                byte b3 = (byte)((transValue & 0x1fC000) >> 14);
                USB.SendCMD(Data.CardID1, addr2, b1);
                USB.SendCMD(Data.CardID1, addr3, b2);
                USB.SendCMD(Data.CardID1, addr4, b3);
                //边沿出发，发送一次脉冲
                USB.SendCMD(Data.CardID1, addr1, 0x1);
                USB.SendCMD(Data.CardID1, addr1, 0x0);
                Thread.Sleep(10);
            }
        }

        private void dataGridView5_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.Exception != null)
            {
                MessageBox.Show("请输入正确的脉宽，输入无效数值将自动设置为0");

                if (e.RowIndex >= 0)
                {
                    dt_OC.Rows[e.RowIndex]["脉宽"] = 0;
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

            if (MySPort.Port1 != null)
            {
                if (MySPort.Port1.IsOpen && MySPort.ShowAngleTag)
                {
                    byte[] SendData1 = new byte[7] { 0xff, 0x01, 0x00, 0x51, 0x00, 0x00, 0x52 };
                    MySPort.WritePort(SendData1);
                    Thread.Sleep(500);
                    byte[] SendData2 = new byte[7] { 0xff, 0x01, 0x00, 0x53, 0x00, 0x00, 0x54 };
                    MySPort.WritePort(SendData2);
                    this.textBox_Hangle.Text = MySPort.Hangle.ToString();
                    this.textBox_Vangle.Text = MySPort.Vangle.ToString();

                    double Hangl = MySPort.Hangle + (360 - MySPort.AddOnAngle);
                    if (Hangl >= 360) Hangl = Hangl - 360;
                    this.textBox_Hangle_add.Text = Hangl.ToString();
                    this.textBox_Vangle_add.Text = MySPort.Vangle.ToString();
                }
            }

            if (_BoxIsStarted)
            {
                //显示机箱00的AD数据
                for (int i = 0; i < Func_AD.ADNums; i++)
                {
                    Func_AD.dt_AD.Rows[i]["测量值"] = dataRe_AD[i];
                }

                //显示机箱01的AD数据
                for (int i = 0; i < Func_AD2_BIT.ADNums1; i++)
                {
                    if (AD_BIT1_L1[i] < 1)
                        dataGridView_BIT1.Rows[i].Cells["L1"].Value = imgGray;
                    else if (AD_BIT1_L1[i] > 4)
                        dataGridView_BIT1.Rows[i].Cells["L1"].Value = imgGreen;
                    else
                        dataGridView_BIT1.Rows[i].Cells["L1"].Value = imgError;

                    if (AD_BIT1_L2[i] < 1)
                        dataGridView_BIT1.Rows[i].Cells["L2"].Value = imgGray;
                    else if (AD_BIT1_L2[i] > 4)
                        dataGridView_BIT1.Rows[i].Cells["L2"].Value = imgGreen;
                    else
                        dataGridView_BIT1.Rows[i].Cells["L2"].Value = imgError;

                    if (AD_BIT1_L3[i] < 1)
                        dataGridView_BIT1.Rows[i].Cells["L3"].Value = imgGray;
                    else if (AD_BIT1_L3[i] > 4)
                        dataGridView_BIT1.Rows[i].Cells["L3"].Value = imgGreen;
                    else
                        dataGridView_BIT1.Rows[i].Cells["L3"].Value = imgError;

                    if (AD_BIT1_L4[i] < 1)
                        dataGridView_BIT1.Rows[i].Cells["L4"].Value = imgGray;
                    else if (AD_BIT1_L4[i] > 4)
                        dataGridView_BIT1.Rows[i].Cells["L4"].Value = imgGreen;
                    else
                        dataGridView_BIT1.Rows[i].Cells["L4"].Value = imgError;
                }

                for (int i = 0; i < Func_AD2_BIT.ADNums2; i++)
                {
                    if (AD_BIT2_L1[i] < 1)
                        dataGridView_BIT2.Rows[i].Cells["L5"].Value = imgGray;
                    else if (AD_BIT2_L1[i] > 4)
                        dataGridView_BIT2.Rows[i].Cells["L5"].Value = imgGreen;
                    else
                        dataGridView_BIT2.Rows[i].Cells["L5"].Value = imgError;

                    if (AD_BIT2_L2[i] < 1)
                        dataGridView_BIT2.Rows[i].Cells["L6"].Value = imgGray;
                    else if (AD_BIT2_L2[i] > 4)
                        dataGridView_BIT2.Rows[i].Cells["L6"].Value = imgGreen;
                    else
                        dataGridView_BIT2.Rows[i].Cells["L6"].Value = imgError;

                    if (AD_BIT2_L3[i] < 1)
                        dataGridView_BIT2.Rows[i].Cells["L7"].Value = imgGray;
                    else if (AD_BIT2_L3[i] > 4)
                        dataGridView_BIT2.Rows[i].Cells["L7"].Value = imgGreen;
                    else
                        dataGridView_BIT2.Rows[i].Cells["L7"].Value = imgError;

                    if (AD_BIT2_L4[i] < 1)
                        dataGridView_BIT2.Rows[i].Cells["L8"].Value = imgGray;
                    else if (AD_BIT2_L4[i] > 4)
                        dataGridView_BIT2.Rows[i].Cells["L8"].Value = imgGreen;
                    else
                        dataGridView_BIT2.Rows[i].Cells["L8"].Value = imgError;
                }
            }


        }



        private void btn_SerialOpen_Click(object sender, EventArgs e)
        {
            if (btn_SerialOpen.Text == "打开端口")
            {
                btn_SerialOpen.Text = "关闭端口";
                int ret = MySPort.Open(comboBox_SerialPortNum.Text, comboBox_SerialBaudrate.Text, comboBox_SerialDatabit.Text,
                    comboBox_SerialStopbit.Text, comboBox_SerialParity.Text);
                if (ret == 0) MessageBox.Show("打开串口失败，请检查串口设置！");
            }
            else
            {
                MySPort.close();
                btn_SerialOpen.Text = "打开端口";
            }
        }

        private void comboBox_SerialPortNum_DropDown(object sender, EventArgs e)
        {
            string[] str = MySPort.GetPortNames();
            if (str == null)
            {
                MyLog.Info("尝试选择串口,但是本机没有串口！");
            }
            comboBox_SerialPortNum.Items.AddRange(str);
            int count = comboBox_SerialPortNum.Items.Count;

            for (int i = 0; i < count; i++)
            {
                string str1 = comboBox_SerialPortNum.Items[i].ToString();
                for (int j = i + 1; j < count; j++)
                {
                    string str2 = comboBox_SerialPortNum.Items[j].ToString();
                    if (str1 == str2)
                    {
                        comboBox_SerialPortNum.Items.RemoveAt(j);
                        count--;
                        j--;
                    }
                }
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            double t = double.Parse(textBox1.Text);
            MySPort.SetPanPosition(t);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            double t = double.Parse(textBox2.Text);
            MySPort.SetTiltPosition(t);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] SendData1 = new byte[7] { 0xff, 0x01, 0x00, 0x51, 0x00, 0x00, 0x52 };
            MySPort.WritePort(SendData1);
            Thread.Sleep(1000);
            byte[] SendData2 = new byte[7] { 0xff, 0x01, 0x00, 0x53, 0x00, 0x00, 0x54 };
            MySPort.WritePort(SendData2);
        }
        /// <summary>
        /// 转台控制panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dockPanel6_ClosingPanel(object sender, DevExpress.XtraBars.Docking.DockPanelCancelEventArgs e)
        {
            MySPort.ShowAngleTag = false;
            MySPort.close();
        }

        private void btn_485_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (dockPanel_Rotator.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Visible)
            {
                dockPanel_Rotator.Show();
                MyLog.Info("显示转台控制界面！");
            }
            else
            {
                dockPanel_Rotator.Hide();
                MyLog.Info("隐藏转台控制界面！");
            }

        }

        private void btn_Modify_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            if (dockPanel_RS422.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Visible)
            {
                dockPanel_RS422.Show();
                MyLog.Info("显示422界面！");
            }
            else
            {
                dockPanel_RS422.Hide();
                MyLog.Info("隐藏422界面！");
            }

        }

        private void btn_ADStart_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                double startAngle = double.Parse(this.textBox_startAngle.Text);
                double stopAngle = double.Parse(this.textBox_stopAngle.Text);

                byte speed = 0x00;
                switch (comboBox2.Text)
                {
                    case "2°/S":
                        speed = 12;
                        break;
                    case "5°/S":
                        speed = 20;
                        break;
                    case "10°/S":
                        speed = 27;
                        break;
                    default:
                        speed = 20;
                        break;
                }
                string direction = comboBox1.Text;
                new Thread(() => { RunCruise(startAngle, stopAngle, speed, direction); }).Start();
            }
            catch (Exception ex)
            {
                MyLog.Error(ex.Message);
            }

        }

        public void RunCruise(double startAngle, double stopAngle, byte speed, string direction)
        {
            while (true)
            {
                double TempAngle = MySPort.Hangle + (360 - MySPort.AddOnAngle);
                if (TempAngle >= 360) TempAngle = TempAngle - 360;

                if (TempAngle == startAngle || (TempAngle - 360) == startAngle)
                {
                    MyLog.Info("转到起点位置");
                    break;
                }
                else
                {
                    MySPort.SetPanPosition(startAngle);            //转到起点位置
                    Thread.Sleep(3000);
                }
            }

            //开始巡航
            MySPort.SetCruise(direction, speed);
            MyLog.Info("开始巡航");
            Thread.Sleep(1000);
            //转到停止位置后停止

            while (true)
            {
                double TempAngle = MySPort.Hangle + (360 - MySPort.AddOnAngle);
                if (TempAngle >= 360) TempAngle = TempAngle - 360;

                if (TempAngle >= stopAngle && TempAngle <= (stopAngle + 20))
                {
                    MyLog.Info("转到停止位置");
                    MySPort.SetCruise("00", 0x0);
                    Thread.Sleep(100);
                    MySPort.SetCruise("00", 0x0);
                    break;
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (button5.Text == "开启补偿")
            {
                button5.Text = "关闭补偿";
                MySPort.AddOnAngle = Convert.ToDouble(textBox_addAngle.Text);
            }
            else
            {
                button5.Text = "开启补偿";
                MySPort.AddOnAngle = 0;
            }
        }

        private void CheckEnable_Log_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {






        }

        private void CheckEnable_AD_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            DevExpress.XtraBars.BarCheckItem chk = (DevExpress.XtraBars.BarCheckItem)sender;
            DevExpress.XtraBars.Docking.DockPanel panel;
            switch (chk.Name)
            {
                case "CheckEnable_AD":
                    panel = this.dockPanel_AD;
                    break;
                case "CheckEnable_OC":
                    panel = this.dockPanel_OC;
                    break;

                case "CheckEnable_LOG":
                    panel = this.dockPanel_LOG;
                    break;

                case "CheckEnable_Rotator":
                    panel = this.dockPanel_Rotator;
                    break;
                case "CheckEnable_RS422":
                    panel = this.dockPanel_RS422;
                    break;
                case "CheckEnable_BIT":
                    panel = this.dockPanel_BIT;
                    break;
                default:
                    panel = this.dockPanel_LOG;
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

        private void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (btn_Start.Caption == "一键开始")
            {
                btn_Start.Caption = "一键停止";
                btn_Start.ImageOptions.LargeImage = CKServer.Properties.Resources.Stop_btn;

                FileThread = new SaveFile();
                FileThread.FileInit();
                FileThread.FileSaveStart();

                foreach (DataRow dr in dt_OC.Rows)
                {
                    dr["脉宽"] = 0;
                }
                foreach (DataRow dr in dt_AD.Rows)
                {
                    dr["测量值"] = 0;
                }

                _BoxIsStarted = true;
                timer1.Enabled = true;

                for (int i = 0; i < 0x0f; i++)
                {
                    if (USB.MyDeviceList[i] != null)
                    {
                        CyControlEndPoint CtrlEndPt = null;
                        CtrlEndPt = USB.MyDeviceList[i].ControlEndPt;
                        if (CtrlEndPt != null)
                        {
                            USB.SendCMD(i, 0x80, 0x01);
                            USB.SendCMD(i, 0x80, 0x00);//复位

                            USB.MyDeviceList[i].Reset();

                            Thread.Sleep(100);

                            USB.SendCMD(i, 0x80, 0x04);//开启接收

                            Register.Byte80H = 0x04;

                            Thread.Sleep(100);
                        }
                        else
                        {
                            MyLog.Error(USB.MyDeviceList[i].Name + ":初始化失败!");
                        }
                    }
                }

                new Thread(() => { RecvAllUSB(); }).Start();


            }
            else
            {
                for (int i = 0; i < 0x0f; i++)
                {
                    if (USB.MyDeviceList[i] != null)
                    {
                        CyControlEndPoint CtrlEndPt = null;
                        CtrlEndPt = USB.MyDeviceList[i].ControlEndPt;
                        if (CtrlEndPt != null)
                        {
                            USB.SendCMD(i, 0x80, 0x00);//关闭接收
                        }
                    }
                }

                btn_Start.Caption = "一键开始";
                btn_Start.ImageOptions.LargeImage = CKServer.Properties.Resources.Start_btn;
                _BoxIsStarted = false;
                Thread.Sleep(200);

                if(FileThread!=null)
                FileThread.FileClose();

                timer1.Enabled = false;

            }
        }

        private void RecvAllUSB()
        {
            CyUSBDevice MyDevice01 = USB.MyDeviceList[Data.CardID1];

            CyUSBDevice MyDevice02 = USB.MyDeviceList[Data.CardID2];

            Trace.WriteLine("RecvAllUSB start!!!!");

            if (MyDevice01 != null)
            {
                ADList.Clear();
                new Thread(() => { DealWithADFun(); }).Start();
                MyLog.Info("CKUSB_CardID1 Runing... ...");
            }
            else
            {
                MyLog.Info("CKUSB_CardID1 Not Using... ...");
            }

            if(MyDevice02!=null)
            {
                AD_BIT1_List.Clear();
                AD_BIT2_List.Clear();
                new Thread(() => { DealWithADFun_BIT(); }).Start();
                MyLog.Info("CKUSB_CardID2 Runing... ...");
            }
            else
            {
                MyLog.Info("CKUSB_CardID2 Not Using... ...");
            }


            byte[] Recv_MidBuf_8K_Box01 = new byte[8192];//8K中间缓存
            int Pos_Recv_MidBuf_8K_Box01 = 0;//中间缓存数据存储到哪个位置

            byte[] Recv_MidBuf_8K_Box02 = new byte[8192];//8K中间缓存
            int Pos_Recv_MidBuf_8K_Box02 = 0;//中间缓存数据存储到哪个位置

            while (_BoxIsStarted)
            {
                try
                {
                    if (MyDevice01 != null)
                    {
                        if (MyDevice01.BulkInEndPt != null)
                        {
                            byte[] RecvBoxBuf = new byte[4096];
                            int RecvBoxLen = 4096;

                            lock (MyDevice01)
                                MyDevice01.BulkInEndPt.XferData(ref RecvBoxBuf, ref RecvBoxLen);//接收USB数据，不定长

                            if (RecvBoxLen > 0)
                            {
                                byte[] tempbuf = new byte[RecvBoxLen];
                                Array.Copy(RecvBoxBuf, tempbuf, RecvBoxLen);//实际收到数据量放到1个Temp数组中
                                                                            //存储源码
                                                                            //SaveFile.Lock_1.EnterWriteLock();
                                                                            //SaveFile.DataQueue_SC1.Enqueue(tempbuf);
                                                                            //SaveFile.Lock_1.ExitWriteLock();
                                                                            //将数据放到一个8K的数组中
                                Array.Copy(tempbuf, 0, Recv_MidBuf_8K_Box01, Pos_Recv_MidBuf_8K_Box01, tempbuf.Length);
                                Pos_Recv_MidBuf_8K_Box01 += tempbuf.Length;
                                //判断数据大于4K，就处理掉4K
                                while (Pos_Recv_MidBuf_8K_Box01 >= 4096)
                                {
                                    if (Recv_MidBuf_8K_Box01[0] == 0xff && (0x0 <= Recv_MidBuf_8K_Box01[1]) && (Recv_MidBuf_8K_Box01[1] < 0x21))
                                    {
                                        DealWithLongFrame(ref Recv_MidBuf_8K_Box01, ref Pos_Recv_MidBuf_8K_Box01);
                                    }
                                    else
                                    {
                                        MyLog.Error("收到异常帧！");
                                        Array.Clear(Recv_MidBuf_8K_Box01, 0, Pos_Recv_MidBuf_8K_Box01);
                                        Pos_Recv_MidBuf_8K_Box01 = 0;
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
                    if (MyDevice02 != null)
                    {
                        if (MyDevice02.BulkInEndPt != null)
                        {
                            byte[] RecvBoxBuf = new byte[4096];
                            int RecvBoxLen = 4096;

                            lock (MyDevice02)
                                MyDevice02.BulkInEndPt.XferData(ref RecvBoxBuf, ref RecvBoxLen);//接收USB数据，不定长

                            if (RecvBoxLen > 0)
                            {
                                byte[] tempbuf = new byte[RecvBoxLen];
                                Array.Copy(RecvBoxBuf, tempbuf, RecvBoxLen);//实际收到数据量放到1个Temp数组中
                                                                            //存储源码
                                                                            //SaveFile.Lock_1.EnterWriteLock();
                                                                            //SaveFile.DataQueue_SC1.Enqueue(tempbuf);
                                                                            //SaveFile.Lock_1.ExitWriteLock();
                                                                            //将数据放到一个8K的数组中
                                Array.Copy(tempbuf, 0, Recv_MidBuf_8K_Box02, Pos_Recv_MidBuf_8K_Box02, tempbuf.Length);
                                Pos_Recv_MidBuf_8K_Box02 += tempbuf.Length;
                                //判断数据大于4K，就处理掉4K
                                while (Pos_Recv_MidBuf_8K_Box02 >= 4096)
                                {
                                    if (Recv_MidBuf_8K_Box02[0] == 0xff && (0x0 <= Recv_MidBuf_8K_Box02[1]) && (Recv_MidBuf_8K_Box02[1] < 0x21))
                                    {
                                        DealWith_BIT_Frame(ref Recv_MidBuf_8K_Box02, ref Pos_Recv_MidBuf_8K_Box02);
                                    }
                                    else
                                    {
                                        MyLog.Error("收到异常帧！");
                                        Array.Clear(Recv_MidBuf_8K_Box02, 0, Pos_Recv_MidBuf_8K_Box02);
                                        Pos_Recv_MidBuf_8K_Box02 = 0;
                                    }
                                }
                            }
                            else if (RecvBoxLen == 0)
                            {
                                //  Trace.WriteLine("收到0包-----0000000000");
                            }
                            else
                            {
                                Trace.WriteLine("USBBox22接收数据异常，居然收到了小于0的数！！");
                                //MyLog.Error("USB接收数据异常，居然收到了小于0的数！！");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MyLog.Error(ex.Message);
                }


            }
        }

        List<byte> ADList = new List<byte>();
        double[] dataRe_AD = new double[Func_AD.ADNums];
        private void DealWithADFun()
        {
            // 获得AD校准参数
            double[] md = new double[64];
            for (int j = 0; j < 64; j++) md[j] = double.Parse(Function.GetConfigStr(Data.ADconfigPath, "add", "AD_Channel_" + j.ToString(), "value"));

            while (_BoxIsStarted)
            {
                bool ret = Func_AD.Return_ADValue(ref ADList, ref dataRe_AD, md);
                if (!ret) Thread.Sleep(10);
            }
        }

        List<Byte> AD_BIT1_List = new List<byte>();
        List<Byte> AD_BIT2_List = new List<byte>();
        double[] AD_BIT1_L1 = new double[Func_AD2_BIT.ADNums1];
        double[] AD_BIT1_L2 = new double[Func_AD2_BIT.ADNums1];
        double[] AD_BIT1_L3 = new double[Func_AD2_BIT.ADNums1];
        double[] AD_BIT1_L4 = new double[Func_AD2_BIT.ADNums1];
        double[] AD_BIT2_L1 = new double[Func_AD2_BIT.ADNums1];
        double[] AD_BIT2_L2 = new double[Func_AD2_BIT.ADNums1];
        double[] AD_BIT2_L3 = new double[Func_AD2_BIT.ADNums1];
        double[] AD_BIT2_L4 = new double[Func_AD2_BIT.ADNums1];

        private void DealWithADFun_BIT()
        {
            while (_BoxIsStarted)
            {
                bool ret1 = Func_AD2_BIT.Return_ADValue(ref AD_BIT1_List, ref AD_BIT1_L1, ref AD_BIT1_L2, ref AD_BIT1_L3, ref AD_BIT1_L4);
                bool ret2 = Func_AD2_BIT.Return_ADValue(ref AD_BIT2_List, ref AD_BIT2_L1, ref AD_BIT2_L2, ref AD_BIT2_L3, ref AD_BIT2_L4);
                if (ret1==false && ret2==false) Thread.Sleep(500);
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
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
            }
            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x01)
            {

                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
            }
            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x08)
            {
                //FF08为短帧通道
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                //SaveFile.Lock_2.EnterWriteLock();
                //SaveFile.DataQueue_SC2.Enqueue(bufsav);
                //SaveFile.Lock_2.ExitWriteLock();

                for (int i = 0; i < 6; i++)
                {
                    if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x00)//1D00：AD数据
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        //SaveFile.Lock_3.EnterWriteLock();
                        //SaveFile.DataQueue_SC3.Enqueue(buf1D0x);
                        //SaveFile.Lock_3.ExitWriteLock();

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

                    }
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x08)//1D08:AD数据
                    {

                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);

                    }
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

        int ThisCount_BIT = 0;
        int LastCount_BIT = 0;
        void DealWith_BIT_Frame(ref byte[] TempBuf, ref int TempTag)
        {
            ThisCount_BIT = TempBuf[2] * 256 + TempBuf[3];
            if (LastCount_BIT != 0 && ThisCount_BIT != 0 && (ThisCount_BIT - LastCount_BIT != 1))
            {
                MyLog.Error("出现漏帧情况！！");
                Trace.WriteLine("出现漏帧情况:" + LastCount_BIT.ToString("x4") + "--" + ThisCount_BIT.ToString("x4"));
            }
            LastCount_BIT = ThisCount_BIT;

            byte[] buf_LongFrame = new byte[4096];
            Array.Copy(TempBuf, 0, buf_LongFrame, 0, 4096);

            Array.Copy(TempBuf, 4096, TempBuf, 0, TempTag - 4096);
            TempTag -= 4096;

            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x08)
            {
                //FF08为短帧通道
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                //SaveFile.Lock_2.EnterWriteLock();
                //SaveFile.DataQueue_SC2.Enqueue(bufsav);
                //SaveFile.Lock_2.ExitWriteLock();

                for (int i = 0; i < 6; i++)
                {
                    if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x00)//1D00：AD数据
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        SaveFile.Lock_3.EnterWriteLock();
                        SaveFile.DataQueue_SC3.Enqueue(buf1D0x);
                        SaveFile.Lock_3.ExitWriteLock();
                        lock (AD_BIT1_List)
                            AD_BIT1_List.AddRange(buf1D0x);

                    }
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x01)//1D01:第2路422
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        SaveFile.Lock_4.EnterWriteLock();
                        SaveFile.DataQueue_SC4.Enqueue(buf1D0x);
                        SaveFile.Lock_4.ExitWriteLock();
                        lock (AD_BIT2_List)
                            AD_BIT2_List.AddRange(buf1D0x);
                    }
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
            else
            {
                //出现非预测帧
            }
        }

        private void dataGridView5_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (e.ColumnIndex == 0)//button列
                {
                    byte addr = (byte)(0x84 + e.RowIndex / 7);
                    byte value = (byte)(e.RowIndex % 7);

                    //边沿出发，发送一次脉冲
                    USB.SendCMD(Data.CardID1, addr, (byte)(0x1 << value));
                    USB.SendCMD(Data.CardID1, addr, 0x0);

                    MyLog.Info("指令输出:" + Func_OC.dt_OC_Out.Rows[e.RowIndex]["名称"]);

                    dataGridView5.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.SelectionBackColor = Color.Lime;

                }
            }
        }

        private void dockPanel_Rotator_Click(object sender, EventArgs e)
        {

        }

        private void btn_HelpNeed_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            MessageBox.Show(
                "Designed by 测控通信室 2018.\r\n" +
                "硬件支持--伊鹏&黄禹(测控通信室)\r\n" +
                "软件支持--平佳伟(测控通信室)\r\n"
                );
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            for (int i = 0; i < 0x0f; i++)
            {
                if (USB.MyDeviceList[i] != null)
                {
                    CyControlEndPoint CtrlEndPt = null;
                    CtrlEndPt = USB.MyDeviceList[i].ControlEndPt;
                    if (CtrlEndPt != null)
                    {
                        USB.SendCMD(i, 0x80, 0x00);//关闭接收
                    }
                }
            }

            btn_Start.Caption = "一键开始";
            btn_Start.ImageOptions.LargeImage = CKServer.Properties.Resources.Start_btn;
            _BoxIsStarted = false;
            Thread.Sleep(200);

            if(FileThread!=null)
            FileThread.FileClose();

            timer1.Enabled = false;
            timer2.Enabled = false;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (_BoxIsStarted)
            {

                USB.SendCMD(1, 0x81, 0x1, true);
                USB.SendCMD(1, 0x81, 0x0, true);

                Trace.WriteLine("进入Timer2_Tick自测  USB.SendCMD(1, 0x81, 0x1, true);USB.SendCMD(1, 0x81, 0x0, true)");
            }
            else
            {
                Trace.WriteLine("进入Timer2_Tick自测 but _BoxIsStarted==false");
            }
        }

        private void btn_Enable_BitTimer2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if(btn_Enable_BitTimer2.Caption == "开始自测")
            {
                btn_Enable_BitTimer2.Caption = "停止自测";
                btn_Enable_BitTimer2.ImageOptions.LargeImage = Properties.Resources.remove_32x32;
                timer2.Enabled = true;
            }
            else
            {
                btn_Enable_BitTimer2.Caption = "开始自测";
                btn_Enable_BitTimer2.ImageOptions.LargeImage = Properties.Resources.refresh_32x32;
                timer2.Enabled = false;
            }
        }
    }
}
