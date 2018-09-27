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
        private DataTable dtDA1 = new DataTable();
        private DataTable dtDA2 = new DataTable();

        private DataTable dtDA3 = new DataTable();
        private DataTable dtDA4 = new DataTable();

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
                dtDA1.Columns.Add("序号", typeof(Int32));
                dtDA1.Columns.Add("名称", typeof(String));
                dtDA1.Columns.Add("衰减控制", typeof(double));
                for (int i = 0; i < 16; i++)
                {
                    DataRow dr = dtDA1.NewRow();
                    dr["序号"] = i + 1;
                    dr["名称"] = ConfigurationManager.AppSettings["DA_Board1_Channel_" + i.ToString()];
                    dr["衰减控制"] = 0;
                    dtDA1.Rows.Add(dr);
                }
                dataGridView4.DataSource = dtDA1;
                dataGridView4.AllowUserToAddRows = false;

                dtDA2.Columns.Add("序号", typeof(Int32));
                dtDA2.Columns.Add("名称", typeof(String));
                dtDA2.Columns.Add("衰减控制", typeof(double));
                for (int i = 0; i < 16; i++)
                {
                    DataRow dr = dtDA2.NewRow();
                    dr["序号"] = i + 17;
                    dr["名称"] = ConfigurationManager.AppSettings["DA_Board2_Channel_" + i.ToString()]; ;
                    dr["衰减控制"] = 0;
                    dtDA2.Rows.Add(dr);
                }
                dataGridView5.DataSource = dtDA2;
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




                dtDA3.Columns.Add("序号", typeof(Int32));
                dtDA3.Columns.Add("名称", typeof(String));
                dtDA3.Columns.Add("相位控制", typeof(double));
                for (int i = 0; i < 16; i++)
                {
                    DataRow dr = dtDA3.NewRow();
                    dr["序号"] = i + 1;
                    dr["名称"] = ConfigurationManager.AppSettings["DA_Board3_Channel_" + i.ToString()];
                    dr["相位控制"] = 0;
                    dtDA3.Rows.Add(dr);
                }
                dataGridView_Fudu1.DataSource = dtDA3;
                dataGridView_Fudu1.AllowUserToAddRows = false;

                dtDA4.Columns.Add("序号", typeof(Int32));
                dtDA4.Columns.Add("名称", typeof(String));
                dtDA4.Columns.Add("相位控制", typeof(double));
                for (int i = 0; i < 16; i++)
                {
                    DataRow dr = dtDA4.NewRow();
                    dr["序号"] = i + 17;
                    dr["名称"] = ConfigurationManager.AppSettings["DA_Board4_Channel_" + i.ToString()];
                    dr["相位控制"] = 0;
                    dtDA4.Rows.Add(dr);
                }
                dataGridView_Fudu2.DataSource = dtDA4;
                dataGridView_Fudu2.AllowUserToAddRows = false;

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

            for (int i = 0; i < 16; i++)
            {
                int[] ValueTable = new int[6] { 5, 5, 5, 5, 5, 5 };

                double transValue = (double)dtDA1.Rows[i]["衰减控制"] / 0.5;
                byte btv = (byte)transValue;

                if ((btv & 0x01) != 0) ValueTable[0] = 0;
                if ((btv & 0x02) != 0) ValueTable[1] = 0;
                if ((btv & 0x04) != 0) ValueTable[2] = 0;
                if ((btv & 0x08) != 0) ValueTable[3] = 0;
                if ((btv & 0x10) != 0) ValueTable[4] = 0;
                if ((btv & 0x20) != 0) ValueTable[5] = 0;
                if (i < 8)//C1-C48
                {
                    ValueTable.CopyTo(Data.DA_Card1, i * 6);
                }
                else//C65-C112
                {
                    ValueTable.CopyTo(Data.DA_Card1, 16+i * 6);
                }
            }

            for (int i = 0; i < 16; i++)
            {
                int[] ValueTable = new int[6] { 5, 5, 5, 5, 5, 5 };

                double transValue = (double)dtDA2.Rows[i]["衰减控制"] / 0.5;
                byte btv = (byte)transValue;

                if ((btv & 0x01) != 0) ValueTable[0] = 0;
                if ((btv & 0x02) != 0) ValueTable[1] = 0;
                if ((btv & 0x04) != 0) ValueTable[2] = 0;
                if ((btv & 0x08) != 0) ValueTable[3] = 0;
                if ((btv & 0x10) != 0) ValueTable[4] = 0;
                if ((btv & 0x20) != 0) ValueTable[5] = 0;

                if (i < 8)//C1-C48
                {
                    ValueTable.CopyTo(Data.DA_Card2, i * 6);
                }
                else//C65-C112
                {
                    ValueTable.CopyTo(Data.DA_Card2, 16 + i * 6);
                }
            }

            CKCommon.clcDAValue(ref Data.DA_Send1, ref Data.DA_Card1,1);

            byte[] DASend = new byte[128 + 8];
            DASend[0] = 0x1D;
            DASend[1] = 0x00;         //1D20 1D21 1D22 1D23对应4个DA芯片
            DASend[2] = 0x00;
            DASend[3] = 0x20;//0x0080 = 128
            DASend[132] = 0xC0;
            DASend[133] = 0xDE;
            DASend[134] = 0xC0;
            DASend[135] = 0xDE;

            Array.Copy(Data.DA_Send1, 0, DASend, 4, 128);
            DASend[1] = 0x00;
            USB.SendCMD(Data.Cardid1, 0x81, 0x01);
            USB.SendCMD(Data.Cardid1, 0x81, 0x00);
            USB.SendData(Data.Cardid1, DASend);

            Array.Copy(Data.DA_Send1, 128, DASend, 4, 128);
            DASend[1] = 0x01;
            USB.SendCMD(Data.Cardid1, 0x81, 0x02);
            USB.SendCMD(Data.Cardid1, 0x81, 0x00);
            USB.SendData(Data.Cardid1, DASend);

            Array.Copy(Data.DA_Send1, 256, DASend, 4, 128);
            DASend[1] = 0x02;
            USB.SendCMD(Data.Cardid1, 0x81, 0x04);
            USB.SendCMD(Data.Cardid1, 0x81, 0x00);
            USB.SendData(Data.Cardid1, DASend);


            Array.Copy(Data.DA_Send1, 384, DASend, 4, 128);
            DASend[1] = 0x03;
            USB.SendCMD(Data.Cardid1, 0x81, 0x08);
            USB.SendCMD(Data.Cardid1, 0x81, 0x00);
            USB.SendData(Data.Cardid1, DASend);


            CKCommon.clcDAValue(ref Data.DA_Send2, ref Data.DA_Card2, 2);

            Array.Copy(Data.DA_Send2, 0, DASend, 4, 128);
            DASend[1] = 0x04;
            USB.SendCMD(Data.Cardid1, 0x81, 0x10);
            USB.SendCMD(Data.Cardid1, 0x81, 0x00);
            USB.SendData(Data.Cardid1, DASend);

            Array.Copy(Data.DA_Send2, 128, DASend, 4, 128);
            DASend[1] = 0x05;
            USB.SendCMD(Data.Cardid1, 0x81, 0x20);
            USB.SendCMD(Data.Cardid1, 0x81, 0x00);
            USB.SendData(Data.Cardid1, DASend);

            Array.Copy(Data.DA_Send2, 256, DASend, 4, 128);
            DASend[1] = 0x06;
            USB.SendCMD(Data.Cardid1, 0x81, 0x40);
            USB.SendCMD(Data.Cardid1, 0x81, 0x00);
            USB.SendData(Data.Cardid1, DASend);

            Array.Copy(Data.DA_Send2, 384, DASend, 4, 128);
            DASend[1] = 0x07;
            USB.SendCMD(Data.Cardid1, 0x82, 0x01);
            USB.SendCMD(Data.Cardid1, 0x82, 0x00);
            USB.SendData(Data.Cardid1, DASend);
        }

        private void btn_ResetDA_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            foreach (DataRow dr in dtDA1.Rows)
            {
                dr["衰减控制"] = 0;
            }
            foreach (DataRow dr in dtDA2.Rows)
            {
                dr["衰减控制"] = 0;
            }
        }

        private void btn_setall_EditValueChanged(object sender, EventArgs e)
        {
            double tag = double.Parse(btn_setall.EditValue.ToString());
            tag = tag % 0.5;
            if (tag != 0)
            {
                MessageBox.Show("衰减值必须是0.5的整数倍");
                btn_setall.EditValue = 0;
            }
            else
            {
                foreach (DataRow dr in dtDA1.Rows)
                {
                    dr["衰减控制"] = btn_setall.EditValue;
                }

                foreach (DataRow dr in dtDA2.Rows)
                {
                    dr["衰减控制"] = btn_setall.EditValue;
                }
            }
        }

        private void btn_add_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            foreach (DataRow dr in dtDA1.Rows)
            {
                if ((double)dr["衰减控制"] < 31.5)
                    dr["衰减控制"] = (double)dr["衰减控制"] + 0.5;
            }

            foreach (DataRow dr in dtDA2.Rows)
            {
                if ((double)dr["衰减控制"] < 31.5)
                    dr["衰减控制"] = (double)dr["衰减控制"] + 0.5;
            }
        }

        private void btn_dec_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            foreach (DataRow dr in dtDA1.Rows)
            {
                if ((double)dr["衰减控制"] >= 0.5)
                    dr["衰减控制"] = (double)dr["衰减控制"] - 0.5;
            }

            foreach (DataRow dr in dtDA2.Rows)
            {
                if ((double)dr["衰减控制"] >= 0.5)
                    dr["衰减控制"] = (double)dr["衰减控制"] - 0.5;
            }
        }

        private void dataGridView4_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (e.ColumnIndex == 2)
                {
                    try
                    {
                        double t = (double)dtDA1.Rows[e.RowIndex]["衰减控制"];
                        if (t < 0 || t > 31.5)
                        {
                            dtDA1.Rows[e.RowIndex]["衰减控制"] = 0;
                        }

                        double tag = t % 0.5;
                        if (tag != 0)
                        {
                            MessageBox.Show("衰减值必须是0.5的整数倍");
                            dtDA1.Rows[e.RowIndex]["衰减控制"] = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        MyLog.Error(ex.Message + "From:dataGridView4_CellEndEdit");
                        dtDA1.Rows[e.RowIndex]["衰减控制"] = 0;
                    }
                }
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
                        double t = (double)dtDA2.Rows[e.RowIndex]["衰减控制"];
                        if (t < 0 || t > 31.5)
                        {
                            dtDA2.Rows[e.RowIndex]["衰减控制"] = 0;
                        }

                        double tag = t % 0.5;
                        if (tag != 0)
                        {
                            MessageBox.Show("衰减值必须是0.5的整数倍");
                            dtDA2.Rows[e.RowIndex]["衰减控制"] = 0;
                        }

                    }
                    catch (Exception ex)
                    {
                        MyLog.Error(ex.Message + "From:dataGridView4_CellEndEdit");
                        dtDA2.Rows[e.RowIndex]["衰减控制"] = 0;
                    }
                }
            }
        }

        private void dataGridView4_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.Exception != null)
            {
                MessageBox.Show("请输入正确的衰减值，输入无效数值将自动设置为0");

                if (e.RowIndex >= 0)
                {
                    if (e.ColumnIndex == 2)
                    {
                        dtDA1.Rows[e.RowIndex]["衰减控制"] = 0;
                    }
                }
            }
        }

        private void dataGridView5_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.Exception != null)
            {
                MessageBox.Show("请输入正确的衰减值，输入无效数值将自动设置为0");

                if (e.RowIndex >= 0)
                {
                    if (e.ColumnIndex == 2)
                    {
                        dtDA2.Rows[e.RowIndex]["衰减控制"] = 0;
                    }
                }
            }
        }

        private void btn_Modify_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //if (myDAmodifyForm != null)
            //{
            //    myDAmodifyForm.Activate();
            //}
            //else
            //{
            //    myDAmodifyForm = new DAmodify();
            //}
            //myDAmodifyForm.ShowDialog();

            dockPanel2.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
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
            switch(btn.Name)
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
                    ModifyStr += dt.Rows[i][0] + ","+ dt.Rows[i][1] + ","+ dt.Rows[i][2] + "\r\n";
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

        private void btn_da_load_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            String Path = Program.GetStartupPath() + @"码本文件\";
            openFileDialog1.InitialDirectory = Path;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                MyLog.Info("载入码本成功！");

                string[] content = File.ReadAllLines(openFileDialog1.FileName);
                string[] temp = new string[3];

                if (content.Length >= 64)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        temp = content[i].Split(',');
                        dtDA1.Rows[i][2] = double.Parse(temp[2].Trim());
                    }
                    for(int i=16;i<32;i++)
                    {
                        temp = content[i].Split(',');
                        dtDA2.Rows[i-16][2] = double.Parse(temp[2].Trim());
                    }
                    for (int i = 32; i < 48; i++)
                    {
                        temp = content[i].Split(',');
                        dtDA3.Rows[i-32][2] = double.Parse(temp[2].Trim());
                    }
                    for (int i = 48; i < 64; i++)
                    {
                        temp = content[i].Split(',');
                        dtDA4.Rows[i - 48][2] = double.Parse(temp[2].Trim());
                    }
                }
            }
        }

        private void btn_da_save_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            String Path = Program.GetStartupPath() + @"码本文件\";
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            saveFileDialog1.InitialDirectory = Path;

            saveFileDialog1.Filter = "文本文件(*.txt)|*.txt|All files(*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                String ModifyStr = "";

                for (int i = 0; i < 16; i++)
                {
                    ModifyStr += dtDA1.Rows[i][0] + "," + dtDA1.Rows[i][1] + "," + dtDA1.Rows[i][2] + "\r\n";
                }
                for (int i = 0; i < 16; i++)
                {
                    ModifyStr += dtDA2.Rows[i][0] + "," + dtDA2.Rows[i][1] + "," + dtDA2.Rows[i][2] + "\r\n";
                }
                for (int i = 0; i < 16; i++)
                {
                    ModifyStr += dtDA3.Rows[i][0] + "," + dtDA3.Rows[i][1] + "," + dtDA3.Rows[i][2] + "\r\n";
                }
                for (int i = 0; i < 16; i++)
                {
                    ModifyStr += dtDA4.Rows[i][0] + "," + dtDA4.Rows[i][1] + "," + dtDA4.Rows[i][2] + "\r\n";
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
            if(CheckEnable_LocalTime.Checked==true)
            {
                barStaticItem3.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
            }
            else
            {
                barStaticItem3.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            }

            if(CheckEnable_RunTime.Checked == true)
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
        }

        private void btn_HelpNeed_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            MessageBox.Show(
                "Designed by 测控通信室 2018.\r\n" +
                "硬件支持--黄禹\r\n" +
                "软件支持--平佳伟\r\n"
                );
        }

        private void btn_VOut2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            for (int i = 0; i < 16; i++)
            {
                int[] ValueTable = new int[6] { 0, 0, 0, 0, 0, 0 };
                //全1-111111为63，代表360°，000001为5.626°，全0为0°

                double transValue = (double)dtDA3.Rows[i]["相位控制"] / 5.625;
                byte btv = (byte)transValue;

                if ((btv & 0x01) != 0) ValueTable[0] = 5;
                if ((btv & 0x02) != 0) ValueTable[1] = 5;
                if ((btv & 0x04) != 0) ValueTable[2] = 5;
                if ((btv & 0x08) != 0) ValueTable[3] = 5;
                if ((btv & 0x10) != 0) ValueTable[4] = 5;
                if ((btv & 0x20) != 0) ValueTable[5] = 5;
                if (i < 8)//C1-C48
                {
                    ValueTable.CopyTo(Data.DA_Card3, i * 6);
                }
                else//C65-C112
                {
                    ValueTable.CopyTo(Data.DA_Card3, 16 + i * 6);
                }
            }

            for (int i = 0; i < 16; i++)
            {
                int[] ValueTable = new int[6] { 0, 0, 0, 0, 0, 0 };

                double transValue = (double)dtDA4.Rows[i]["相位控制"] / 5.625;
                byte btv = (byte)transValue;

                if ((btv & 0x01) != 0) ValueTable[0] = 5;
                if ((btv & 0x02) != 0) ValueTable[1] = 5;
                if ((btv & 0x04) != 0) ValueTable[2] = 5;
                if ((btv & 0x08) != 0) ValueTable[3] = 5;
                if ((btv & 0x10) != 0) ValueTable[4] = 5;
                if ((btv & 0x20) != 0) ValueTable[5] = 5;

                if (i < 8)//C1-C48
                {
                    ValueTable.CopyTo(Data.DA_Card4, i * 6);
                }
                else//C65-C112
                {
                    ValueTable.CopyTo(Data.DA_Card4, 16 + i * 6);
                }
            }

            CKCommon.clcDAValue(ref Data.DA_Send3, ref Data.DA_Card3, 1);

            byte[] DASend = new byte[128 + 8];
            DASend[0] = 0x1D;
            DASend[1] = 0x00;         //1D20 1D21 1D22 1D23对应4个DA芯片
            DASend[2] = 0x00;
            DASend[3] = 0x20;//0x0080 = 128
            DASend[132] = 0xC0;
            DASend[133] = 0xDE;
            DASend[134] = 0xC0;
            DASend[135] = 0xDE;

            Array.Copy(Data.DA_Send3, 0, DASend, 4, 128);
            DASend[1] = 0x00;
            USB.SendCMD(Data.Cardid2, 0x81, 0x01);
            USB.SendCMD(Data.Cardid2, 0x81, 0x00);
            USB.SendData(Data.Cardid2, DASend);

            Array.Copy(Data.DA_Send3, 128, DASend, 4, 128);
            DASend[1] = 0x01;
            USB.SendCMD(Data.Cardid2, 0x81, 0x02);
            USB.SendCMD(Data.Cardid2, 0x81, 0x00);
            USB.SendData(Data.Cardid2, DASend);

            Array.Copy(Data.DA_Send3, 256, DASend, 4, 128);
            DASend[1] = 0x02;
            USB.SendCMD(Data.Cardid2, 0x81, 0x04);
            USB.SendCMD(Data.Cardid2, 0x81, 0x00);
            USB.SendData(Data.Cardid2, DASend);


            Array.Copy(Data.DA_Send3, 384, DASend, 4, 128);
            DASend[1] = 0x03;
            USB.SendCMD(Data.Cardid2, 0x81, 0x08);
            USB.SendCMD(Data.Cardid2, 0x81, 0x00);
            USB.SendData(Data.Cardid2, DASend);


            CKCommon.clcDAValue(ref Data.DA_Send4, ref Data.DA_Card4, 2);

            Array.Copy(Data.DA_Send4, 0, DASend, 4, 128);
            DASend[1] = 0x04;
            USB.SendCMD(Data.Cardid2, 0x81, 0x10);
            USB.SendCMD(Data.Cardid2, 0x81, 0x00);
            USB.SendData(Data.Cardid2, DASend);

            Array.Copy(Data.DA_Send4, 128, DASend, 4, 128);
            DASend[1] = 0x05;
            USB.SendCMD(Data.Cardid2, 0x81, 0x20);
            USB.SendCMD(Data.Cardid2, 0x81, 0x00);
            USB.SendData(Data.Cardid2, DASend);

            Array.Copy(Data.DA_Send4, 256, DASend, 4, 128);
            DASend[1] = 0x06;
            USB.SendCMD(Data.Cardid2, 0x81, 0x40);
            USB.SendCMD(Data.Cardid2, 0x81, 0x00);
            USB.SendData(Data.Cardid2, DASend);

            Array.Copy(Data.DA_Send4, 384, DASend, 4, 128);
            DASend[1] = 0x07;

            USB.SendCMD(Data.Cardid2, 0x82, 0x01);
            USB.SendCMD(Data.Cardid2, 0x82, 0x00);
            USB.SendData(Data.Cardid2, DASend);
        }

        private void btn_add2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            foreach (DataRow dr in dtDA3.Rows)
            {
                if ((double)dr["相位控制"] < 354.375)
                    dr["相位控制"] = (double)dr["相位控制"] + 5.625;
            }

            foreach (DataRow dr in dtDA4.Rows)
            {
                if ((double)dr["相位控制"] < 354.375)
                    dr["相位控制"] = (double)dr["相位控制"] + 5.625;
            }
        }

        private void btn_dec2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            foreach (DataRow dr in dtDA3.Rows)
            {
                if ((double)dr["相位控制"] >= 5.625)
                    dr["相位控制"] = (double)dr["相位控制"] - 5.625;
            }

            foreach (DataRow dr in dtDA4.Rows)
            {
                if ((double)dr["相位控制"] >= 5.625)
                    dr["相位控制"] = (double)dr["相位控制"] - 5.625;
            }
        }

        private void barEditItem12_EditValueChanged(object sender, EventArgs e)
        {
            double tag = double.Parse(barEditItem12.EditValue.ToString());
            tag = tag % 5.625;
            if (tag != 0)
            {
                MessageBox.Show("衰减值必须是5.625的整数倍");
                barEditItem12.EditValue = 0;
            }
            else
            {
                foreach (DataRow dr in dtDA3.Rows)
                {
                    dr["相位控制"] = barEditItem12.EditValue;
                }

                foreach (DataRow dr in dtDA4.Rows)
                {
                    dr["相位控制"] = barEditItem12.EditValue;
                }
            }
        }

        private void dataGridView_Fudu1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (e.ColumnIndex == 2)
                {
                    try
                    {
                        double t = (double)dtDA3.Rows[e.RowIndex]["相位控制"];
                        if (t < 0 || t > 354.375)
                        {
                            dtDA3.Rows[e.RowIndex]["相位控制"] = 0;
                        }

                        double tag = t % 5.625;
                        if (tag != 0)
                        {
                            MessageBox.Show("相位值必须是5.625的整数倍");
                            dtDA3.Rows[e.RowIndex]["相位控制"] = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        MyLog.Error(ex.ToString());
                        dtDA3.Rows[e.RowIndex]["相位控制"] = 0;
                    }
                }
            }
        }

        private void dataGridView_Fudu1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.Exception != null)
            {
                MessageBox.Show("请输入正确的相位，输入无效数值将自动设置为0");

                if (e.RowIndex >= 0)
                {
                    if (e.ColumnIndex == 2)
                    {
                        dtDA3.Rows[e.RowIndex]["相位控制"] = 0;
                    }
                }
            }
        }

        private void dataGridView_Fudu2_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (e.ColumnIndex == 2)
                {
                    try
                    {
                        double t = (double)dtDA4.Rows[e.RowIndex]["相位控制"];
                        if (t < 0 || t > 354.375)
                        {
                            dtDA4.Rows[e.RowIndex]["相位控制"] = 0;
                        }

                        double tag = t % 5.625;
                        if (tag != 0)
                        {
                            MessageBox.Show("相位值必须是5.625的整数倍");
                            dtDA4.Rows[e.RowIndex]["相位控制"] = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        MyLog.Error(ex.ToString());
                        dtDA4.Rows[e.RowIndex]["相位控制"] = 0;
                    }
                }
            }
        }

        private void dataGridView_Fudu2_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.Exception != null)
            {
                MessageBox.Show("请输入正确的相位值，输入无效数值将自动设置为0");

                if (e.RowIndex >= 0)
                {
                    if (e.ColumnIndex == 2)
                    {
                        dtDA4.Rows[e.RowIndex]["相位控制"] = 0;
                    }
                }
            }
        }
    }
}
