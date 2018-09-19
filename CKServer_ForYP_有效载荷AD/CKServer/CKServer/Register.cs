using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CKServer
{
    class Register
    {
        public static Dictionary<byte, byte> RegDictionary = new Dictionary<byte, byte>();

        public static byte Byte80H = 0x00;
        public static byte Byte81H = 0x00;
        public static byte Byte82H = 0x00;
        public static byte Byte83H = 0x00;
        public static byte Byte84H = 0x00;
        public static byte Byte85H = 0x00;
        public static byte Byte86H = 0x00;
        public static byte Byte87H = 0x00;
        public static byte Byte88H = 0x00;
        public static byte Byte89H = 0x00;
        public static byte Byte8AH = 0x00;
        public static byte Byte8BH = 0x00;
        public static byte Byte8CH = 0x00;
        public static byte Byte8DH = 0x00;
        public static byte Byte8EH = 0x00;
        public static byte Byte8FH = 0x00;
        public static byte Byte90H = 0x00;
        public static byte Byte91H = 0x00;
        public static byte Byte92H = 0x00;
        public static byte Byte93H = 0x00;
        public static byte Byte94H = 0x00;
        public static byte Byte95H = 0x00;
        public static byte Byte96H = 0x00;
        public static byte Byte97H = 0x00;
        public static byte Byte98H = 0x00;
        public static byte Byte99H = 0x00;
        public static byte Byte9AH = 0x00;
        public static byte Byte9BH = 0x00;
        public static byte Byte9CH = 0x00;
        public static byte Byte9DH = 0x00;
        public static byte Byte9EH = 0x00;
        public static byte Byte9FH = 0x00;

        public static DataTable dt_Reg = new DataTable();
        public static void Init()
        {
            byte[] RegCtl = new byte[] {
                Byte80H, Byte81H, Byte82H, Byte83H, Byte84H, Byte85H, Byte86H, Byte87H,
                Byte88H, Byte89H, Byte8AH, Byte8BH, Byte8CH, Byte8DH, Byte8EH, Byte8FH,
                Byte90H, Byte91H, Byte92H, Byte93H, Byte94H, Byte95H, Byte96H, Byte97H,
                Byte98H, Byte99H, Byte9AH, Byte9BH, Byte9CH, Byte9DH, Byte9EH, Byte9FH };

            dt_Reg.Columns.Add("名称", typeof(String));
            dt_Reg.Columns.Add("地址", typeof(String));
            dt_Reg.Columns.Add("属性", typeof(String));
            dt_Reg.Columns.Add("bit7", typeof(String));
            dt_Reg.Columns.Add("bit6", typeof(String));
            dt_Reg.Columns.Add("bit5", typeof(String));
            dt_Reg.Columns.Add("bit4", typeof(String));
            dt_Reg.Columns.Add("bit3", typeof(String));
            dt_Reg.Columns.Add("bit2", typeof(String));
            dt_Reg.Columns.Add("bit1", typeof(String));
            dt_Reg.Columns.Add("bit0", typeof(String));
            for (int i = 0; i < 15; i++)
            {
                DataRow dr = dt_Reg.NewRow();
                dr["名称"] = Function.GetConfigStr(Data.RegconfigPath, "add", (0x80 + i).ToString("x2").ToUpper(), "name");
                dr["地址"] = Function.GetConfigStr(Data.RegconfigPath, "add", (0x80 + i).ToString("x2").ToUpper(), "key");
                dr["属性"] = Function.GetConfigStr(Data.RegconfigPath, "add", (0x80 + i).ToString("x2").ToUpper(), "property");
                //bit7保留，不使用
                dr["bit7"] = Function.GetConfigStr(Data.RegconfigPath, "add", (0x80 + i).ToString("x2").ToUpper(), "bit7");

                dr["bit6"] = Function.GetConfigStr(Data.RegconfigPath, "add", (0x80 + i).ToString("x2").ToUpper(), "bit6");
                if ((string)dr["bit6"] != "/") dr["bit6"] = "0:" + dr["bit6"];

                dr["bit5"] = Function.GetConfigStr(Data.RegconfigPath, "add", (0x80 + i).ToString("x2").ToUpper(), "bit5");
                if ((string)dr["bit5"] != "/") dr["bit5"] = "0:" + dr["bit5"];

                dr["bit4"] = Function.GetConfigStr(Data.RegconfigPath, "add", (0x80 + i).ToString("x2").ToUpper(), "bit4");
                if ((string)dr["bit4"] != "/") dr["bit4"] = "0:" + dr["bit4"];

                dr["bit3"] = Function.GetConfigStr(Data.RegconfigPath, "add", (0x80 + i).ToString("x2").ToUpper(), "bit3");
                if ((string)dr["bit3"] != "/") dr["bit3"] = "0:" + dr["bit3"];

                dr["bit2"] = Function.GetConfigStr(Data.RegconfigPath, "add", (0x80 + i).ToString("x2").ToUpper(), "bit2");
                if ((string)dr["bit2"] != "/") dr["bit2"] = "0:" + dr["bit2"];

                dr["bit1"] = Function.GetConfigStr(Data.RegconfigPath, "add", (0x80 + i).ToString("x2").ToUpper(), "bit1");
                if ((string)dr["bit1"] != "/") dr["bit1"] = "0:" + dr["bit1"];

                dr["bit0"] = Function.GetConfigStr(Data.RegconfigPath, "add", (0x80 + i).ToString("x2").ToUpper(), "bit0");
                if ((string)dr["bit0"] != "/") dr["bit0"] = "0:" + dr["bit0"];

                dt_Reg.Rows.Add(dr);

                RegDictionary.Add((byte)(0x80+i), RegCtl[i]);
            }

        }



    }

}
