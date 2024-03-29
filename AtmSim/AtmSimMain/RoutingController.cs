﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph.Algorithms.ShortestPath;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms;

namespace AtmSim
{
    public class RoutingController
    {

        private Manager manager;
        private String strDebug="";
        private Random random = new Random();
        public RoutingController(Manager manager) 
        {
            this.manager = manager;
            //musi byc kopiowany, bo jak porty beda zajete to trzeba bedzie usunac dana krawedz
            
            //MyOwnLog.save( this.findBestPath(4, 5).ToString() );
            //this.setupConnection(4, 5, 2, 2);
        }



        public class SetupStore
        {
            public int source { get; set; }
            public int target { get; set;}
            public RoutingGraph ownTopology { get; set; }
            public List<RoutingGraph.Link> path { get; set; }
            public List<string> vcivpiList { get; set; }
            public int connectN{ get; set; }
            public int requieredCapacity { get; set; }
            public SetupStore(int source, int target, RoutingGraph ownTopology, int connectN, int requieredCapacity)
            {
                this.source = source;
                this.target = target;
                this.ownTopology = ownTopology;
                this.connectN = connectN;
                path = new List<RoutingGraph.Link>();
                vcivpiList = new List<string>();
                this.requieredCapacity = requieredCapacity;
               
            }
            public SetupStore() { }
        }


        public VirtualPath setupPath(int src, int trg, int pathN, int requiredCapacity)
        {
            RoutingGraph ownTopology = RoutingGraph.MapTopology(manager.Topology, requiredCapacity);
            SetupStore ss = new SetupStore(src, trg, ownTopology, pathN, requiredCapacity);
            if (ss.ownTopology.EdgeCount != 0)
            {
                if (this.findBestPath(ss) && this.askLRMs(ss))  //if true -> creating list vcivpi
                {
                    return this.parseToVirtualPath(ss);
                }
                else
                    return null;
            }
            return null;
        }

        public NetworkConnection setupConnection(int src, int trg, int connectN, int requieredCapacity)
        {
            RoutingGraph ownTopology = RoutingGraph.MapTopology(manager.Topology, manager.VirtualPaths, requieredCapacity);
            SetupStore ss = new SetupStore(src, trg, ownTopology, connectN, requieredCapacity);
            if (ss.ownTopology.EdgeCount != 0)
            {
                if (this.findBestPath(ss) && this.askLRMs(ss))  //if true -> creating list vcivpi
                {
                    return this.parseToNetworConnection(ss);
                }
                else
                    return null;
            }
            return null;
        }

        private VirtualPath parseToVirtualPath(SetupStore ss)
        {
            VirtualPath virtualPath = new VirtualPath(ss.connectN, ss.path.First().tLink.Source, ss.path.Last().tLink.Target,
                ss.path.First().tLink.SourcePort, ss.path.Last().tLink.TargetPort,
                Int32.Parse(ss.path.First().SourceRouting.Split(':')[1]), Int32.Parse(ss.path.Last().TargetRouting.Split(':')[1]),
                ss.requieredCapacity);

            LinkConnection link;
            foreach (var e in ss.path)
            {
                link = new LinkConnection();
                link.SourceId = e.Source.Id;
                link.TargetId = e.Target.Id;
                link.SourceRouting = e.SourceRouting;
                link.TargetRouting = e.TargetRouting;
                link.Link = e.tLink;
                virtualPath.Path.Add(link);
            }
            return virtualPath;
        }

        private NetworkConnection parseToNetworConnection(SetupStore ss)
        {
            NetworkConnection networkConnection = new NetworkConnection(ss.connectN);
            //  List<LinkConnection> links = new List<LinkConnection>();
            LinkConnection link;
            networkConnection.Capacity = ss.requieredCapacity;

            foreach (var e in ss.path)
            {
                link = new LinkConnection();
                link.SourceId = e.Source.Id;
                link.TargetId = e.Target.Id;
                link.SourceRouting = e.SourceRouting;
                link.TargetRouting = e.TargetRouting;
                link.Link = e.tLink;
                networkConnection.Path.Add(link);
            }
            return networkConnection;
        }




        private RoutingGraph.Node IDtoNode(int id, RoutingGraph ownTopology)
         {

             return ownTopology.Vertices.ToList().Find(delegate(RoutingGraph.Node no)
             { 
                return no.Id == id; 
            });

        }

        private Boolean findBestPath(SetupStore ss)
        {
            //  Func<Topology.Link, double> edgeCost = e => 1; //koszty lini takie same
            Dictionary<RoutingGraph.Link, double> edgeCost = new Dictionary<RoutingGraph.Link, double>(ss.ownTopology.EdgeCount);

            int index = 0;
            int max = ss.ownTopology.EdgeCount;
            while (index < max)
            {       //free capisity < requierd
                if (ss.ownTopology.Edges.ElementAt(index).Capacity < ss.requieredCapacity)
                {
                    ss.ownTopology.RemoveEdge(ss.ownTopology.Edges.ElementAt(index));
                    max = ss.ownTopology.EdgeCount;
                }
                else
                    index++;
            }
            foreach (var e in ss.ownTopology.Edges)
            {
                edgeCost.Add(e, e.Capacity);

            }


            // We want to use Dijkstra on this graph
            var dijkstra = new DijkstraShortestPathAlgorithm<RoutingGraph.Node, RoutingGraph.Link>(ss.ownTopology, e => edgeCost[e]);

            // Attach a Vertex Predecessor Recorder Observer to give us the paths
            var predecessorObserver = new VertexPredecessorRecorderObserver<RoutingGraph.Node, RoutingGraph.Link>();
            predecessorObserver.Attach(dijkstra);
            dijkstra.Compute(this.IDtoNode(ss.source, ss.ownTopology));
            IEnumerable<RoutingGraph.Link> path;
            //List<Topology.Link> ddd = new List<Topology.Link>();
            if (predecessorObserver.TryGetPath(this.IDtoNode(ss.target, ss.ownTopology), out path))
            {
                ss.path.AddRange(path);
                return true;
            }
            else return false;
        }

        private Boolean doIHaveFreePorts(String response)
        {
            switch (response)
            {
                case "True": return true;
                case "False": return false;
                default: return false;
            }
        }

        public bool askLRMs(SetupStore ss){


            string VpiVci = "";
            if (!manager.Ping(ss.source))
                return false;
            foreach (var e in ss.path)
            {
                if (!manager.Ping(e.Target.Id))
                    return false;
                string[] srcrt;
                string[] trgrt;
                do
                {
                    srcrt = e.SourceRouting.Split(':'); // [0] -> Port, [1] -> Vpi, [2] -> Vci
                    trgrt = e.TargetRouting.Split(':');
                    if (srcrt[1] == "" && trgrt[1] == "")
                        srcrt[1] = trgrt[1] = rand();
                    if (srcrt[2] == "" && trgrt[2] == "")
                        srcrt[2] = trgrt[2] = rand();
                } while (
                    !doIHaveFreePorts(manager.Get(e.Source.Id, "PortsOut." + srcrt[0] + ".Available." + srcrt[1] + "." + srcrt[2])) ||
                    !doIHaveFreePorts(manager.Get(e.Target.Id, "PortsIn." + trgrt[0] + ".Available." + trgrt[1] + "." + trgrt[2]))
                    );
                e.SourceRouting = srcrt[0] + ":" + srcrt[1] + ":" + srcrt[2];
                e.TargetRouting = trgrt[0] + ":" + trgrt[1] + ":" + trgrt[2];
                ss.vcivpiList.Add(VpiVci);

            }
            return true;
        }

    

        private String Get(int id,  String str) //debugging
        {
            return "true";
        }


        private void addRouting(int id, String lable, String value, int idNumber){} //debugging


        private String rand(){
           
            int num = random.Next()%100;
            return Convert.ToString(num);
        }
    }
}
