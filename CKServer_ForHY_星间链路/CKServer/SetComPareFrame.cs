using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CKServer
{
    public partial class SetComPareFrame : Form
    {
        DataTable dt_LVDS_CP = new DataTable();

        public MainForm mform;

        public SetComPareFrame(CKServer.MainForm parent)
        {
            InitializeComponent();
            mform = parent;
        }

        public void init_table()
        {
            dt_LVDS_CP.Columns.Add("名称", typeof(String));
            dt_LVDS_CP.Columns.Add("设定值", typeof(String));

            for (int i = 0; i < 8; i++)
            {
                DataRow dr = dt_LVDS_CP.NewRow();

                dr["名称"] = Function.GetConfigStr(Data.LVDSconfigPath, "add", "LVDS_Config_" + i.ToString(), "name");
                dr["设定值"] = Function.GetConfigStr(Data.LVDSconfigPath, "add", "LVDS_Config_" + i.ToString(), "value");
                dt_LVDS_CP.Rows.Add(dr);
            }

            dataGridView1.DataSource = dt_LVDS_CP;
            dataGridView1.AllowUserToAddRows = false;
        }


        private void SetComPareFrame_Load(object sender, EventArgs e)
        {
            dt_LVDS_CP.Columns.Add("名称", typeof(String));
            dt_LVDS_CP.Columns.Add("设定值", typeof(String));

            for (int i = 0; i < 8; i++)
            {
                DataRow dr = dt_LVDS_CP.NewRow();

                dr["名称"] = Function.GetConfigStr(Data.LVDSconfigPath, "add", "LVDS_Config_" + i.ToString(), "name");
                dr["设定值"] = Function.GetConfigStr(Data.LVDSconfigPath, "add", "LVDS_Config_" + i.ToString(), "value");
                dt_LVDS_CP.Rows.Add(dr);
            }

            dataGridView1.DataSource = dt_LVDS_CP;
            dataGridView1.AllowUserToAddRows = false;
        }
    }
}
