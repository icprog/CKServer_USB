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
            InitDataTable();
            Function.Init();//初始化DA参数

        }

        private void InitDataTable()
        {
            try
            {
                dt_AD.Columns.Add("序号", typeof(Int32));
                dt_AD.Columns.Add("名称", typeof(String));
                dt_AD.Columns.Add("测量值", typeof(double));
                for (int i = 0; i < 6; i++)
                {
                    DataRow dr = dt_AD.NewRow();
                    dr["序号"] = i + 1;
                    dr["名称"] = ConfigurationManager.AppSettings["AD_Channel_" + i.ToString()];
                    dr["测量值"] = 0;
                    dt_AD.Rows.Add(dr);
                }
                dataGridView4.DataSource = dt_AD;
                dataGridView4.AllowUserToAddRows = false;

                dt_OC.Columns.Add("序号", typeof(Int32));
                dt_OC.Columns.Add("名称", typeof(String));
                dt_OC.Columns.Add("脉宽", typeof(int));

                for (int i = 0; i < 10; i++)
                {
                    DataRow dr = dt_OC.NewRow();
                    dr["序号"] = i;
                    dr["名称"] = ConfigurationManager.AppSettings["OC_Channel_" + i.ToString()]; ;
                    dr["脉宽"] = 0;
                    dt_OC.Rows.Add(dr);
                }
                dataGridView5.DataSource = dt_OC;
                dataGridView5.AllowUserToAddRows = false;

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
                USB.SendCMD(Data.OCid, addr2, b1);
                USB.SendCMD(Data.OCid, addr3, b2);
                USB.SendCMD(Data.OCid, addr4, b3);
                //边沿出发，发送一次脉冲
                USB.SendCMD(Data.OCid, addr1, 0x1);
                USB.SendCMD(Data.OCid, addr1, 0x0);
                Thread.Sleep(10);
            }

        }

        private void btn_ResetDA_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            foreach (DataRow dr in dt_OC.Rows)
            {
                dr["脉宽"] = 0;
            }
            foreach (DataRow dr in dt_AD.Rows)
            {
                dr["测量值"] = 0;
            }
        }

        private void btn_setall_EditValueChanged(object sender, EventArgs e)
        {
            foreach (DataRow dr in dt_OC.Rows)
            {
                dr["脉宽"] = btn_setall.EditValue;
            }

        }

        private void btn_add_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            foreach (DataRow dr in dt_OC.Rows)
            {
                if ((int)dr["脉宽"] < 200)
                    dr["脉宽"] = (int)dr["脉宽"] + 1;
            }

        }

        private void btn_dec_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            foreach (DataRow dr in dt_OC.Rows)
            {
                if ((int)dr["脉宽"] >= 0)
                    dr["脉宽"] = (int)dr["脉宽"] - 1;
            }

        }


        private void dataGridView5_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (e.ColumnIndex == 2)
                {
                    try
                    {
                        double t = (double)dt_OC.Rows[e.RowIndex]["脉宽"];
                        if (t < 0 || t > 2000000)
                        {
                            dt_OC.Rows[e.RowIndex]["脉宽"] = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        MyLog.Error(ex.Message + "From:dataGridView5_CellEndEdit");
                        dt_OC.Rows[e.RowIndex]["脉宽"] = 0;
                    }
                }
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

            if (MySPort.Port1!=null)
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
            if(dockPanel6.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Visible)
            {
                dockPanel6.Show();
                MyLog.Info("显示转台控制界面！");
            }
            else
            {
                dockPanel6.Hide();
                MyLog.Info("隐藏转台控制界面！");
            }
            
        }

        private void btn_Modify_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            if (dockPanel3.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Visible)
            {
                dockPanel3.Show();
                MyLog.Info("显示422界面！");
            }
            else
            {
                dockPanel3.Hide();
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
            catch(Exception ex)
            {
                MyLog.Error(ex.Message);
            }

        }

        public void RunCruise(double startAngle,double stopAngle,byte speed,string direction)
        {
            while(true)
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

                if (TempAngle >= stopAngle && TempAngle <= (stopAngle+20))
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
                MySPort.AddOnAngle =0;
            }
        }
    }
}
