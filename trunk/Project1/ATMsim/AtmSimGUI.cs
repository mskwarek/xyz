﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AtmSim
{
    public partial class AtmSimGUI : Form
    {
        private string selectedName = "";

        public AtmSimGUI()
        {
            InitializeComponent();
        }

        private void netNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Manager.Reset();
            this.InitTestNetwork2(); // **** TODO **** w tym miejscu znajdzie się parsowanie pliku wejściowego
            this.elementListBox.Items.Clear();
            foreach (string element in Manager.GetElements())
                elementListBox.Items.Add(element);
        }

        private void netTopologyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*TopologyGUI topologyGUI = new TopologyGUI();
            topologyGUI.Show();*/
            TopologyView topologyView = new TopologyView();
            topologyView.Show();
        }

        private void InitTestNetwork()
        {
            //// Tymczasowo jedziemy z danymi na sztywno...
            //Manager.Config config = new Manager.Config();
            //Manager.Routing routing = new Manager.Routing();
            //Common.Log log = new Common.Log("Log 1:");
            //log.LogMsg("Galia est omnis divisa in partes tres");
            //log.LogMsg("Quorum unam incolunt Belgae aliam Aquitani");
            //log.LogMsg("Tertiam qui ipsorut Celtae nostra Gali apellantur");
            ///*
            //string log =     + Environment.NewLine +
            //                "" + Environment.NewLine +
            //                ;
            // */
            //config.Add("ID", "X");
            //config.Add("type", "leet");
            //routing.Add("B", "1");
            //routing.Add("A", "-");
            //Manager.AddNode("e1", config, routing, log);
            //config = new Manager.Config();
            //routing = new Manager.Routing();
            //config.Add("ID", "Y");
            //config.Add("type", "woot");
            //routing.Add("A", "2");
            //routing.Add("B", "-");
            //log = new Common.Log(log);
            //log.ChangeInit("Log 2:");
            //Manager.AddNode("e2", config, routing, log);
            //Manager.AddConnection("e1", "e2");
            //// Koniec kodu tymczasowego
        }

        private void InitTestNetwork2()
        {
            AtmSim.Components.Node Router1 = new AtmSim.Components.Node(1, 2, "Router1");
            AtmSim.Components.Node Router2 = new AtmSim.Components.Node(3, 1, "Router2");
            AtmSim.Components.Node Router3 = new AtmSim.Components.Node(1, 2, "Router3");
            
            AtmSim.Components.TestPortIn in21 = new AtmSim.Components.TestPortIn(0);
            AtmSim.Components.TestPortIn in12 = new AtmSim.Components.TestPortIn(0);
            AtmSim.Components.TestPortIn in32 = new AtmSim.Components.TestPortIn(1);
            AtmSim.Components.TestPortIn in13 = new AtmSim.Components.TestPortIn(0);
            AtmSim.Components.TestPortIn in35 = new AtmSim.Components.TestPortIn(0);
            AtmSim.Components.TestPortIn in42 = new AtmSim.Components.TestPortIn(2);

            AtmSim.Components.TestPortOut out12 = new AtmSim.Components.TestPortOut(0); out12.Connect(in12);
            AtmSim.Components.TestPortOut out13 = new AtmSim.Components.TestPortOut(1); out13.Connect(in13);
            AtmSim.Components.TestPortOut out21 = new AtmSim.Components.TestPortOut(0); out21.Connect(in21);
            AtmSim.Components.TestPortOut out32 = new AtmSim.Components.TestPortOut(0); out32.Connect(in32);
            AtmSim.Components.TestPortOut out42 = new AtmSim.Components.TestPortOut(0); out42.Connect(in42);
            AtmSim.Components.TestPortOut out35 = new AtmSim.Components.TestPortOut(1); out35.Connect(in35);

            Router1.GetPortsIn().SetValue(in21, 0);
            Router2.GetPortsIn().SetValue(in12, 0);
            Router2.GetPortsIn().SetValue(in32, 1);
            Router2.GetPortsIn().SetValue(in42, 2);
            Router3.GetPortsIn().SetValue(in13, 0);

            Router1.GetPortsOut().SetValue(out12, 0);
            Router1.GetPortsOut().SetValue(out13, 1);
            Router2.GetPortsOut().SetValue(out21, 0);
            Router3.GetPortsOut().SetValue(out32, 0);
            Router3.GetPortsOut().SetValue(out35, 1);

            AtmSim.Components.Sorce Source4 = new AtmSim.Components.Sorce();
            Source4.SetPortOut(out42);
            AtmSim.Components.Sink Sink5 = new AtmSim.Components.Sink(in35);

            Manager.AddNode("Router1", Router1.Agent);
            Manager.AddNode("Router2", Router2.Agent);
            Manager.AddNode("Router3", Router3.Agent);
        }

        private void elementListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.elementListBox.SelectedIndex >= 0)
                this.selectedName = (string)this.elementListBox.Items[this.elementListBox.SelectedIndex];
        }

        private void configButton_Click(object sender, EventArgs e)
        {
            /*ConfigGUI configGUI = new ConfigGUI(
                Manager.GetConfig(this.selectedName),
                Manager.GetRouting(this.selectedName));*/
            ConfigGUI configGUI = new ConfigGUI(this.selectedName);
            configGUI.Show();
        }

        private void logButton_Click(object sender, EventArgs e)
        {
            LogGUI logGUI = new LogGUI(this.selectedName);
            logGUI.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Manager.LogMsg(this.selectedName, "Hello!");
        }

    }
}
