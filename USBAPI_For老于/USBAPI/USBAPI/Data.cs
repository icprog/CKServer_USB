using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Data;

namespace USBSpeedTest
{
    class Data
    {
        public static int OnlyId = 0;

        public static List<byte> ADList01 = new List<byte>();//1D06
        public static List<byte> ADList02 = new List<byte>();//1D07
        public static List<byte> ADList03 = new List<byte>();//1D06
        public static List<byte> ADList04 = new List<byte>();//1D07

        public static DataTable dt_AD01 = new DataTable();
        public static DataTable dt_AD02 = new DataTable();
        public static DataTable dt_AD03 = new DataTable();
        public static DataTable dt_AD04 = new DataTable();

        public static double[] daRe_AD01 = new double[8] { 0,0,0,0,0,0,0,0};
        public static double[] daRe_AD02 = new double[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        public static double[] daRe_AD03 = new double[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        public static double[] daRe_AD04 = new double[8] { 0, 0, 0, 0, 0, 0, 0, 0 };

        public static bool AdFrmIsAlive = false;
      
    }
}
