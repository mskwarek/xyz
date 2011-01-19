﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace AtmSim.Components
{
    public class SourceAgent : IAgent
    {
        //ArrayList informacionlist = new ArrayList();

        Source node;
        Configuration config;
        Socket managerSocket;
        byte[] buffer = new byte[4086];

        public SourceAgent(Source n, int port)
        {
            node = n;
            config = new Configuration(n.Name);
            Configuration pOut = new Configuration("0");
            pOut.Add("Open");
            pOut.Add("Connected");
            pOut.Add("_port");
            Configuration psOut = new Configuration("PortsOut");
            psOut.Add(pOut);
            config.Add("ID");
            config.Add("Name");
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
            if (command[0] == "ping")
            {
                response += "pong";
            }
            else if (command[0] == "get")
            {
                if (command.Length != 2)
                    return response;
                if (command[1] == "config")
                    return Serial.SerializeObject(config);
                //if (command[1] == "routing")
                //    return Serial.SerializeObject(GetRoutingTable());
                if (command[1] == "log")
                    return Serial.SerializeObject(node.Log);
                response += "getresp " + command[1];
                string[] param = command[1].Split('.');
                if (param[0] == "type")
                    response += " Source";
                if (param[0] == "ID")
                    response += " " + node.Id;
                else if (param[0] == "Name")
                    response += " " + node.Name;
                else if (param[0] == "PortsOut")
                {
                    if (param.Length != 3)
                        return response;
                    if (param[2] == "Open")
                        response += " " + node.PortOut.Open;
                    else if (param[2] == "Connected")
                        response += " " + node.PortOut.Connected;
                    else if (param[2] == "_port")
                        response += " " + node.PortOut.TcpPort;
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
                    response += " " + node.Id; // parametr niezmienny
                else if (param[0] == "Name")
                {
                    node.Name = command[2];
                    response += " " + node.Name;
                }
                else if (param[0] == "PortOut")
                {
                    if (param.Length != 2)
                        return response;
                    if (param[2] == "Open")
                        response += " " + node.PortOut.Open; // póki co niezmienne
                    else if (param[2] == "Connected")
                    {
                        if (command[2] == "True")
                            node.PortOut.Connect();
                        response += " " + node.PortOut.Connected;
                    }
                    else if (param[2] == "_port")
                    {
                        try
                        {
                            node.PortOut.TcpPort = Int32.Parse(command[2]);
                        }
                        catch (ArgumentNullException) { return ""; }
                        response += " " + node.PortOut.TcpPort;
                    }
                    else return response;
                }
            }
            //else if (command[0] == "rtadd")
            //{
            //    if (command.Length != 3)
            //        return response;
            //    response += "rtaddresp " + command[1] + " " + command[2];
            //    try
            //    {
            //        node.Matrix.RoutingTable.Add(new RoutingEntry(command[1]), new RoutingEntry(command[2]));
            //    }
            //    catch (ArgumentException)
            //    {
            //        response += " fail";
            //        return response;
            //    }
            //    response += " ok";

            //}
            //else if (command[0] == "rtdel")
            //{
            //    if (command.Length != 2)
            //        return response;
            //    response += "rtdelresp " + command[1];
            //    if (node.Matrix.RoutingTable.Remove(new RoutingEntry(command[1])))
            //        response += " ok";
            //    else
            //        response += " fail";
            //}
            return response;
        }


        public string[] GetParamList()
        {
            string[] param = { "name","message","target" } ;
            return param;
        }

        public string GetParam(string name)
        {
            if (name == "name")
                return node.Name;
            else if (name == "message")
                return node.Message;
            else if (name == "target")
                return node.Target;
            else
                return "";
        }

        public void SetParam(string name, string value)
        {
            if (name == "name")
                node.Name = value;
            else if (name == "message")
                if (value == "random") { node.Message = null; }
                else { node.Message = value; }
            else if (name == "send")
                node.Send();
            else if (name == "target")
                node.Target = value;

        }

        public Routing GetRoutingTable()
        {
            Routing table = new Routing();
            foreach (var element in node.Matrix)
            {
                table.Add(element.Key, element.Value.ToString());
            }
            return table;
        }

        public void AddRoutingEntry(string label, string value)
        {
            if (!node.Matrix.ContainsKey(label))
                node.Matrix.Add(label, new RoutingEntry(value));
        }

        public void RemoveRoutingEntry(string entry)
        {
            if (node.Matrix.ContainsKey(entry))
                node.Matrix.Remove(entry);
        }
  
        public string GetLog()
        {
            return node.Log.ToString();
        }
    }
}
