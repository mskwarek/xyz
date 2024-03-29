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
    public partial class ConfigGUI : Form
    {
        private int id;
        private string elementName;
        private Manager manager;
        private TreeNode configuration;
        private Routing localRouting;
        // zmodyfikowane wpisy
        private Dictionary<string,string> modifiedConfig;
        private List<string> addedRouting;
        private List<string> removedRouting;
        private string Selected 
        {
            get
            {
                return configTree.SelectedNode.FullPath;
            }
        }

        public ConfigGUI(Manager manager, int id)
        {
            this.manager = manager;
            this.id = id;
            this.elementName = manager.Get(this.id, "Name");
            this.configuration = getTree(manager.GetConfig(this.id));
            this.localRouting = new Routing(manager.GetRouting(this.id));
            this.modifiedConfig = new Dictionary<string, string>();
            this.addedRouting = new List<string>();
            this.removedRouting = new List<string>();
            InitializeComponent();
            this.Text += " " + this.elementName;
            foreach (TreeNode node in configuration.Nodes)
                this.configTree.Nodes.Add(node);
            this.configTree.PathSeparator = ".";
            this.routingPropertyGrid.SelectedObject = new DictionaryPropertyGridAdapter(this.localRouting);
        }

        private TreeNode getTree(C configuration)
        {
            TreeNode treeNode = new TreeNode(configuration.N);
            treeNode.Name = configuration.N;
            foreach (C node in configuration.S)
            {
                treeNode.Nodes.Add(getTree(node));
            }
            return treeNode;
        }

        private void restoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.configTabControl.SelectedIndex == this.generalTab.TabIndex)
            {
                this.modifiedConfig.Clear();
            }
            else if (this.configTabControl.SelectedIndex == this.routingTab.TabIndex)
            {
                this.localRouting = new Routing(manager.GetRouting(this.id));
                this.routingPropertyGrid.SelectedObject = new DictionaryPropertyGridAdapter(this.localRouting);
                this.routingPropertyGrid.Refresh();
                this.addedRouting.Clear();
                this.removedRouting.Clear();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.configTabControl.SelectedIndex == this.generalTab.TabIndex)
                saveConfig();
            else if (this.configTabControl.SelectedIndex == this.routingTab.TabIndex)
                saveRouting();
        }

        private void ConfigGUI_FormClosing(object sender, EventArgs e)
        {
            if (addedRouting.Count > 0 || removedRouting.Count > 0)
            {
                DialogResult dlg = MessageBox.Show("Zapisać ustawienia routingu?", "Konfiguracja", MessageBoxButtons.YesNo);
                if (dlg == DialogResult.Yes)
                    saveRouting();
            }
            if (modifiedConfig.Count > 0)
            {
                DialogResult dlg = MessageBox.Show("Zapisać konfigurację?", "Konfiguracja", MessageBoxButtons.YesNo);
                if (dlg == DialogResult.Yes)
                    saveConfig();
            }
        }

        private void configTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (modifiedConfig.ContainsKey(Selected))
                configTextBox.Text = modifiedConfig[Selected];
            else
                configTextBox.Text = manager.Get(id, Selected);

            if (configTextBox.Text == "")
            {
                configTextBox.Enabled = false;
                okButton.Enabled = false;
            }
            else
            {
                configTextBox.Enabled = true;
                okButton.Enabled = true;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (modifiedConfig.ContainsKey(Selected))
                modifiedConfig[Selected] = configTextBox.Text;
            else
                modifiedConfig.Add(Selected, configTextBox.Text);
        }

        private void saveConfig()
        {
            foreach (string param in modifiedConfig.Keys)
            {
                manager.Set(id, param, modifiedConfig[param]);
            }
            modifiedConfig.Clear();
        }

        private void saveRouting()
        {
            foreach (string label in removedRouting)
                manager.RemoveRouting(this.id, label);
            foreach (string label in addedRouting)
                manager.AddRouting(this.id, label, localRouting[label]);
            addedRouting.Clear();
            removedRouting.Clear();
        }

        private void addRoutingEntryButton_Click(object sender, EventArgs e)
        {
            AddEntryPrompt prompt = new AddEntryPrompt(this);
            prompt.Show();
            this.Hide();
        }

        public void AddRoutingEntry(string label, string value)
        {
            localRouting.Add(label, value);
            this.addedRouting.Add(label);
            routingPropertyGrid.Refresh();
        }

        private void removeRoutingEntryButton_Click(object sender, EventArgs e)
        {
            string selectedEntry = routingPropertyGrid.SelectedGridItem.Label;
            if (localRouting.ContainsKey(selectedEntry))
            {
                localRouting.Remove(selectedEntry);
                removedRouting.Add(selectedEntry);
                if (addedRouting.Contains(selectedEntry))
                    addedRouting.Remove(selectedEntry);
            }
            routingPropertyGrid.Refresh();
        }
    }
}
