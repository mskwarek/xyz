﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Xml;


namespace AtmSim.Config
{
    /*
     * Konfiguracja wezla sieci 
     */
    //[XmlRootAttribute("Node", Namespace = "", IsNullable = false)]
    public class Node
    {
        private int id = 0;
        //[XmlElementAttribute("ID")]
        public int Id { get { return id; } set { id = value; } }  // identyfikator urzadzenia

        private string type = "";
        //[XmlElementAttribute("Type")]
        public string Type { get { return type; } set { type = value; } } // typ wezla, moze byc source, sink lub node, mozna przerobic na enum, ale wteyd bedzie zapisywac do pliku numer a nie stringa      

        private string name = "";
        //[XmlElementAttribute("Name")]
        public string Name { get { return name; } set { name = value; } } // nazwa urzadzenia

        private List<PortIn> portsIn = new List<PortIn>();
        //[XmlArrayItem("PortsIn")]
        public List<PortIn> PortsIn { get { return portsIn; } set { portsIn = value; } } // ilosc portow wejsciowych wezla

        private List<PortOut> portsOut = new List<PortOut>();
        //[XmlArrayItem("PortsOut")]
        public List<PortOut> PortsOut { get { return portsOut; } set { portsOut = value; } } // ilos portow wyjsciowych routera

        public void save(string filename)
        {
            // create a writer and open the file
            TextWriter tw = new StreamWriter(filename);
          
            // write a line of text to the file
            tw.Write(Serial.SerializeObject(this));

            // close the stream
            tw.Close();
        }

        public static Node fopen(string filename)
        {
            TextReader tr = new StreamReader(filename);
            String str = "";
            String input = null;

            while ((input = tr.ReadLine()) != null)
            {
                str += input;
            }
            tr.Close();
            return ((Node)Serial.DeserializeObject(str, typeof(Node)));
        }
    }

    //[XmlRootAttribute("PortIn", Namespace = "", IsNullable = false)]
    public class PortIn
    {
        private int id = 0;
        //[XmlElementAttribute("ID")]
        public int Id { get { return id; } set { id = value; } }
    }

    //[XmlRootAttribute("PortOut", Namespace = "", IsNullable = false)]
    public class PortOut
    {
        private int id = 0;
        //[XmlElementAttribute("ID")]
        public int Id { get { return id; } set { id = value; } }
    }
}

  
 

