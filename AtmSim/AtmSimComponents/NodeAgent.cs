﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace AtmSim.Components
{
    public class NodeAgent : IAgent
    {
        // Referencja wezla zarzadzanego przez agenta
        Node node;
        // Zawartosc konfiguracji wezla
        Configuration config;
        Socket managerSocket;
        byte[] buffer = new byte[4086];

        public NodeAgent(Node n, int port)
        {
            node = n;
            config = new Configuration(n.Name);
            Configuration psIn = new Configuration("PortsIn");
            for (int i = 0; i < node.PortsIn.Length; i++)
            {
                Configuration pIn = new Configuration(i.ToString());
                pIn.Add("Open");
                pIn.Add("Connected");
                pIn.Add("_port");
                psIn.Add(pIn);
            }
            Configuration psOut = new Configuration("PortsIn");
            for (int i = 0; i < node.PortsOut.Length; i++)
            {
                Configuration pOut = new Configuration(i.ToString());
                pOut.Add("Open");
                pOut.Add("Connected");
                pOut.Add("_port");
                psOut.Add(pOut);
            }
            config.Add("ID");
            config.Add("Name");
            config.Add(psIn);
            config.Add(psOut);

            managerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint server = new IPEndPoint(IPAddress.Loopback, port);
            managerSocket.Connect(server);
            managerSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnDataReceived, managerSocket);
        }

        private void OnDataReceived(IAsyncResult asyn)
        {
            int recv = managerSocket.EndReceive(asyn);
            if (recv == 0)
            {
                managerSocket.Close();
                return;
            }
            string query = Encoding.ASCII.GetString(buffer, 0, recv);
            string response = ProcessQuery(query);
            if (response != "")
                managerSocket.Send(System.Text.Encoding.ASCII.GetBytes(response));
            managerSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnDataReceived, managerSocket);
        }

        private string ProcessQuery(string query)
        {
            string[] command = query.Split(' ');
            string response = "";
            if (command[0] == "get")
            {
                if (command.Length != 2)
                    return response;
                if (command[1] == "config")
                    return Serial.SerializeObject(config);
                if (command[1] == "routing")
                    return Serial.SerializeObject(GetRoutingTable());
                if (command[1] == "log")
                    return Serial.SerializeObject(node.Log);
                response += "getresp " + command[1];
                string[] param = command[1].Split('.');
                if (param[0] == "ID")
                    response += " " + node.Id;
                else if (param[0] == "Name")
                    response += " " + node.Name;
                else if (param[0] == "PortsIn")
                {
                    if (param.Length != 3)
                        return response;
                    int n;
                    try { n = Int32.Parse(param[1]); }
                    catch (ArgumentNullException) { return response; }
                    if (n >= node.PortsIn.Length)
                        return response;
                    if (param[2] == "Open")
                        response += " " + node.PortsIn[n].Open;
                    else if (param[2] == "Connected")
                        response += " " + node.PortsIn[n].Connected;
                    else if (param[2] == "_port")
                        response += " " + node.PortsIn[n].TcpPort;
                    else return response;
                }
                else if (param[0] == "PortsOut")
                {
                    if (param.Length != 3)
                        return response;
                    int n;
                    try { n = Int32.Parse(param[1]); }
                    catch (ArgumentNullException) { return response; }
                    if (n >= node.PortsOut.Length)
                        return response;
                    if (param[2] == "Open")
                        response += " " + node.PortsOut[n].Open;
                    else if (param[2] == "Connected")
                        response += " " + node.PortsOut[n].Connected;
                    else if (param[2] == "_port")
                        response += " " + node.PortsOut[n].TcpPort;
                    else return response;
                }
            }
            else if (command[0] == "set")
            {
                if (command.Length != 3)
                    return response;
                response += "setresp " + command[1];
                string[] param = command[1].Split('.');
                if (param[0] == "ID")
                    response += " X"; // parametr niezmienny
                else if (param[0] == "Name")
                {
                    node.Name = command[2];
                    response += " " + node.Name;
                }
                else if (param[0] == "PortsIn")
                {
                    if (param.Length != 3)
                        return response;
                    int n;
                    try { n = Int32.Parse(param[1]); }
                    catch (ArgumentNullException) { return response; }
                    if (n >= node.PortsIn.Length)
                        return response;
                    if (param[2] == "Open")
                        response += " " + node.PortsIn[n].Open; // póki co niezmienne
                    else if (param[2] == "Connected")
                        response += " " + node.PortsIn[n].Connected; // niezmienne
                    else if (param[2] == "_port")
                        response += " " + node.PortsIn[n].TcpPort; // niezmienne
                    else return response;
                }
                else if (param[0] == "PortsOut")
                {
                    if (param.Length != 3)
                        return response;
                    int n;
                    try { n = Int32.Parse(param[1]); }
                    catch (ArgumentNullException) { return response; }
                    if (n >= node.PortsOut.Length)
                        return response;
                    if (param[2] == "Open")
                        response += " " + node.PortsOut[n].Open; // póki co niezmienne
                    else if (param[2] == "Connected")
                    {
                        if (command[2] == "1")
                            node.PortsOut[n].Connect();
                        response += " " + node.PortsOut[n].Connected;
                    }
                    else if (param[2] == "_port")
                    {
                        try
                        {
                            node.PortsOut[n].TcpPort = Int32.Parse(command[2]);
                        }
                        catch (ArgumentNullException) { return ""; }
                        response += " " + node.PortsOut[n].TcpPort;
                    }
                    else return response;
                }
            }
            else if (command[0] == "rtadd")
            {
                if (command.Length != 3)
                    return response;
                response += "rtaddresp " + command[1] + " " + command[2];
                try
                {
                    node.Matrix.RoutingTable.Add(new RoutingEntry(command[1]), new RoutingEntry(command[2]));
                }
                catch (ArgumentException)
                {
                    response += " fail";
                    return response;
                }
                response += " ok";

            }
            else if (command[0] == "rtdel")
            {
                if (command.Length != 2)
                    return response;
                response += "rtdelresp " + command[1];
                if (node.Matrix.RoutingTable.Remove(new RoutingEntry(command[1])))
                    response += " ok";
                else
                    response += " fail";
            }
            return response;
        }

        public string[] GetParamList()
        {
            string[] param = { "name", "portsIn", "portsOut" };
            return param;
        }

        public string GetParam(string name)
        {
            if (name == "name")
                return node.Name;
            else if (name == "portsIn")
                return "0-" + (node.PortsIn.Length - 1);
            else if (name == "portsOut")
                return "0-" + (node.PortsOut.Length - 1);
            else return "";
        }

        public void SetParam(string name, string value)
        {
            if (name == "name")
                node.Name = value;
        }

        public Routing GetRoutingTable()
        {
            Routing table = new Routing();
            foreach (var element in node.Matrix.RoutingTable)
            {
                table.Add(element.Key.ToString(), element.Value.ToString());
            }
            return table;
        }

        public void AddRoutingEntry(string label, string value)
        {
            node.Matrix.AddToMatrix(new RoutingEntry(label), new RoutingEntry(value));
        }

        public void RemoveRoutingEntry(string entry)
        {
            node.Matrix.DeleteFromMatrix(new RoutingEntry(entry));
        }

        public string GetLog()
        {
            return node.Log.ToString();
        }
    }
}
