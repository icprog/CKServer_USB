using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Configuration;

namespace CKServer
{
    class Data
    {
        public static int CountV = 0;
        public static int CountA = 0;

        public static int OnlyID = 0;

        public static string Path = null;                       //程序运行目录

        public static int Cardid = int.Parse(ConfigurationManager.AppSettings["CardID"]);

        public static int[] DA_Card1 = new int[128];      //根据用户输入转化为对应的int值
        public static int[] DA_Card2 = new int[128];      //根据用户输入转化为对应的int值

        public static byte[] DA_Send1 = new byte[512];      //根据DA_Card1的实际DA值转化为对应的码字
        public static byte[] DA_Send2 = new byte[512];      //根据DA_Card2的实际DA值转化为对应的码字

        public static double[] DA1_value_a = new double[128];                         //DA板卡1修正参数a
        public static double[] DA1_value_b = new double[128];                         //DA板卡1修正参数b

        public static double[] DA2_value_a = new double[128];                         //DA板卡2修正参数a
        public static double[] DA2_value_b = new double[128];                         //DA板卡2修正参数b

        //   public static object synObj_dq0 = new object();
        public static object synObj_sc1 = new object();


        public static Dictionary<string, Queue<byte[]>> Apid_EPDU_Dictionary = new Dictionary<string, Queue<byte[]>>();
        public static Dictionary<string, Queue<byte[]>> Apid_EPDU_Dictionary2 = new Dictionary<string, Queue<byte[]>>();
        public static Dictionary<string, Queue<byte[]>> Apid_EPDU_Dictionary3 = new Dictionary<string, Queue<byte[]>>();

        public static List<string> AlreadyOnApid = new List<string>();
        public static List<string> AlreadyOnApid2 = new List<string>();
        public static List<string> AlreadyOnApid3 = new List<string>();

        public static bool TBChan1Used = false;
        public static bool TBChan2Used = false;
        public static bool MOXAChanUsed = false;

        public static int SCRecvCounts = 0;


        public static string DAconfigPath = Program.GetStartupPath() + @"配置文件\DAconfig.xml";
        public static string ADconfigPath = Program.GetStartupPath() + @"配置文件\ADconfig.xml";
        public static string RconfigPath = Program.GetStartupPath() + @"配置文件\Rconfig.xml";
        public static string OCconfigPath = Program.GetStartupPath() + @"配置文件\OCconfig.xml";
        public static string RegconfigPath = Program.GetStartupPath() + @"配置文件\Regconfig.xml";
        public static string YCconfigPath = Program.GetStartupPath() + @"配置文件\YCconfig.xml";
        public static string LVDSconfigPath = Program.GetStartupPath() + @"配置文件\LVDSconfig.xml";
        public static void SaveConfig(string Path,string key ,string value)
        {
            XDocument xDoc = XDocument.Load(Path);
            XmlReader reader = xDoc.CreateReader();

            bool Matched = false;//是否已在XML中

            foreach (var p in xDoc.Root.Elements("add"))
            {
                if (p.Attribute("key").Value == key)
                {
                    p.Attribute("value").Value = value;
                    Matched = true;
                }
            }
            if(Matched == false)
            {
                XElement element = new XElement("add", new XAttribute("key", key), new XAttribute("value", value));
                xDoc.Root.Add(element);
            }

            xDoc.Save(Path);
            //var query = from p in xDoc.Root.Elements("add")
            //            where p.Attribute("key").Value == "DAModifyA1"
            //            orderby p.Value
            //            select p.Value;

            //foreach (string s in query)
            //{
            //    Console.WriteLine(s);
            //}

        }

        public static string GetConfig(string Path,string key)
        {
            XDocument xDoc = XDocument.Load(Path);
            XmlReader reader = xDoc.CreateReader();
            string value = "Error";

            var query = from p in xDoc.Root.Elements("add")
                        where p.Attribute("key").Value == key
                        select p.Attribute("value").Value;

            foreach(string s in query)
            {
                value = s;
            }

            //foreach (var p in xDoc.Root.Elements("add"))
            //{
            //    if (p.Attribute("key").Value == key)
            //    {
            //        value = p.Attribute("value").Value;
            //    }
            //}
            return value;

        }

     
        public static void SaveConfigStr(string Path, string key, string name ,string value)
        {
            XDocument xDoc = XDocument.Load(Path);
            XmlReader reader = xDoc.CreateReader();

            bool Matched = false;//是否已在XML中

            foreach (var p in xDoc.Root.Elements("add"))
            {
                if (p.Attribute("key").Value == key)
                {
                    p.Attribute(name).Value = value;
                    Matched = true;
                }
            }
            if (Matched == false)
            {
                XElement element = new XElement("add", new XAttribute("key", key), new XAttribute("name", value));
                xDoc.Root.Add(element);
            }

            xDoc.Save(Path);
            //var query = from p in xDoc.Root.Elements("add")
            //            where p.Attribute("key").Value == "DAModifyA1"
            //            orderby p.Value
            //            select p.Value;

            //foreach (string s in query)
            //{
            //    Console.WriteLine(s);
            //}

        }



    }
}
