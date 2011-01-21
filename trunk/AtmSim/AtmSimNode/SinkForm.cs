﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace AtmSim
{
    public partial class SinkForm : Form
    {
        private Components.Sink sink;
        private Components.Caller caller;
        Thread refresher;
        public SinkForm(Config.Node cNode, int mPort, int cPort)
        {
            sink = new Components.Sink(cNode, mPort);
            caller = new Components.Caller(cNode.Name, cPort);
            InitializeComponent();
            refresher = new Thread(RefreshTextBox);
            refresher.Start();
            caller.Init(cNode.Id);
        }

        private void RefreshTextBox()
        {
            while (true)
            {
                Thread.Sleep(50);
                messageTextBox.Text = sink.Receiver.Buffer;
                callerMessageTextBox.Text = caller.Message;
            }
        }
    }
}