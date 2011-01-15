﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AtmSim
{
    public class Configuration
    {
        private List<Configuration> nodes;
        public List<Configuration> Nodes
        { get { return nodes; } }
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public Configuration()
        {
            nodes = new List<Configuration>();
        }
        public Configuration(string n)
            : this()
        {
            name = n;
        }
    }
}
