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

        System.Windows.Forms.CheckBox[] Channel_LVDSCheck;
        System.Windows.Forms.CheckBox[] Channel_422Check;
        DevExpress.XtraEditors.ButtonEdit[] Channel_422Editor;
        DevExpress.XtraEditors.ButtonEdit[] Channel_LVDSEditor;

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
            USBDevice RemoveDevice = evt.Device;
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
                        ComboBox_BoardSelect.Items.Add(key.ToString("x2") + ":" + USB.MyDeviceList[key].FriendlyName);
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

                ErrorLog.path = Program.GetStartupPath() + @"ErrorLog\";
                ErrorLog.start();


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

            Channel_LVDSCheck = new System.Windows.Forms.CheckBox[16] { checkBox_LVDS_select1,checkBox_LVDS_select2,checkBox_LVDS_select3,checkBox_LVDS_select4,checkBox_LVDS_select5, checkBox_LVDS_select6,checkBox_LVDS_select7,checkBox_LVDS_select8,
                checkBox_LVDS_select9, checkBox_LVDS_select10,checkBox_LVDS_select11,checkBox_LVDS_select12,checkBox_LVDS_select13, checkBox_LVDS_select14,checkBox_LVDS_select15,checkBox_LVDS_select16 };

            Channel_422Check = new System.Windows.Forms.CheckBox[8] { checkBox_422_select1, checkBox_422_select2,
                checkBox_422_select3, checkBox_422_select4, checkBox_422_select5, checkBox_422_select6, checkBox_422_select7, checkBox_422_select8 };

            Channel_422Editor = new DevExpress.XtraEditors.ButtonEdit[8] { buttonEdit_422_1, buttonEdit_422_2,
                buttonEdit_422_3,buttonEdit_422_4,buttonEdit_422_5,buttonEdit_422_6,buttonEdit_422_7,buttonEdit_422_8};

            Channel_LVDSEditor = new DevExpress.XtraEditors.ButtonEdit[16] { buttonEdit1, buttonEdit2,buttonEdit3,buttonEdit4,buttonEdit5,buttonEdit6,
                buttonEdit7,buttonEdit8,buttonEdit9,buttonEdit10,buttonEdit11,buttonEdit12,buttonEdit13,buttonEdit14,buttonEdit15,buttonEdit16};

            try
            {
                SetDevice(false);
                barStaticItem1.Caption = "存储路径" + Data.Path;
                InitDataTable();//初始化datadable
                Function.Init();//初始化DA参数      
                Data.init();

                Func_LVDS.Init();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("MainFrom_Load init：" + ex.Message);

            }

            #region LVDS初始加载上次保存参数
            try
            {
                barEditItem_lvdsfreqset.EditValue = ConfigurationManager.AppSettings["LVDS_Send_SetFreq"];
                barEditItem_lvdsfreqreal.EditValue = ConfigurationManager.AppSettings["LVDS_Send_RealFreq"];
                barEditItem_422freqset.EditValue = ConfigurationManager.AppSettings["422_Send_SetFreq"];
                barEditItem_422freqreal.EditValue = ConfigurationManager.AppSettings["422_Send_RealFreq"];


                barEditItem_lvds_KeepRun.EditValue = ConfigurationManager.AppSettings["LVDS_Set_AFlag"];
                barEditItem_lvds_KeepStop.EditValue = ConfigurationManager.AppSettings["LVDS_Set_BFlag"];
                barEditItem_422_KeepRun.EditValue = ConfigurationManager.AppSettings["422_Set_AFlag"];
                barEditItem_422_KeepStop.EditValue = ConfigurationManager.AppSettings["422_Set_BFlag"];

                for (int i = 0; i < 16; i++)
                {
                    Channel_LVDSEditor[i].Text = ConfigurationManager.AppSettings["PATH_DAT_" + (i + 1).ToString().PadLeft(2, '0')];
                }
                for (int i = 0; i < 8; i++)
                {
                    Channel_422Editor[i].Text = ConfigurationManager.AppSettings["PATH_422_DAT_" + (i + 1).ToString().PadLeft(2, '0')];
                }

                foreach (var item in Channel_LVDSCheck)
                {
                    string name = item.Name;
                    int key = int.Parse(name.Substring(20));

                    if (ConfigurationManager.AppSettings["checkBox_LVDS_select_" + key.ToString().PadLeft(2, '0')] == "True")
                    {
                        item.Checked = true;
                    }
                    else
                    {
                        item.Checked = false;
                    }
                }

                foreach (var item in Channel_422Check)
                {
                    string name = item.Name;
                    int key = int.Parse(name.Substring(19));
                    if (ConfigurationManager.AppSettings["checkBox_422_select_" + key.ToString().PadLeft(2, '0')] == "True")
                    {
                        item.Checked = true;
                    }
                    else
                    {
                        item.Checked = false;
                    }
                }

                int temp = int.Parse(ConfigurationManager.AppSettings["LVDS_ComPareChan"]);
                switch(temp)
                {
                    case 0:
                        radioButton1.Checked = true;
                        radioButton8_Click(radioButton1, e);
                        break;
                    case 1:
                        radioButton2.Checked = true;
                        radioButton8_Click(radioButton2, e);
                        break;
                    case 2:
                        radioButton3.Checked = true;
                        radioButton8_Click(radioButton3, e);
                        break;
                    case 3:
                        radioButton4.Checked = true;
                        radioButton8_Click(radioButton4, e);
                        break;
                    case 4:
                        radioButton5.Checked = true;
                        radioButton8_Click(radioButton5, e);
                        break;
                    case 5:
                        radioButton6.Checked = true;
                        radioButton8_Click(radioButton6, e);
                        break;
                    case 6:
                        radioButton7.Checked = true;
                        radioButton8_Click(radioButton7, e);
                        break;
                    case 7:
                        radioButton8.Checked = true;
                        radioButton8_Click(radioButton8, e);
                        break;
                    default:
                        radioButton1.Checked = true;
                        radioButton8_Click(radioButton1, e);
                        break;
                }


            }
            catch (Exception ex)
            {
                Trace.WriteLine("LVDS机箱自动载入上次配置 Failed：" + ex.Message);
            }
            #endregion


            #region 初始化一些破灯
            try
            {
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
            #endregion

            Func_RS422.textBox_Show422Reslt = this.textBox_RS422_Show;

        }


        private void InitDataTable()
        {
            try
            {
                Func_RS422.Init_Table();
                dataGridView_RS422_Send.DataSource = Func_RS422.dt_RS422_Send;
                dataGridView_RS422_Send.AllowUserToAddRows = false;

                Func_LVDS.Init_Table();
                dataGridView_LVDS_01.DataSource = Func_LVDS.dt_LVDS_01;
                dataGridView_LVDS_01.AllowUserToAddRows = false;

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
                Trace.WriteLine("Main InitDataTable：" + ex.Message);
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

            if (_BoxIsStarted)
            {
                for (int i = 0; i < Func_AD.ADNums; i++)
                {
                    Func_AD.dt_AD.Rows[i]["测量值"] = dataRe_AD[i];
                    if (i == 1) Func_AD.dt_AD.Rows[i]["测量值"] = 0;
                    if (i == 2) Func_AD.dt_AD.Rows[i]["测量值"] = 0;
                    if (i == 3) Func_AD.dt_AD.Rows[i]["测量值"] = 0;
                }
                //每1s更新一次OC数据显示
                for (int i = 0; i < 162; i++)
                {
                    if ((int)Func_OC.dt_OC_In1.Rows[i]["计数"] != dataRe_OC1[2 * i])
                    {
                        Func_OC.dt_OC_In1.Rows[i]["计数"] = dataRe_OC1[2 * i];
                        Func_OC.dt_OC_In1.Rows[i]["脉宽"] = dataRe_OC1[2 * i + 1];
                        dataGridView_OC_In1.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                    }
                    if ((int)Func_OC.dt_OC_In2.Rows[i]["计数"] != dataRe_OC2[2 * i])
                    {
                        Func_OC.dt_OC_In2.Rows[i]["计数"] = dataRe_OC2[2 * i];
                        Func_OC.dt_OC_In2.Rows[i]["脉宽"] = dataRe_OC2[2 * i + 1];
                        dataGridView_OC_In2.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                    }
                    if ((int)Func_OC.dt_OC_In3.Rows[i]["计数"] != dataRe_OC3[2 * i])
                    {
                        Func_OC.dt_OC_In3.Rows[i]["计数"] = dataRe_OC3[2 * i];
                        Func_OC.dt_OC_In3.Rows[i]["脉宽"] = dataRe_OC3[2 * i + 1];
                        dataGridView_OC_In3.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                    }
                }
                //每1s更新一次LVDS数据比对结果
                if (Func_LVDS.LVDS_ComPareTag)
                {

                    List<Func_LVDS.RecvChan_VCID_Struct> showVcid = new List<Func_LVDS.RecvChan_VCID_Struct>();
                    showVcid.Add(Func_LVDS.VCID_01);
                    showVcid.Add(Func_LVDS.VCID_02);
                    showVcid.Add(Func_LVDS.VCID_03);
                    showVcid.Add(Func_LVDS.VCID_04);
                    showVcid.Add(Func_LVDS.VCID_05);
                    showVcid.Add(Func_LVDS.VCID_06);
                    showVcid.Add(Func_LVDS.VCID_07);
                    showVcid.Add(Func_LVDS.VCID_08);
                    showVcid.Add(Func_LVDS.VCID_09);
                    showVcid.Add(Func_LVDS.VCID_10);
                    showVcid.Add(Func_LVDS.VCID_11);
                    showVcid.Add(Func_LVDS.VCID_12);
                    showVcid.Add(Func_LVDS.VCID_13);
                    showVcid.Add(Func_LVDS.VCID_14);
                    showVcid.Add(Func_LVDS.VCID_15);
                    showVcid.Add(Func_LVDS.VCID_16);


                    for (int j=0;j<16;j++)
                    {
                        Func_LVDS.dt_LVDS_01.Rows[2*j]["收到数据"] = showVcid[j].Real_RecvCounts;
                        Func_LVDS.dt_LVDS_01.Rows[2*j]["出错行"] = showVcid[j].Real_ErrorRow;
                        Func_LVDS.dt_LVDS_01.Rows[2*j]["出错列"] = showVcid[j].Real_ErrorCol;


                        Func_LVDS.dt_LVDS_01.Rows[2*j+1]["收到数据"] = showVcid[j].Delay_RecvCounts;
                        Func_LVDS.dt_LVDS_01.Rows[2*j+1]["出错行"] = showVcid[j].Delay_ErrorRow;
                        Func_LVDS.dt_LVDS_01.Rows[2*j+1]["出错列"] = showVcid[j].Delay_ErrorCol;
                    }

                    //Func_LVDS.dt_LVDS_01.Rows[0]["收到数据"] = Func_LVDS.VCID_01.Real_RecvCounts;
                    //Func_LVDS.dt_LVDS_01.Rows[0]["出错行"] = Func_LVDS.VCID_01.Real_ErrorRow;
                    //Func_LVDS.dt_LVDS_01.Rows[0]["出错列"] = Func_LVDS.VCID_01.Real_ErrorCol;


                    //Func_LVDS.dt_LVDS_01.Rows[0]["收到数据"] = Func_LVDS.VCID_01.Real_RecvCounts;
                    //Func_LVDS.dt_LVDS_01.Rows[0]["出错行"] = Func_LVDS.VCID_01.Real_ErrorRow;
                    //Func_LVDS.dt_LVDS_01.Rows[0]["出错列"] = Func_LVDS.VCID_01.Real_ErrorCol;

                    //Func_LVDS.dt_LVDS_01.Rows[1]["收到数据"] = Func_LVDS.VCID_01.Delay_RecvCounts;
                    //Func_LVDS.dt_LVDS_01.Rows[1]["出错行"] = Func_LVDS.VCID_01.Delay_ErrorRow;
                    //Func_LVDS.dt_LVDS_01.Rows[1]["出错列"] = Func_LVDS.VCID_01.Delay_ErrorCol;


                    //Func_LVDS.dt_LVDS_01.Rows[2]["收到数据"] = Func_LVDS.VCID_02.Real_RecvCounts;
                    //Func_LVDS.dt_LVDS_01.Rows[2]["出错行"] = Func_LVDS.VCID_02.Real_ErrorRow;
                    //Func_LVDS.dt_LVDS_01.Rows[2]["出错列"] = Func_LVDS.VCID_02.Real_ErrorCol;

                    //Func_LVDS.dt_LVDS_01.Rows[3]["收到数据"] = Func_LVDS.VCID_02.Delay_RecvCounts;
                    //Func_LVDS.dt_LVDS_01.Rows[3]["出错行"] = Func_LVDS.VCID_02.Delay_ErrorRow;
                    //Func_LVDS.dt_LVDS_01.Rows[3]["出错列"] = Func_LVDS.VCID_02.Delay_ErrorCol;

                    //Func_LVDS.dt_LVDS_01.Rows[4]["收到数据"] = Func_LVDS.VCID_03.Real_RecvCounts;
                    //Func_LVDS.dt_LVDS_01.Rows[4]["出错行"] = Func_LVDS.VCID_03.Real_ErrorRow;
                    //Func_LVDS.dt_LVDS_01.Rows[4]["出错列"] = Func_LVDS.VCID_03.Real_ErrorCol;

                    //Func_LVDS.dt_LVDS_01.Rows[5]["收到数据"] = Func_LVDS.VCID_03.Delay_RecvCounts;
                    //Func_LVDS.dt_LVDS_01.Rows[5]["出错行"] = Func_LVDS.VCID_03.Delay_ErrorRow;
                    //Func_LVDS.dt_LVDS_01.Rows[5]["出错列"] = Func_LVDS.VCID_03.Delay_ErrorCol;

                    //Func_LVDS.dt_LVDS_01.Rows[6]["收到数据"] = Func_LVDS.VCID_04.Real_RecvCounts;
                    //Func_LVDS.dt_LVDS_01.Rows[6]["出错行"] = Func_LVDS.VCID_04.Real_ErrorRow;
                    //Func_LVDS.dt_LVDS_01.Rows[6]["出错列"] = Func_LVDS.VCID_04.Real_ErrorCol;

                    //Func_LVDS.dt_LVDS_01.Rows[7]["收到数据"] = Func_LVDS.VCID_05.Delay_RecvCounts;
                    //Func_LVDS.dt_LVDS_01.Rows[7]["出错行"] = Func_LVDS.VCID_05.Delay_ErrorRow;
                    //Func_LVDS.dt_LVDS_01.Rows[7]["出错列"] = Func_LVDS.VCID_05.Delay_ErrorCol;



                }
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
        }

        private void btn_Modify_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            if (dockPanel_BoxLVDS_A.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Visible)
            {
                dockPanel_BoxLVDS_A.Show();
                MyLog.Info("显示422界面！");
            }
            else
            {
                dockPanel_BoxLVDS_A.Hide();
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

                FileThread = new SaveFile();
                FileThread.FileInit();
                FileThread.FileSaveStart();

                _BoxIsStarted = true;


                for (int i = 0; i < 0x0f; i++)
                {
                    if (USB.MyDeviceList[i] != null)
                    {
                        CyControlEndPoint CtrlEndPt = null;
                        CtrlEndPt = USB.MyDeviceList[i].ControlEndPt;
                        if (CtrlEndPt != null)
                        {
                            if (i == Data.LVDSid)
                            {
                                String tbstr = btn_TbUsedSelct.EditValue.ToString();
                                if (tbstr == "同步模式")
                                {
                                    USB.SendCMD(Data.LVDSid, 0x8e, 0x06);
                                }
                                else
                                {
                                    USB.SendCMD(Data.LVDSid, 0x8e, 0x02);
                                }
                            }


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

                FileThread.FileClose();

                Func_LVDS.Stop();

            }
        }

        private void RecvAllUSB()
        {
            CyUSBDevice MyDevice01 = USB.MyDeviceList[Data.DAid];
            CyUSBDevice MyDevice02 = USB.MyDeviceList[Data.OCid];
            CyUSBDevice MyDevice03 = USB.MyDeviceList[Data.SC422id];
            CyUSBDevice MyDevice04 = USB.MyDeviceList[Data.LVDSid];
            bool MyDevice01_Enable = false;
            bool MyDevice02_Enable = false;
            bool MyDevice03_Enable = false;
            bool MyDevice04_Enable = false;

            int RunNums_DA = 1;
            int RunNums_OC = 2;
            int RunNums_SC422 = 5;
            int RunNums_LVDS = 20;

            int RunCounts = 0;

            Trace.WriteLine("RecvAllUSB start!!!!");

            if (MyDevice01 != null)
            {
                MyDevice01_Enable = true;
                ADList.Clear();
                new Thread(() => { DealWithADFun(); }).Start();
            }

            if (MyDevice02 != null)
            {
                MyDevice02_Enable = true;
                OCList1.Clear();
                OCList2.Clear();
                OCList3.Clear();

                foreach (DataRow dr in Func_OC.dt_OC_In1.Rows)
                {
                    dr["计数"] = 0;
                    dr["脉宽"] = 0;
                }
                foreach (DataRow dr in Func_OC.dt_OC_In2.Rows)
                {
                    dr["计数"] = 0;
                    dr["脉宽"] = 0;
                }
                foreach (DataRow dr in Func_OC.dt_OC_In3.Rows)
                {
                    dr["计数"] = 0;
                    dr["脉宽"] = 0;
                }

                for (int i = 0; i < 324; i++)
                {
                    dataRe_OC1[i] = 0;
                    dataRe_OC2[i] = 0;
                    dataRe_OC3[i] = 0;
                }

                new Thread(() => { DealWithOCFun(); }).Start();

            }
            if (MyDevice03 != null)
                MyDevice03_Enable = true;

            if (MyDevice04 != null)//LVDS
            {
                MyDevice04_Enable = true;
                Func_LVDS.Run();//初始化LVDS比对各项参数
                                //            new Thread(() => { RealTime_ComPare2(); }).Start();
            }

            byte[] Recv_MidBuf_8K_Box01 = new byte[8192];//8K中间缓存
            int Pos_Recv_MidBuf_8K_Box01 = 0;//中间缓存数据存储到哪个位置

            byte[] Recv_MidBuf_8K_Box02 = new byte[8192];//8K中间缓存
            int Pos_Recv_MidBuf_8K_Box02 = 0;//中间缓存数据存储到哪个位置

            byte[] Recv_MidBuf_8K_Box03 = new byte[8192];//8K中间缓存
            int Pos_Recv_MidBuf_8K_Box03 = 0;//中间缓存数据存储到哪个位置

            byte[] Recv_MidBuf_8K_Box04 = new byte[8192];//8K中间缓存
            int Pos_Recv_MidBuf_8K_Box04 = 0;//中间缓存数据存储到哪个位置
            while (_BoxIsStarted)
            {
                if (MyDevice01_Enable && RunCounts < RunNums_DA)
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
                            SaveFile.Lock_1.EnterWriteLock();
                            SaveFile.DataQueue_SC1.Enqueue(tempbuf);
                            SaveFile.Lock_1.ExitWriteLock();
                            //将数据放到一个8K的数组中
                            Array.Copy(tempbuf, 0, Recv_MidBuf_8K_Box01, Pos_Recv_MidBuf_8K_Box01, tempbuf.Length);
                            Pos_Recv_MidBuf_8K_Box01 += tempbuf.Length;
                            //判断数据大于4K，就处理掉4K
                            while (Pos_Recv_MidBuf_8K_Box01 >= 4096)
                            {
                                if (Recv_MidBuf_8K_Box01[0] == 0xff && (0x0 <= Recv_MidBuf_8K_Box01[1]) && (Recv_MidBuf_8K_Box01[1] < 0x11))
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

                if (MyDevice02_Enable && RunCounts >= RunNums_DA && RunCounts < RunNums_OC)
                {
                    if (MyDevice02.BulkInEndPt != null)
                    {
                        byte[] RecvBoxBuf = new byte[4096];
                        int RecvBoxLen = 4096;
                        lock (MyDevice02)
                            MyDevice02.BulkInEndPt.XferData(ref RecvBoxBuf, ref RecvBoxLen);

                        if (RecvBoxLen > 0)
                        {
                            byte[] tempbuf = new byte[RecvBoxLen];
                            Array.Copy(RecvBoxBuf, tempbuf, RecvBoxLen);

                            //存储源码
                            SaveFile.Lock_10.EnterWriteLock();
                            SaveFile.DataQueue_SC10.Enqueue(tempbuf);
                            SaveFile.Lock_10.ExitWriteLock();

                            lock (Recv_MidBuf_8K_Box02)
                                Array.Copy(tempbuf, 0, Recv_MidBuf_8K_Box02, Pos_Recv_MidBuf_8K_Box02, tempbuf.Length);

                            Pos_Recv_MidBuf_8K_Box02 += tempbuf.Length;

                            while (Pos_Recv_MidBuf_8K_Box02 >= 4096)
                            {
                                if (Recv_MidBuf_8K_Box02[0] == 0xff && Recv_MidBuf_8K_Box02[1] == 0x08)
                                {
                                    DealWith_OC_Frame(ref Recv_MidBuf_8K_Box02, ref Pos_Recv_MidBuf_8K_Box02);
                                }
                                else
                                {
                                    MyLog.Error("OC收到异常帧:" + Recv_MidBuf_8K_Box02[0].ToString("x2") + Recv_MidBuf_8K_Box02[1].ToString("x2"));
                                    Array.Clear(Recv_MidBuf_8K_Box02, 0, Pos_Recv_MidBuf_8K_Box02);
                                    Pos_Recv_MidBuf_8K_Box02 = 0;
                                }
                            }
                        }
                        else if (RecvBoxLen == 0)
                        {
                            //  Trace.WriteLine("OC机箱收到0包-----0000000000");
                        }
                        else
                        {
                            Trace.WriteLine("OC机箱接收数据异常，居然收到了小于0的数！！");
                            //MyLog.Error("USB接收数据异常，居然收到了小于0的数！！");
                        }
                    }
                }

                //422机箱接收
                if (MyDevice03_Enable && RunCounts >= RunNums_OC && RunCounts < RunNums_SC422)
                {
                    if (MyDevice03.BulkInEndPt != null)
                    {

                        byte[] RecvBoxBuf = new byte[4096];
                        int RecvBoxLen = 4096;

                        lock (MyDevice03)
                            MyDevice03.BulkInEndPt.XferData(ref RecvBoxBuf, ref RecvBoxLen);

                        if (RecvBoxLen > 0)
                        {
                            byte[] tempbuf = new byte[RecvBoxLen];
                            Array.Copy(RecvBoxBuf, tempbuf, RecvBoxLen);
                            //存储源码

                            SaveFile.Lock_20.EnterWriteLock();
                            SaveFile.DataQueue_SC20.Enqueue(tempbuf);
                            SaveFile.Lock_20.ExitWriteLock();

                            Array.Copy(tempbuf, 0, Recv_MidBuf_8K_Box03, Pos_Recv_MidBuf_8K_Box03, tempbuf.Length);
                            Pos_Recv_MidBuf_8K_Box03 += tempbuf.Length;

                            while (Pos_Recv_MidBuf_8K_Box03 >= 4096)
                            {
                                if (Recv_MidBuf_8K_Box03[0] == 0xff)
                                {
                                    DealWith_RS422_Frame(ref Recv_MidBuf_8K_Box03, ref Pos_Recv_MidBuf_8K_Box03);
                                }
                                else
                                {
                                    MyLog.Error("422机箱 收到异常帧！");
                                    Array.Clear(Recv_MidBuf_8K_Box03, 0, Pos_Recv_MidBuf_8K_Box03);
                                    Pos_Recv_MidBuf_8K_Box03 = 0;
                                }
                            }
                        }
                        else if (RecvBoxLen == 0)
                        {
                            //    Trace.WriteLine("422机箱收到0包-----0000000000");
                        }
                        else
                        {
                            Trace.WriteLine("422机箱接收数据异常，居然收到了小于0的数！！");
                        }
                    }
                }

                if (MyDevice04_Enable && RunCounts >= RunNums_SC422 && RunCounts < RunNums_LVDS)
                {
                    if (MyDevice04.BulkInEndPt != null)
                    {
                        byte[] RecvBoxBuf = new byte[4096];
                        int RecvBoxLen = 4096;

                        lock (MyDevice04)
                            MyDevice04.BulkInEndPt.XferData(ref RecvBoxBuf, ref RecvBoxLen);

                        if (RecvBoxLen > 0)
                        {
                            byte[] tempbuf = new byte[RecvBoxLen];
                            Array.Copy(RecvBoxBuf, tempbuf, RecvBoxLen);
                            //存储源码
                            //SaveFile.Lock_19.EnterWriteLock();
                            //SaveFile.DataQueue_SC19.Enqueue(tempbuf);
                            //SaveFile.Lock_19.ExitWriteLock();

                            Array.Copy(tempbuf, 0, Recv_MidBuf_8K_Box04, Pos_Recv_MidBuf_8K_Box04, tempbuf.Length);
                            Pos_Recv_MidBuf_8K_Box04 += tempbuf.Length;

                            while (Pos_Recv_MidBuf_8K_Box04 >= 4096)
                            {
                                if (Recv_MidBuf_8K_Box04[0] == 0xff && (0x0 <= Recv_MidBuf_8K_Box04[1]) && (Recv_MidBuf_8K_Box04[1] < 0x11))
                                {
                                    DealWith_LVDS_Frame(ref Recv_MidBuf_8K_Box04, ref Pos_Recv_MidBuf_8K_Box04);
                                }
                                else
                                {
                                    MyLog.Error("收到异常帧！");
                                    Array.Clear(Recv_MidBuf_8K_Box04, 0, Pos_Recv_MidBuf_8K_Box04);
                                    Pos_Recv_MidBuf_8K_Box04 = 0;
                                }
                            }
                        }
                        else if (RecvBoxLen == 0)
                        {
                            //    Trace.WriteLine("LVDS机箱收到0包-----0000000000");
                        }
                        else
                        {
                            Trace.WriteLine("LVDS机箱接收数据异常，居然收到了小于0的数！！");
                        }
                    }
                }

                RunCounts++;
                if (RunCounts > RunNums_LVDS)
                    RunCounts = 0;
            }
        }

        int ThisCount_OC = 0;
        int LastCount_OC = 0;
        void DealWith_OC_Frame(ref byte[] TempBuf, ref int TempTag)
        {
            ThisCount_OC = TempBuf[2] * 256 + TempBuf[3];
            if (LastCount_OC != 0 && ThisCount_OC != 0 && (ThisCount_OC - LastCount_OC != 1))
            {
                MyLog.Error("出现漏帧情况！！");
                Trace.WriteLine("出现漏帧情况:" + LastCount_OC.ToString("x4") + "--" + ThisCount_OC.ToString("x4"));
            }
            LastCount_OC = ThisCount_OC;

            byte[] buf_LongFrame = new byte[4096];
            Array.Copy(TempBuf, 0, buf_LongFrame, 0, 4096);

            lock (TempBuf)
                Array.Copy(TempBuf, 4096, TempBuf, 0, TempTag - 4096);

            TempTag -= 4096;

            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x08)
            {
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);

                for (int i = 0; i < 6; i++)
                {
                    if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x00)//1D00：第1路OC
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        lock (OCList1)//将OC数据放入OCList,在另一个线程中解析
                        {
                            for (int j = 0; j < buf1D0x.Length; j++) OCList1.Add(buf1D0x[j]);
                        }
                    }
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x01)//1D01:第2路OC
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        lock (OCList2)//将OC数据放入OCList,在另一个线程中解析
                        {
                            for (int j = 0; j < buf1D0x.Length; j++) OCList2.Add(buf1D0x[j]);
                        }

                    }
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x02)//1D02:第3路OC
                    {
                        Data.Status_1D08.ChanActive = true;

                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);

                        lock (OCList3)//将OC数据放入OCList,在另一个线程中解析
                        {
                            for (int j = 0; j < buf1D0x.Length; j++) OCList3.Add(buf1D0x[j]);
                        }
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

        int ThisCount_RS422 = 0;
        int LastCount_RS422 = 0;
        void DealWith_RS422_Frame(ref byte[] TempBuf, ref int TempTag)
        {
            ThisCount_RS422 = TempBuf[2] * 256 + TempBuf[3];
            if (LastCount_RS422 != 0 && ThisCount_RS422 != 0 && (ThisCount_RS422 - LastCount_RS422 != 1))
            {
                MyLog.Error("出现漏帧情况！！");
                Trace.WriteLine("出现漏帧情况:" + LastCount_RS422.ToString("x4") + "--" + ThisCount_RS422.ToString("x4"));
            }
            LastCount_RS422 = ThisCount_RS422;

            byte[] buf_LongFrame = new byte[4096];
            Array.Copy(TempBuf, 0, buf_LongFrame, 0, 4096);

            Array.Copy(TempBuf, 4096, TempBuf, 0, TempTag - 4096);
            TempTag -= 4096;

            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x05)
            {
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                for (int i = 0; i < 6; i++)
                {
                    if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] <= 0x10)
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);

                        int ChanNo = (buf_LongFrame[1] - 0x05) * 15 + bufsav[i * 682 + 1];

                        if (bufsav[i * 682 + 1] == 0x00)//1D00：第1路
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_21, ref SaveFile.DataQueue_SC21, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x01)//1D01:
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_22, ref SaveFile.DataQueue_SC22, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x02)//1D02
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_23, ref SaveFile.DataQueue_SC23, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x03)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_24, ref SaveFile.DataQueue_SC24, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x04)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_25, ref SaveFile.DataQueue_SC25, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x05)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_26, ref SaveFile.DataQueue_SC26, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x06)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_27, ref SaveFile.DataQueue_SC27, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x07)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_28, ref SaveFile.DataQueue_SC28, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x08)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_29, ref SaveFile.DataQueue_SC29, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x09)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_30, ref SaveFile.DataQueue_SC30, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x0A)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_31, ref SaveFile.DataQueue_SC31, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x0B)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_32, ref SaveFile.DataQueue_SC32, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x0C)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_33, ref SaveFile.DataQueue_SC33, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x0D)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_34, ref SaveFile.DataQueue_SC34, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x0E)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_35, ref SaveFile.DataQueue_SC35, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x0F)
                        {
                            //空闲帧
                        }
                        else
                        {
                            Trace.WriteLine("FF05通道出错!");
                        }
                    }
                }
            }
            else if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x06)
            {
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                for (int i = 0; i < 6; i++)
                {
                    if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] <= 0x10)
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);

                        int ChanNo = (buf_LongFrame[1] - 0x05) * 15 + bufsav[i * 682 + 1];

                        if (bufsav[i * 682 + 1] == 0x00)//1D00：第1路
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_36, ref SaveFile.DataQueue_SC36, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x01)//1D01:
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_37, ref SaveFile.DataQueue_SC37, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x02)//1D02
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_38, ref SaveFile.DataQueue_SC38, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x03)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_39, ref SaveFile.DataQueue_SC39, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x04)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_40, ref SaveFile.DataQueue_SC40, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x05)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_41, ref SaveFile.DataQueue_SC41, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x06)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_42, ref SaveFile.DataQueue_SC42, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x07)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_43, ref SaveFile.DataQueue_SC43, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x08)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_44, ref SaveFile.DataQueue_SC44, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x09)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_45, ref SaveFile.DataQueue_SC45, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x0A)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_46, ref SaveFile.DataQueue_SC46, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x0B)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_47, ref SaveFile.DataQueue_SC47, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x0C)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_48, ref SaveFile.DataQueue_SC48, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x0D)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_49, ref SaveFile.DataQueue_SC49, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x0E)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_50, ref SaveFile.DataQueue_SC50, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x0F)
                        {
                            //空闲帧
                        }
                        else
                        {
                            Trace.WriteLine("FF06通道出错!");
                        }
                    }
                }

            }
            else if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x07)
            {
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                for (int i = 0; i < 6; i++)
                {
                    if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] <= 0x10)
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        int ChanNo = (buf_LongFrame[1] - 0x05) * 15 + bufsav[i * 682 + 1];
                        if (bufsav[i * 682 + 1] == 0x00)//1D00：第1路
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_51, ref SaveFile.DataQueue_SC51, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x01)//1D01:
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_52, ref SaveFile.DataQueue_SC52, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x02)//1D02
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_53, ref SaveFile.DataQueue_SC53, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x03)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_54, ref SaveFile.DataQueue_SC54, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x04)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_55, ref SaveFile.DataQueue_SC55, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x05)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_56, ref SaveFile.DataQueue_SC56, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x06)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_57, ref SaveFile.DataQueue_SC57, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x07)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_58, ref SaveFile.DataQueue_SC58, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x08)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_59, ref SaveFile.DataQueue_SC59, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x09)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_60, ref SaveFile.DataQueue_SC60, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x0A)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_61, ref SaveFile.DataQueue_SC61, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x0B)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_62, ref SaveFile.DataQueue_SC62, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x0C)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_63, ref SaveFile.DataQueue_SC63, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x0D)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_64, ref SaveFile.DataQueue_SC64, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x0E)
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_65, ref SaveFile.DataQueue_SC65, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x0F)
                        {
                            //空闲帧
                        }
                        else
                        {
                            Trace.WriteLine("FF07通道出错!");
                        }
                    }
                }

            }
            else if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x08)
            {
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                for (int i = 0; i < 6; i++)
                {
                    if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] <= 0x10)
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        int ChanNo = (buf_LongFrame[1] - 0x05) * 15 + bufsav[i * 682 + 1];
                        if (bufsav[i * 682 + 1] == 0x00)//1D00：第1路
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_66, ref SaveFile.DataQueue_SC66, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x01)//1D01:
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_67, ref SaveFile.DataQueue_SC68, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x02)//1D02
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_68, ref SaveFile.DataQueue_SC68, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x03)//1D03----同步422第1路接收
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_69, ref SaveFile.DataQueue_SC69, buf1D0x, ChanNo);
                        }
                        else if (bufsav[i * 682 + 1] == 0x04)//1D04----同步422第2路接收
                        {
                            Func_RS422.SaveToFile(ref SaveFile.Lock_70, ref SaveFile.DataQueue_SC70, buf1D0x, ChanNo);
                        }

                        else if (bufsav[i * 682 + 1] == 0x0F)
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
            else
            {
                Trace.WriteLine("422机箱收到异常帧头！");
            }

        }



        int ThisCount_LVDS = 0;
        int LastCount_LVDS = 0;
        void DealWith_LVDS_Frame(ref byte[] TempBuf, ref int TempTag)
        {
            ThisCount_LVDS = TempBuf[2] * 256 + TempBuf[3];
            if (LastCount_LVDS != 0 && ThisCount_LVDS != 0 && (ThisCount_LVDS - LastCount_LVDS != 1))
            {
                MyLog.Error("出现漏帧情况！！");
                Trace.WriteLine("出现漏帧情况:" + LastCount_LVDS.ToString("x4") + "--" + ThisCount_LVDS.ToString("x4"));
            }
            LastCount_LVDS = ThisCount_LVDS;

            byte[] buf_LongFrame = new byte[4096];
            Array.Copy(TempBuf, 0, buf_LongFrame, 0, 4096);

            Array.Copy(TempBuf, 4096, TempBuf, 0, TempTag - 4096);
            TempTag -= 4096;

            byte[] bufsav = new byte[4092];
            Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);

            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x01)
            {
                SaveToFile(ref SaveFile.Lock_11, ref SaveFile.DataQueue_SC11, bufsav, 1);

            }
            else if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x02)
            {
                SaveToFile(ref SaveFile.Lock_12, ref SaveFile.DataQueue_SC12, bufsav, 2);
            }
            else if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x03)
            {
                SaveToFile(ref SaveFile.Lock_13, ref SaveFile.DataQueue_SC13, bufsav, 3);
            }
            else if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x04)
            {
                SaveToFile(ref SaveFile.Lock_14, ref SaveFile.DataQueue_SC14, bufsav, 4);
            }
            else if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x05)
            {
                SaveToFile(ref SaveFile.Lock_15, ref SaveFile.DataQueue_SC15, bufsav, 5);
            }
            else if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x06)
            {
                SaveToFile(ref SaveFile.Lock_16, ref SaveFile.DataQueue_SC16, bufsav, 6);
            }
            else if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x07)
            {
                SaveToFile(ref SaveFile.Lock_17, ref SaveFile.DataQueue_SC17, bufsav, 7);
            }
            else if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x08)
            {
                SaveToFile(ref SaveFile.Lock_18, ref SaveFile.DataQueue_SC18, bufsav, 8);
            }
            else
            {
                Trace.WriteLine("DealWith_LVDS_Frame 收到非法数据??");
            }
        }



        public void SaveToFile(ref ReaderWriterLockSlim rwlock, ref Queue<byte[]> queue, byte[] bufsav, int ChanNo)
        {
            RadioButton[] RdoList = new RadioButton[8] { radioButton1, radioButton2, radioButton3, radioButton4,
            radioButton5,radioButton6,radioButton7,radioButton8};

            rwlock.EnterWriteLock();
            queue.Enqueue(bufsav);
            rwlock.ExitWriteLock();

            if (Data.LVDSCPTAG)
            {
                try
                {
                    if (RdoList[ChanNo - 1].Checked)
                    {
                        Func_LVDS.DispatchLock.EnterWriteLock();
                        Func_LVDS.NeedDispatchBuf.AddRange(bufsav);
                        Func_LVDS.DispatchLock.ExitWriteLock();
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
            }
            
        }


        public void DealWithCADU(byte[] CADU, ref List<string> APIDList, string ChannelName,
            ref Dictionary<string, Queue<byte[]>> Apid_EPDU_Dictionary, ref Dictionary<string, BinaryWriter> myDictionary)
        {

            string Save_path = Program.GetStartupPath() + @"存储数据\LVDS机箱数据\" + ChannelName + @"\";
            if (!Directory.Exists(Save_path))
                Directory.CreateDirectory(Save_path);

            if (CADU.Length == 1024)
            {
                string currentApid = Convert.ToString(CADU[5], 2).PadLeft(8, '0');

                if (APIDList.IndexOf(currentApid) < 0)
                {
                    APIDList.Add(currentApid);
                    Queue<byte[]> myEPDUqueue = new Queue<byte[]>();

                    string timestr = string.Format("{0}-{1:D2}-{2:D2} {3:D2}：{4:D2}：{5:D2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                    string filename0 = Save_path + currentApid + "_" + timestr + ".dat";
                    FileStream ApidFile = new FileStream(filename0, FileMode.Create);
                    BinaryWriter bw1 = new BinaryWriter(ApidFile);

                    myDictionary.Add(currentApid, bw1);
                }

                if (myDictionary.ContainsKey(currentApid))
                {
                    myDictionary[currentApid].Write(CADU);
                    myDictionary[currentApid].Flush();
                }
                else
                {
                    Trace.WriteLine("未找到APID对应的File文件!!Error!!!");
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
                    }
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
        List<byte> OCList1 = new List<byte>();
        List<byte> OCList2 = new List<byte>();
        List<byte> OCList3 = new List<byte>();
        int[] dataRe_OC1 = new int[162 * 2];//162路 计数+脉宽
        int[] dataRe_OC2 = new int[162 * 2];
        int[] dataRe_OC3 = new int[162 * 2];
        private void DealWithOCFun()
        {
            while (_BoxIsStarted)
            {
                //bool ret1 = Func_OC.Return_OCValue(ref OCList1, ref dataRe_OC1);
                //bool ret2 = Func_OC.Return_OCValue(ref OCList2, ref dataRe_OC2);
                //bool ret3 = Func_OC.Return_OCValue(ref OCList3, ref dataRe_OC3);

                bool ret1 = Func_OC.Return_OCValue(ref OCList3, ref dataRe_OC1);
                bool ret2 = Func_OC.Return_OCValue(ref OCList2, ref dataRe_OC2);
                bool ret3 = Func_OC.Return_OCValue(ref OCList1, ref dataRe_OC3);

                if (ret1 == false && ret2 == false && ret3 == false)
                {
                    Thread.Sleep(500);
                }
            }
        }


        List<byte> ADList = new List<byte>();
        double[] dataRe_AD = new double[Func_AD.ADNums];
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

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            #region 关闭USB接收以及File存储
            try
            {
                Func_LVDS.Stop();

                _BoxIsStarted = false;
                Thread.Sleep(200);
                FileThread.FileClose();


            }
            catch (Exception ex)
            {

            }
            #endregion

            #region 记录界面选项，下次重启自动加载
            try
            {
                foreach (var item in Channel_LVDSCheck)
                {
                    string name = item.Name;
                    int key = int.Parse(name.Substring(20));
                    if (item.Checked)
                    {
                        Function.SetConfigValue("checkBox_LVDS_select_" + key.ToString().PadLeft(2, '0'), "True");
                    }
                    else
                    {
                        Function.SetConfigValue("checkBox_LVDS_select_" + key.ToString().PadLeft(2, '0'), "False");
                    }
                }

                foreach (var item in Channel_422Check)
                {
                    string name = item.Name;
                    int key = int.Parse(name.Substring(19));
                    if (item.Checked)
                    {
                        Function.SetConfigValue("checkBox_422_select_" + key.ToString().PadLeft(2, '0'), "True");
                    }
                    else
                    {
                        Function.SetConfigValue("checkBox_422_select_" + key.ToString().PadLeft(2, '0'), "False");
                    }
                }
            }
            catch (Exception ex)
            {

            }

            #endregion
        }



        private void buttonEdit1_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            ButtonEdit editor = (ButtonEdit)sender;
            EditorButton Button = e.Button;
            String msgstr = "未选择文件";
            String ConfigPath = "PATH_DAT_01";
            byte FrameHeadLastByte = 0x00;

            String SenderName = editor.Name;
            int SendChan = int.Parse(SenderName.Substring(10)) - 1;//界面上从1开始，实际数组从0开始
            FrameHeadLastByte = (byte)SendChan;//1D0x中0x的值，在此获取

            msgstr = "通道" + (SendChan + 1).ToString();

            ConfigPath = "PATH_DAT_" + SenderName.Substring(10).PadLeft(2, '0');//配置文件中存储

            Trace.WriteLine(msgstr);
            Trace.WriteLine(ConfigPath);
            Trace.WriteLine(FrameHeadLastByte);

            if (Button.Caption == "SelectBin")
            {
                openFileDialog1.InitialDirectory = Data.Path + @"码本文件\";
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
        }


        private void CheckEnable_LVDS_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            DevExpress.XtraBars.BarCheckItem chk = (DevExpress.XtraBars.BarCheckItem)sender;
            DevExpress.XtraBars.Docking.DockPanel panel;
            switch (chk.Name)
            {
                case "CheckEnable_BoxRS422":
                    panel = this.dockPanel_BoxRS422;
                    break;
                case "CheckEnable_BoxRS422_Show":
                    panel = this.dockPanel_BoxRS422_Show;
                    break;
                case "CheckEnable_BoxLVDS_A":
                    panel = this.dockPanel_BoxLVDS_A;
                    break;
                case "CheckEnable_BoxLVDS_B":
                    panel = this.dockPanel_BoxLVDS_A;
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
                case "CheckEnable_BoxLVDS_Compare":
                    panel = this.dockPanel_LVDS_Compare;
                    break;
                default:
                    panel = this.dockPanel_BoxRS422;
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
            System.Diagnostics.Process.Start("Explorer", Program.GetStartupPath() + @"存储数据\LVDS机箱数据\");
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
                for (int i = 0; i < 128; i++)
                {
                    double value = 0;
                    if (j == 0 && i < Func_DA.DABoard1Nums) value = (double)Func_DA.dt_DA1.Rows[i]["电压"];
                    if (j == 1 && i < Func_DA.DABoard2Nums) value = (double)Func_DA.dt_DA2.Rows[i]["电压"];
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
                        USB.SendCMD(Data.DAid, 0x82, 0x01);
                        USB.SendCMD(Data.DAid, 0x82, 0x00);
                    }
                    else
                    {
                        USB.SendCMD(Data.DAid, 0x81, (byte)((0x01) << (i + 4 * j)));
                        USB.SendCMD(Data.DAid, 0x81, 0x00);
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
                    if (value < 0 || value > 10)
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
                    if (value < 0 || value > 10)
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

        private void dataGridView5_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (e.ColumnIndex == 0)//button列
                {
                    byte[] OCData = new byte[24];//此处不用OCOutChanNums，用32表示输出所有路数
                    for (int i = 0; i < OCData.Count(); i++) OCData[i] = 0x0;

                    OCData[e.RowIndex] = (byte)((int)Func_OC.dt_OC_Out.Rows[e.RowIndex]["脉宽"]);

                    Func_OC.Send2OCBD(OCData);

                    MyLog.Info("指令输出:" + Func_OC.dt_OC_Out.Rows[e.RowIndex]["名称"]);
                }
            }
        }
        private void dataGridView_OC_Out_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView5_CellContentClick(sender, e);
        }

        private void btn_VOut_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            byte[] OCData = new byte[24];
            for (int i = 0; i < OCData.Count(); i++)
            {
                if (i < Func_OC.OCOutChanNums)
                    OCData[i] = (byte)((int)Func_OC.dt_OC_Out.Rows[i]["脉宽"]);
                else
                    OCData[i] = 0x0;
            }
            Func_OC.Send2OCBD(OCData);
            MyLog.Info("统一输出全部OC！");
        }

        private void btn_RegSet_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (null != barEdit_BoxSelect.EditValue)
            {
                int CardId = Convert.ToInt32(barEdit_BoxSelect.EditValue.ToString().Split(':')[0], 16);

                if (null != barEdit_RegAddr.EditValue)
                {
                    string temp_addr = barEdit_RegAddr.EditValue.ToString();
                    string temp_value = barEdit_RegValue.EditValue.ToString();
                    byte addr = 0x00;
                    byte value = 0x80;

                    try
                    {
                        addr = Convert.ToByte(temp_addr, 16);
                        value = Convert.ToByte(temp_value, 16);

                        if (addr < 0xff && addr >= 0x80 && value < 0x80 && value >= 0x0)
                        {
                            if (!USB.SendCMD(CardId, addr, value)) MessageBox.Show("设备未连接，请检查连接！");
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


        }

        private void barEdit_BoxSelect_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }


        private void buttonEdit_422_1_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            ButtonEdit editor = (ButtonEdit)sender;
            EditorButton Button = e.Button;

            String msgstr = "未选择文件";
            String ConfigPath = "PATH_DAT_01";
            byte FrameHeadLastByte = 0x00;

            String SenderName = editor.Name;
            int SendChan = int.Parse(SenderName.Substring(15)) - 1;//界面上从1开始，实际数组从0开始//buttonEdit_422_1
            FrameHeadLastByte = (byte)(0x10 + SendChan);//1D0x中0x的值，在此获取

            msgstr = "422通道" + (SendChan + 1).ToString();

            ConfigPath = "PATH_422_DAT_" + SenderName.Substring(15).PadLeft(2, '0');//配置文件中存储

            Trace.WriteLine(msgstr);
            Trace.WriteLine(ConfigPath);
            Trace.WriteLine(FrameHeadLastByte);

            if (Button.Caption == "SelectBin")
            {
                openFileDialog1.InitialDirectory = Data.Path + @"码本文件\";
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
            //else
            //{
            //    try
            //    {
            //        if (editor.Text != null)
            //        {
            //            FileStream file = new FileStream(editor.Text, FileMode.Open, FileAccess.Read);

            //            //int fileBytes = (int)file.Length + 8;//为何要+8??
            //            int fileBytes = (int)file.Length;
            //            byte[] read_file_buf = new byte[fileBytes];
            //            for (int i = 0; i < fileBytes; i++) read_file_buf[i] = 0xff;
            //            file.Read(read_file_buf, 0, fileBytes);

            //            int AddToFour = fileBytes % 4;

            //            //1D0x + 长度2Bytes + 数据+(填写00填满32位) + 4 * C0DEC0DE
            //            byte[] FinalSendBytes = new byte[fileBytes + 4 - AddToFour + 20];
            //            byte[] head = new byte[2] { 0x1D, FrameHeadLastByte };
            //            byte[] len = new byte[2] { 0, 0 };
            //            len[0] = (byte)(fileBytes / 256);
            //            len[1] = (byte)(fileBytes % 256);
            //            byte[] end = new byte[16] { 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE };

            //            head.CopyTo(FinalSendBytes, 0);
            //            len.CopyTo(FinalSendBytes, 2);
            //            read_file_buf.CopyTo(FinalSendBytes, 4);


            //            if (AddToFour != 0)
            //            {
            //                byte[] add_buf = new byte[4 - AddToFour];//4-余数才是要补的数据
            //                for (int t = 0; t < add_buf.Count(); t++)
            //                {
            //                    add_buf[t] = 0x0;
            //                }
            //                add_buf.CopyTo(FinalSendBytes, fileBytes + 4);
            //                end.CopyTo(FinalSendBytes, fileBytes + 4 + add_buf.Count());
            //            }
            //            else
            //            {
            //                end.CopyTo(FinalSendBytes, fileBytes + 4);
            //            }

            //            file.Close();

            //            if (USB.MyDeviceList[Data.LVDSid] != null)
            //            {
            //                int Repeats = int.Parse(SendRepeats[SendChan].Text);
            //                int DivTime = int.Parse(SendDivTimes[SendChan].Text);



            //                if (Repeats > 1)
            //                {
            //                    new Thread(() => { LoopSend2USB(Data.LVDSid, FinalSendBytes, Repeats, DivTime, 0x81, SendChan); }).Start();
            //                }
            //                else
            //                {

            //                    USB.SendCMD(Data.LVDSid, (byte)(0x81 + SendChan / 7), (byte)(0x1 << SendChan % 7));
            //                    USB.SendCMD(Data.LVDSid, (byte)(0x81 + SendChan / 7), 0x0);

            //                    USB.SendData(Data.LVDSid, FinalSendBytes);
            //                }
            //            }
            //            else
            //            {
            //                MyLog.Error("向设备" + msgstr + "注入码表失败，请检查设置及连接！");
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show("检查码表路径是否正确！" + ex.Message);
            //        MyLog.Error(ex.Message);
            //    }
            //}
        }

        private void SetFreq()
        {
            int freq = 0;
            string temp_freq = barEditItem_lvdsfreqset.EditValue.ToString();
            int.TryParse(temp_freq, out freq);
            if (freq >= 8 && freq <= 150)
            {
                int addon = 900 % freq;
                int temp = 900 / freq;
                if (addon != 0)
                {
                    double t = (double)(900 / (double)temp);
                    barEditItem_lvdsfreqreal.EditValue = t.ToString("0.00");
                }
                else
                {
                    barEditItem_lvdsfreqreal.EditValue = freq.ToString("0.00");
                }
                USB.SendCMD(Data.LVDSid, 0x86, (byte)temp);//写入参数

                Function.SetConfigValue("LVDS_Send_SetFreq", barEditItem_lvdsfreqset.EditValue.ToString());
                Function.SetConfigValue("LVDS_Send_RealFreq", barEditItem_lvdsfreqreal.EditValue.ToString());
            }
            else
            {
                MessageBox.Show("输入10~150之间的数值！");
            }

            int freq422 = 0;
            string temp_freq422 = barEditItem_422freqset.EditValue.ToString();
            int.TryParse(temp_freq422, out freq422);

            if (freq422 >= 0 && freq422 <= 30)
            {
                int addon422 = 900 % freq422;
                int temp422 = 900 / freq422;
                if (addon422 != 0)
                {
                    double t = (double)(900 / (double)temp422);

                    barEditItem_422freqreal.EditValue = t.ToString("0.00");
                }
                else
                {
                    barEditItem_422freqreal.EditValue = freq422.ToString("0.00");
                }
                USB.SendCMD(Data.LVDSid, 0x87, (byte)temp422);//写入参数

                Function.SetConfigValue("422_Send_SetFreq", barEditItem_422freqset.EditValue.ToString());
                Function.SetConfigValue("422_Send_RealFreq", barEditItem_422freqreal.EditValue.ToString());
            }
            else
            {
                MessageBox.Show("输入0~30之间的数值！");
            }

            USB.SendCMD(Data.LVDSid, 0x8f, 0x01);//写入参数后，写寄存器使能
            USB.SendCMD(Data.LVDSid, 0x8f, 0x00);
        }

        private void barButton_lvdsfreqset_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            int freq = 0;
            string temp_freq = barEditItem_lvdsfreqset.EditValue.ToString();
            int.TryParse(temp_freq, out freq);
            if (freq >= 8 && freq <= 150)
            {
                int addon = 900 % freq;
                int temp = 900 / freq;
                if (addon != 0)
                {
                    double t = (double)(900 / (double)temp);
                    barEditItem_lvdsfreqreal.EditValue = t.ToString("0.00");
                }
                else
                {
                    barEditItem_lvdsfreqreal.EditValue = freq.ToString("0.00");
                }
                USB.SendCMD(Data.LVDSid, 0x86, (byte)temp);//写入参数

                Function.SetConfigValue("LVDS_Send_SetFreq", barEditItem_lvdsfreqset.EditValue.ToString());
                Function.SetConfigValue("LVDS_Send_RealFreq", barEditItem_lvdsfreqreal.EditValue.ToString());
            }
            else
            {
                MessageBox.Show("输入10~150之间的数值！");
            }

            int freq422 = 0;
            string temp_freq422 = barEditItem_422freqset.EditValue.ToString();
            int.TryParse(temp_freq422, out freq422);

            if (freq422 >= 0 && freq422 <= 30)
            {
                int addon422 = 900 % freq422;
                int temp422 = 900 / freq422;
                if (addon422 != 0)
                {
                    double t = (double)(900 / (double)temp422);

                    barEditItem_422freqreal.EditValue = t.ToString("0.00");
                }
                else
                {
                    barEditItem_422freqreal.EditValue = freq422.ToString("0.00");
                }
                USB.SendCMD(Data.LVDSid, 0x87, (byte)temp422);//写入参数

                Function.SetConfigValue("422_Send_SetFreq", barEditItem_422freqset.EditValue.ToString());
                Function.SetConfigValue("422_Send_RealFreq", barEditItem_422freqreal.EditValue.ToString());
            }
            else
            {
                MessageBox.Show("输入0~30之间的数值！");
            }

            USB.SendCMD(Data.LVDSid, 0x8f, 0x01);//写入参数后，写寄存器使能
            USB.SendCMD(Data.LVDSid, 0x8f, 0x00);
        }

        private void barEditItem_lvdsfreqset_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void barButton_422Down_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            if (barButton_422Down.Caption == "开始发送")
            {
                checkBox1.Enabled = false;
                barButton_422Down.Caption = "停止发送";
                barButton_422Down.ImageOptions.LargeImage = CKServer.Properties.Resources.Stop_btn;

                SetFreq();

                byte FrameHeadLastByte = 0x10;

                #region 422选中通道发送
                foreach (var item in Channel_422Check)
                {
                    if (item.Checked == true)
                    {
                        string name = item.Name;
                        int key = int.Parse(name.Substring(19));
                        FrameHeadLastByte = (byte)(0x10+(byte)(key - 1));
                        int SendChan = key - 1;
                        try
                        {
                            if (Channel_422Editor[key - 1].Text != null)
                            {
                                FileStream file = new FileStream(Channel_422Editor[key - 1].Text, FileMode.Open, FileAccess.Read);

                                //int fileBytes = (int)file.Length + 8;//为何要+8??
                                int fileBytes = (int)file.Length;
                                byte[] read_file_buf = new byte[fileBytes];
                                for (int i = 0; i < fileBytes; i++) read_file_buf[i] = 0xff;
                                file.Read(read_file_buf, 0, fileBytes);

                                int AddToFour = fileBytes % 4;

                                //1D0x + 长度2Bytes + 数据+(填写00填满32位) + 4 * C0DEC0DE
                                byte[] FinalSendBytes = new byte[fileBytes + 4 - AddToFour + 20];
                                byte[] head = new byte[2] { 0x1D, FrameHeadLastByte };
                                byte[] len = new byte[2] { 0, 0 };
                                len[0] = (byte)(fileBytes / 256);
                                len[1] = (byte)(fileBytes % 256);
                                byte[] end = new byte[16] { 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE };

                                head.CopyTo(FinalSendBytes, 0);
                                len.CopyTo(FinalSendBytes, 2);
                                read_file_buf.CopyTo(FinalSendBytes, 4);


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

                                if (USB.MyDeviceList[Data.LVDSid] != null)
                                {
                                    USB.SendCMD(Data.LVDSid, (byte)(0x81 + SendChan / 7), (byte)(0x1 << SendChan % 7));
                                    USB.SendCMD(Data.LVDSid, (byte)(0x81 + SendChan / 7), 0x0);

                                    USB.SendData(Data.LVDSid, FinalSendBytes);
                                }
                                else
                                {
                                    MyLog.Error("向422通道" + key.ToString() + "注入码表失败，请检查设置及连接！");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("检查码表路径是否正确！" + ex.Message);
                            MyLog.Error(ex.Message);
                        }
                    }
                    item.Enabled = false;
                }
#endregion

            }
            else
            {
                checkBox1.Enabled = true;
                barButton_422Down.Caption = "开始发送";
                barButton_422Down.ImageOptions.LargeImage = CKServer.Properties.Resources.Start_btn;

                #region 422选中通道禁止发送
                foreach (var item in Channel_422Check)
                {
                    if (item.Checked == true)
                    {
                        string name = item.Name;
                        int key = int.Parse(name.Substring(19));
                        int SendChan = key - 1;
                        if (USB.MyDeviceList[Data.LVDSid] != null)
                        {
                            USB.SendCMD(Data.LVDSid, (byte)(0x81 + SendChan / 7), (byte)(0x1 << SendChan % 7));
                            USB.SendCMD(Data.LVDSid, (byte)(0x81 + SendChan / 7), 0x0);
                        }

                    }
                    item.Enabled = true;
                }
                #endregion
            }
        }

        private void checkBox1_CheckStateChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                foreach (var item in Channel_422Check)
                {
                    item.Checked = true;
                }
            }
            else
            {
                foreach (var item in Channel_422Check)
                {
                    item.Checked = false;
                }
            }
        }

        private void checkBox2_CheckStateChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                foreach (var item in Channel_LVDSCheck)
                {
                    item.Checked = true;
                }
            }
            else
            {
                foreach (var item in Channel_LVDSCheck)
                {
                    item.Checked = false;
                }
            }
        }

        private void barButton_LVDSDown_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (barButton_LVDSDown.Caption == "开始发送")
            {
                checkBox2.Enabled = false;//发送过程禁止选择
                barButton_LVDSDown.Caption = "停止发送";
                barButton_LVDSDown.ImageOptions.LargeImage = CKServer.Properties.Resources.Stop_btn;

                SetFreq();//设置频率

                byte FrameHeadLastByte = 0x00;//设置LVDS发送通道1D00
                #region LVDS选中通道发送
                foreach (var item in Channel_LVDSCheck)
                {
                    if (item.Checked == true)
                    {
                        string name = item.Name;
                        int key = int.Parse(name.Substring(20));
                        FrameHeadLastByte = (byte)(key - 1);
                        int SendChan = key - 1;
                        try
                        {
                            if (Channel_LVDSEditor[key - 1].Text != null)
                            {
                                FileStream file = new FileStream(Channel_LVDSEditor[key - 1].Text, FileMode.Open, FileAccess.Read);

                                //int fileBytes = (int)file.Length + 8;//为何要+8??
                                int fileBytes = (int)file.Length;
                                byte[] read_file_buf = new byte[fileBytes];
                                for (int i = 0; i < fileBytes; i++) read_file_buf[i] = 0xff;
                                file.Read(read_file_buf, 0, fileBytes);

                                int AddToFour = fileBytes % 4;

                                //1D0x + 长度2Bytes + 数据+(填写00填满32位) + 4 * C0DEC0DE
                                byte[] FinalSendBytes = new byte[fileBytes + 4 - AddToFour + 20];
                                byte[] head = new byte[2] { 0x1D, FrameHeadLastByte };
                                byte[] len = new byte[2] { 0, 0 };
                                len[0] = (byte)(fileBytes / 256);
                                len[1] = (byte)(fileBytes % 256);
                                byte[] end = new byte[16] { 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE };

                                head.CopyTo(FinalSendBytes, 0);
                                len.CopyTo(FinalSendBytes, 2);
                                read_file_buf.CopyTo(FinalSendBytes, 4);


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

                                if (USB.MyDeviceList[Data.LVDSid] != null)
                                {
                                    USB.SendCMD(Data.LVDSid, (byte)(0x83 + SendChan / 7), (byte)(0x1 << SendChan % 7));
                                    USB.SendCMD(Data.LVDSid, (byte)(0x83 + SendChan / 7), 0x0);

                                    USB.SendData(Data.LVDSid, FinalSendBytes);

                                }
                                else
                                {
                                    MyLog.Error("向LVDS通道" + key.ToString() + "注入码表失败，请检查设置及连接！");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("检查码表路径是否正确！" + ex.Message);
                            MyLog.Error(ex.Message);
                        }
                    }

                    item.Enabled = false;
                }
                #endregion 




            }
            else
            {
                checkBox2.Enabled = true;
                barButton_LVDSDown.Caption = "开始发送";
                barButton_LVDSDown.ImageOptions.LargeImage = CKServer.Properties.Resources.Start_btn;

                #region LVDS通道停止发送
                foreach (var item in Channel_LVDSCheck)
                {
                    if (item.Checked == true)
                    {
                        string name = item.Name;
                        int key = int.Parse(name.Substring(20));
                        int SendChan = key - 1;
                        if (USB.MyDeviceList[Data.LVDSid] != null)
                        {
                            USB.SendCMD(Data.LVDSid, (byte)(0x83 + SendChan / 7), (byte)(0x1 << SendChan % 7));
                            USB.SendCMD(Data.LVDSid, (byte)(0x83 + SendChan / 7), 0x0);

                        }
                    }
                    item.Enabled = true;
                }

                #endregion



            }
        }

        private void barButton_422Keepset_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void barButton_lvdsKeepset_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void dataGridView_LVDS_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {


        }

        private void btn_OpenPath_ErrorLVDSLog_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            System.Diagnostics.Process.Start("Explorer", Program.GetStartupPath() + @"ErrorLog\");
        }

        private void btn_OpenPath_Storage2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            System.Diagnostics.Process.Start("Explorer", Program.GetStartupPath() + @"存储数据\LVDS机箱数据\");
        }


        private void dataGridView_RS422_Send_CellClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView_RS422_Send_CellDoubleClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (e.ColumnIndex == 3)
                {
                    openFileDialog2.InitialDirectory = Data.Path + @"码本文件\";
                    string tmpFilter = openFileDialog2.Filter;
                    string title = openFileDialog2.Title;
                    openFileDialog2.Title = "选择要注入的码表文件";
                    openFileDialog2.Filter = "dat files (*.dat)|*.dat|All files (*.*) | *.*";

                    if (openFileDialog2.ShowDialog() == DialogResult.OK) //selecting bitstream
                    {
                        Func_RS422.dt_RS422_Send.Rows[e.RowIndex][e.ColumnIndex] = openFileDialog2.FileName;
                        Refresh();
                    }
                    else
                    {
                        openFileDialog2.Filter = tmpFilter;
                        openFileDialog2.Title = title;
                        return;
                    }

                }
            }
        }

        private void dataGridView_RS422_Send_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (e.ColumnIndex == 1)
                {
                    DataGridViewCheckBoxCell checkcell = (DataGridViewCheckBoxCell)dataGridView_RS422_Send.Rows[e.RowIndex].Cells[1];
                    Boolean flag = Convert.ToBoolean(checkcell.EditedFormattedValue);
                    Func_RS422.dt_RS422_Send.Rows[e.RowIndex][e.ColumnIndex] = flag;
                }

                if (e.ColumnIndex == 4)
                {
                    try
                    {
                        //1.根据路径获取数据
                        int SendChan = e.RowIndex;
                        byte FrameHeadLastByte = (byte)SendChan;

                        FileStream file = new FileStream((string)Func_RS422.dt_RS422_Send.Rows[e.RowIndex]["码本路径"], FileMode.Open, FileAccess.Read);

                        int fileBytes = (int)file.Length;
                        byte[] read_file_buf = new byte[fileBytes];
                        for (int i = 0; i < fileBytes; i++) read_file_buf[i] = 0xff;
                        file.Read(read_file_buf, 0, fileBytes);

                        int AddToFour = fileBytes % 4;

                        //1D0x + 长度2Bytes + 数据+(填写00填满32位) + 4 * C0DEC0DE
                        byte[] FinalSendBytes = new byte[fileBytes + 4 - AddToFour + 20];
                        byte[] head = new byte[2] { 0x1D, FrameHeadLastByte };
                        byte[] len = new byte[2] { 0, 0 };
                        len[0] = (byte)(fileBytes / 256);
                        len[1] = (byte)(fileBytes % 256);
                        byte[] end = new byte[16] { 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE };

                        head.CopyTo(FinalSendBytes, 0);
                        len.CopyTo(FinalSendBytes, 2);
                        read_file_buf.CopyTo(FinalSendBytes, 4);


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

                        if (USB.MyDeviceList[Data.SC422id] != null)
                        {
                            //2.通过寄存器将RAM清0
                            byte addr = (byte)(0x81 + e.RowIndex / 7);
                            byte value = (byte)(e.RowIndex % 7);
                            USB.SendCMD(Data.SC422id, addr, (byte)(0x1 << value));


                            USB.SendCMD(Data.SC422id, addr, 0x0);

                            //3.将数据推送到RAM
                            USB.SendData(Data.SC422id, FinalSendBytes);
                        }
                        else
                        {
                            MyLog.Error("SC422机箱未连接！请检查设置及连接！");
                        }
                    }
                    catch (Exception ex)
                    {
                        MyLog.Error(ex.Message + "From异步422发送通道：" + (e.RowIndex + 1).ToString());
                    }

                }
            }

        }


        private void barCheckItem_SelectAll_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (barCheckItem_SelectAll.Checked)
            {
                for (int i = 0; i < Func_RS422.RS422Nums; i++)
                {
                    Func_RS422.dt_RS422_Send.Rows[i]["选中发送"] = true;
                }
            }
            else
            {
                for (int i = 0; i < Func_RS422.RS422Nums; i++)
                {
                    Func_RS422.dt_RS422_Send.Rows[i]["选中发送"] = false;
                }
            }
        }

        private void barButtonItem_422Send_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            foreach (DataRow dr in Func_RS422.dt_RS422_Send.Rows)
            {
                if ((bool)dr["选中发送"])
                {
                    int SendChan = (int)dr["序号"] - 1;

                    try
                    {
                        //1.根据路径获取数据

                        byte FrameHeadLastByte = (byte)SendChan;

                        FileStream file = new FileStream((string)dr["码本路径"], FileMode.Open, FileAccess.Read);

                        int fileBytes = (int)file.Length;
                        byte[] read_file_buf = new byte[fileBytes];
                        for (int i = 0; i < fileBytes; i++) read_file_buf[i] = 0xff;
                        file.Read(read_file_buf, 0, fileBytes);

                        int AddToFour = fileBytes % 4;

                        //1D0x + 长度2Bytes + 数据+(填写00填满32位) + 4 * C0DEC0DE
                        byte[] FinalSendBytes = new byte[fileBytes + 4 - AddToFour + 20];
                        byte[] head = new byte[2] { 0x1D, FrameHeadLastByte };
                        byte[] len = new byte[2] { 0, 0 };
                        len[0] = (byte)(fileBytes / 256);
                        len[1] = (byte)(fileBytes % 256);
                        byte[] end = new byte[16] { 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE };

                        head.CopyTo(FinalSendBytes, 0);
                        len.CopyTo(FinalSendBytes, 2);
                        read_file_buf.CopyTo(FinalSendBytes, 4);

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

                        if (USB.MyDeviceList[Data.SC422id] != null)
                        {
                            //2.通过寄存器将RAM清0
                            byte addr = (byte)(0x81 + SendChan / 7);
                            byte value = (byte)(SendChan % 7);
                            USB.SendCMD(Data.SC422id, addr, (byte)(0x1 << value));
                            USB.SendCMD(Data.SC422id, addr, 0x0);

                            //3.将数据推送到RAM
                            USB.SendData(Data.SC422id, FinalSendBytes);
                        }
                        else
                        {
                            MyLog.Error("SC422机箱未连接！请检查设置及连接！");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        MyLog.Error(ex.Message + "From异步422发送通道：" + (SendChan + 1).ToString());
                    }


                }
            }


        }

        private void buttonEdit25_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            ButtonEdit editor = (ButtonEdit)sender;
            EditorButton Button = e.Button;

            String msgstr = "未选择文件";
            String ConfigPath = "PATH_DAT_01";
            byte FrameHeadLastByte = 0x00;

            //buttonEdit_SC422_TB1
            String SenderName = editor.Name;
            int SendChan = int.Parse(SenderName.Substring(19)) - 1;//界面上从1开始，实际数组从0开始//buttonEdit_422_1
            FrameHeadLastByte = (byte)(0x30 + SendChan);//1D0x中0x的值，在此获取

            msgstr = "SC422机箱：同步422通道" + (SendChan + 1).ToString();

            if (Button.Caption == "SelectBin")
            {
                openFileDialog1.InitialDirectory = Data.Path + @"码本文件\";
                string tmpFilter = openFileDialog1.Filter;
                string title = openFileDialog1.Title;
                openFileDialog1.Title = "选择要注入的码表文件";
                openFileDialog1.Filter = "dat files (*.dat)|*.dat|All files (*.*) | *.*";

                if (openFileDialog1.ShowDialog() == DialogResult.OK) //selecting bitstream
                {
                    editor.Text = openFileDialog1.FileName;
                    Refresh();
                    MyLog.Info("选取" + msgstr + "文件成功");
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
                try
                {
                    FileStream file = new FileStream(editor.Text, FileMode.Open, FileAccess.Read);

                    //int fileBytes = (int)file.Length + 8;//为何要+8??
                    int fileBytes = (int)file.Length;
                    byte[] read_file_buf = new byte[fileBytes];
                    for (int i = 0; i < fileBytes; i++) read_file_buf[i] = 0xff;
                    file.Read(read_file_buf, 0, fileBytes);

                    int AddToFour = fileBytes % 4;

                    //1D0x + 长度2Bytes + 数据+(填写00填满32位) + 4 * C0DEC0DE
                    byte[] FinalSendBytes = new byte[fileBytes + 4 - AddToFour + 20];
                    byte[] head = new byte[2] { 0x1D, FrameHeadLastByte };
                    byte[] len = new byte[2] { 0, 0 };
                    len[0] = (byte)(fileBytes / 256);
                    len[1] = (byte)(fileBytes % 256);
                    byte[] end = new byte[16] { 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE };

                    head.CopyTo(FinalSendBytes, 0);
                    len.CopyTo(FinalSendBytes, 2);
                    read_file_buf.CopyTo(FinalSendBytes, 4);


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

                    if (USB.MyDeviceList[Data.SC422id] != null)
                    {
                        //RAM清0
                        USB.SendCMD(Data.SC422id, (byte)(0x88 + SendChan / 7), (byte)(0x1 << SendChan % 7));
                        USB.SendCMD(Data.SC422id, (byte)(0x88 + SendChan / 7), 0x0);

                        //向下注数
                        USB.SendData(Data.SC422id, FinalSendBytes);

                    }
                    else
                    {
                        MyLog.Error("向设备" + msgstr + "注入码表失败，请检查设置及连接！");
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("检查码表路径是否正确！" + ex.Message);
                    MyLog.Error(ex.Message);
                }
            }
        }

        private void barEditItem13_ItemPress(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void barCheckItem1_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (barCheckItem_A1.Checked)
            {
                barCheckItem_A2.Checked = false;
                Func_RS422.ShowTB1 = true;
            }



        }

        private void barCheckItem2_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (barCheckItem_A2.Checked)
            {
                barCheckItem_A1.Checked = false;
                Func_RS422.ShowTB1 = false;
            }
        }

        private void barCheckItem_B1_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (barCheckItem_B1.Checked)
            {
                barCheckItem_B2.Checked = false;
                Func_RS422.ShowTB2 = true;
            }
        }

        private void barCheckItem_B2_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (barCheckItem_B2.Checked)
            {
                barCheckItem_B1.Checked = false;
                Func_RS422.ShowTB2 = true;
            }
        }

        private void barButtonItem_SelectFile_A_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            openFileDialog1.InitialDirectory = Data.Path + @"码本文件\";
            string tmpFilter = openFileDialog1.Filter;
            string title = openFileDialog1.Title;
            openFileDialog1.Title = "选择要注入的码表文件";
            openFileDialog1.Filter = "dat files (*.dat)|*.dat|All files (*.*) | *.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK) //selecting bitstream
            {
                barEditItem_FilePath_A.EditValue = openFileDialog1.FileName;
                Refresh();
            }
            else
            {
                openFileDialog1.Filter = tmpFilter;
                openFileDialog1.Title = title;
                return;
            }

        }

        private void barButtonItem_SelectFile_B_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            openFileDialog1.InitialDirectory = Data.Path + @"码本文件\";
            string tmpFilter = openFileDialog1.Filter;
            string title = openFileDialog1.Title;
            openFileDialog1.Title = "选择要注入的码表文件";
            openFileDialog1.Filter = "dat files (*.dat)|*.dat|All files (*.*) | *.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK) //selecting bitstream
            {
                barEditItem_FilePath_B.EditValue = openFileDialog1.FileName;
                Refresh();
            }
            else
            {
                openFileDialog1.Filter = tmpFilter;
                openFileDialog1.Title = title;
                return;
            }
        }

        private void barButton_RS422freqset_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            double freq = 115.2;
            double.TryParse(barEditItem_RS422freqset.EditValue.ToString(), out freq);
            byte addr = 0x8a;
            byte value = 0x0;
            switch (freq)
            {
                case 1:
                    value = 0x1;
                    break;
                case 2:
                    value = 0x2;
                    break;
                case 4:
                    value = 0x4;
                    break;
                case 8:
                    value = 0x8;
                    break;
                case 16:
                    value = 0x10;
                    break;
                case 32:
                    value = 0x20;
                    break;
                case 64:
                    value = 0x40;
                    break;
                case 115.2:
                    value = 0x0;
                    break;
                default:
                    value = 0x0;
                    break;
            }

            USB.SendCMD(Data.SC422id, 0x8a, value);
            USB.SendCMD(Data.SC422id, 0x8b, value);

        }

        private void barButton_TB1Send_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (barButton_TB1Send.Caption == "发送")
            {
                try
                {
                    byte FrameHeadLastByte = 0x30;

                    FileStream file = new FileStream(barEditItem_FilePath_A.EditValue.ToString(), FileMode.Open, FileAccess.Read);

                    //int fileBytes = (int)file.Length + 8;//为何要+8??
                    int fileBytes = (int)file.Length;
                    byte[] read_file_buf = new byte[fileBytes];
                    for (int i = 0; i < fileBytes; i++) read_file_buf[i] = 0xff;
                    file.Read(read_file_buf, 0, fileBytes);

                    int AddToFour = fileBytes % 4;

                    //1D0x + 长度2Bytes + 数据+(填写00填满32位) + 4 * C0DEC0DE
                    byte[] FinalSendBytes = new byte[fileBytes + 4 - AddToFour + 20];
                    byte[] head = new byte[2] { 0x1D, FrameHeadLastByte };
                    byte[] len = new byte[2] { 0, 0 };
                    len[0] = (byte)(fileBytes / 256);
                    len[1] = (byte)(fileBytes % 256);
                    byte[] end = new byte[16] { 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE };

                    head.CopyTo(FinalSendBytes, 0);
                    len.CopyTo(FinalSendBytes, 2);
                    read_file_buf.CopyTo(FinalSendBytes, 4);

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
                    if (USB.MyDeviceList[Data.SC422id] != null)
                    {
                        //RAM清0
                        USB.SendCMD(Data.SC422id, 0x88, 0x01);
                        USB.SendCMD(Data.SC422id, 0x88, 0x00);

                        //向下注数
                        USB.SendData(Data.SC422id, FinalSendBytes);


                        if (barCheckItem_A1.Checked)
                        {//单次发送
                            Func_RS422.Reg_8CH = (byte)(Func_RS422.Reg_8CH & 0xfe);
                            USB.SendCMD(Data.SC422id, 0x8C, Func_RS422.Reg_8CH);


                        }
                        else if (barCheckItem_A2.Checked)
                        {
                            //连续发送
                            Func_RS422.Reg_8CH = (byte)(Func_RS422.Reg_8CH | 0x1);
                            USB.SendCMD(Data.SC422id, 0x8C, Func_RS422.Reg_8CH);

                            barButton_TB1Send.Caption = "停止";
                            barButton_TB1Send.ImageOptions.LargeImage = CKServer.Properties.Resources.Stop_btn;
                            barCheckItem_A1.Enabled = false;
                            barCheckItem_A2.Enabled = false;
                        }
                        else
                        {
                            MessageBox.Show("请选择单次发送 or 连续发送！");
                        }

                    }
                    else
                    {
                        MyLog.Error("向设备注数失败，请检查设置及连接！");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("检查码本路径是否正确！" + ex.Message);
                    MyLog.Error(ex.Message);
                }
            }
            else
            {
                //RAM清0
                USB.SendCMD(Data.SC422id, 0x88, 0x01);
                USB.SendCMD(Data.SC422id, 0x88, 0x00);

                barButton_TB1Send.Caption = "发送";
                barButton_TB1Send.ImageOptions.LargeImage = CKServer.Properties.Resources.Start_btn;
                barCheckItem_A1.Enabled = true;
                barCheckItem_A2.Enabled = true;
            }

        }

        private void barButton_TB2Send_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (barButton_TB2Send.Caption == "发送")
            {
                try
                {
                    byte FrameHeadLastByte = 0x31;

                    FileStream file = new FileStream(barEditItem_FilePath_B.EditValue.ToString(), FileMode.Open, FileAccess.Read);

                    //int fileBytes = (int)file.Length + 8;//为何要+8??
                    int fileBytes = (int)file.Length;
                    byte[] read_file_buf = new byte[fileBytes];
                    for (int i = 0; i < fileBytes; i++) read_file_buf[i] = 0xff;
                    file.Read(read_file_buf, 0, fileBytes);

                    int AddToFour = fileBytes % 4;

                    //1D0x + 长度2Bytes + 数据+(填写00填满32位) + 4 * C0DEC0DE
                    byte[] FinalSendBytes = new byte[fileBytes + 4 - AddToFour + 20];
                    byte[] head = new byte[2] { 0x1D, FrameHeadLastByte };
                    byte[] len = new byte[2] { 0, 0 };
                    len[0] = (byte)(fileBytes / 256);
                    len[1] = (byte)(fileBytes % 256);
                    byte[] end = new byte[16] { 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE, 0xC0, 0xDE };

                    head.CopyTo(FinalSendBytes, 0);
                    len.CopyTo(FinalSendBytes, 2);
                    read_file_buf.CopyTo(FinalSendBytes, 4);

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
                    if (USB.MyDeviceList[Data.SC422id] != null)
                    {
                        //RAM清0
                        USB.SendCMD(Data.SC422id, 0x88, 0x02);
                        USB.SendCMD(Data.SC422id, 0x88, 0x00);

                        //向下注数
                        USB.SendData(Data.SC422id, FinalSendBytes);


                        if (barCheckItem_B1.Checked)
                        {
                            Func_RS422.Reg_8CH = (byte)(Func_RS422.Reg_8CH & 0xfd);
                            USB.SendCMD(Data.SC422id, 0x8C, Func_RS422.Reg_8CH);
                        }
                        else if (barCheckItem_B2.Checked)
                        {
                            //向连续发送寄存器发指令
                            Func_RS422.Reg_8CH = (byte)(Func_RS422.Reg_8CH | 0x2);
                            USB.SendCMD(Data.SC422id, 0x8C, Func_RS422.Reg_8CH);

                            barButton_TB2Send.Caption = "停止";
                            barButton_TB2Send.ImageOptions.LargeImage = CKServer.Properties.Resources.Stop_btn;
                            barCheckItem_B1.Enabled = false;
                            barCheckItem_B2.Enabled = false;
                        }
                        else
                        {
                            MessageBox.Show("请选择单次发送 or 连续发送！");
                        }

                    }
                    else
                    {
                        MyLog.Error("向设备注数失败，请检查设置及连接！");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("检查码本路径是否正确！" + ex.Message);
                    MyLog.Error(ex.Message);
                }
            }
            else
            {
                //RAM清0
                USB.SendCMD(Data.SC422id, 0x88, 0x02);
                USB.SendCMD(Data.SC422id, 0x88, 0x00);

                barButton_TB2Send.Caption = "发送";
                barButton_TB2Send.ImageOptions.LargeImage = CKServer.Properties.Resources.Start_btn;
                barCheckItem_B1.Enabled = true;
                barCheckItem_B2.Enabled = true;
            }
        }

        private void barButtonItem_RS422BaudSet_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            int baud = 9600;
            int.TryParse(barEditItem_RS422_baud.EditValue.ToString(), out baud);
            byte addr = 0x89;
            byte value = 0x0;
            switch (baud)
            {
                case 9600:
                    value = 0x0;
                    break;
                case 19200:
                    value = 0x1;
                    break;
                case 38400:
                    value = 0x2;
                    break;
                case 57600:
                    value = 0x3;
                    break;
                case 115200:
                    value = 0x4;
                    break;
                case 460800:
                    value = 0x5;
                    break;
                case 921600:
                    value = 0x6;
                    break;
                case 1000000:
                    value = 0x7;
                    break;
                default:
                    value = 0x0;
                    break;
            }

            USB.SendCMD(Data.SC422id, addr, value);

        }

        private void barButtonItem_GNSS_A_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (barButtonItem_GNSS_A.Caption == "开始：GNSS秒信号A")
            {
                barButtonItem_GNSS_A.Caption = "停止：GNSS秒信号A";
                barButtonItem_GNSS_A.ImageOptions.Image = CKServer.Properties.Resources.Stop_btn;

                Func_RS422.Reg_8DH = (byte)(Func_RS422.Reg_8DH | 0x01);
                USB.SendCMD(Data.SC422id, 0x8D, Func_RS422.Reg_8DH);
            }
            else
            {
                Func_RS422.Reg_8DH = (byte)(Func_RS422.Reg_8DH & 0xfe);
                USB.SendCMD(Data.SC422id, 0x8D, Func_RS422.Reg_8DH);

                barButtonItem_GNSS_A.Caption = "开始：GNSS秒信号A";
                barButtonItem_GNSS_A.ImageOptions.Image = CKServer.Properties.Resources.Start_btn;
            }
        }

        private void barButtonItem_GNSS_B_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (barButtonItem_GNSS_B.Caption == "开始：GNSS秒信号B")
            {
                barButtonItem_GNSS_B.Caption = "停止：GNSS秒信号B";
                barButtonItem_GNSS_B.ImageOptions.Image = CKServer.Properties.Resources.Stop_btn;

                Func_RS422.Reg_8DH = (byte)(Func_RS422.Reg_8DH | 0x02);
                USB.SendCMD(Data.SC422id, 0x8D, Func_RS422.Reg_8DH);

            }
            else
            {
                barButtonItem_GNSS_B.Caption = "开始：GNSS秒信号B";
                barButtonItem_GNSS_B.ImageOptions.Image = CKServer.Properties.Resources.Start_btn;

                Func_RS422.Reg_8DH = (byte)(Func_RS422.Reg_8DH & 0xfd);
                USB.SendCMD(Data.SC422id, 0x8D, Func_RS422.Reg_8DH);
            }
        }


        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dockPanel_LVDS_Compare_Click(object sender, EventArgs e)
        {

        }

        private void radioButton8_Click(object sender, EventArgs e)
        {            
            RadioButton rdo = (RadioButton)sender;
            string Name = rdo.Name;
            int key = int.Parse(Name.Substring(11, 1)) - 1;

            if (rdo.Text == "不比对")
            {
                Data.LVDSCPTAG = false;
            }
            else
            {
                Data.LVDSCPTAG = true;

                Trace.WriteLine(Name + "clicked!");

                Func_LVDS.LVDS_ComPare_Chan = key;

                Function.SetConfigValue("LVDS_ComPareChan", key.ToString());

                for (int i = 0; i < 32; i++)
                {
                    Func_LVDS.dt_LVDS_01.Rows[i]["序号"] = (i + 1).ToString();
                    Func_LVDS.dt_LVDS_01.Rows[i]["通道名称"] = Function.GetConfigStr(Data.LVDSconfigPath, "LVDS_Channel_" + key.ToString(), "VCID_Channel_" + (i / 2 + 1).ToString(), "name");

                    if (i % 2 == 0) Func_LVDS.dt_LVDS_01.Rows[i]["实时/延时"] = "实时";
                    if (i % 2 == 1) Func_LVDS.dt_LVDS_01.Rows[i]["实时/延时"] = "延时";

                    Func_LVDS.dt_LVDS_01.Rows[i]["VCID"] = Function.GetConfigStr(Data.LVDSconfigPath, "LVDS_Channel_" + key.ToString(), "VCID_Channel_" + (i / 2 + 1).ToString(), "vcid");
                    Func_LVDS.dt_LVDS_01.Rows[i]["帧计数"] = 0;
                    Func_LVDS.dt_LVDS_01.Rows[i]["收到数据"] = 0;
                    Func_LVDS.dt_LVDS_01.Rows[i]["比对长度"] = int.Parse(Function.GetConfigStr(Data.LVDSconfigPath, "LVDS_Channel_" + key.ToString(), "VCID_Channel_" + (i / 2 + 1).ToString(), "CPLen"));
                    Func_LVDS.dt_LVDS_01.Rows[i]["出错行"] = 0;
                    Func_LVDS.dt_LVDS_01.Rows[i]["出错列"] = 0;

                    Func_LVDS.CPLenList[i] = int.Parse(Function.GetConfigStr(Data.LVDSconfigPath, "LVDS_Channel_" + key.ToString(), "VCID_Channel_" + (i / 2 + 1).ToString(), "CPLen"));

                }

            }


        }

        private void dataGridView_LVDS_01_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex>=0)
            {
                if(e.ColumnIndex == 6)
                {
                    Func_LVDS.CPLenList[e.RowIndex] = (int)Func_LVDS.dt_LVDS_01.Rows[e.RowIndex]["比对长度"];

                    String add = "LVDS_Channel_" + Func_LVDS.LVDS_ComPare_Chan.ToString();
                    String key = "VCID_Channel_" + (e.RowIndex + 1).ToString();
                    String name = "CPLen";
                    String value = Func_LVDS.dt_LVDS_01.Rows[e.RowIndex]["比对长度"].ToString();
                    Function.SaveConfigStr(Data.LVDSconfigPath, add,key, name, value);

                }
            }
        }
    }
}
