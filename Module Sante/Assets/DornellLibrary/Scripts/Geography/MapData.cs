using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dornell.Geography {
    public class MapData {
        public class Node {
            public long id;
            public float lat, lon;
            public float altitude;
        }

        public class Way {
            public long id;
            public float width;
            public List<Node> nodes;
        }

        public class Building {
            public long id;
            public float height;
            public List<Node> nodes;
        }

        public Dictionary<long, Node> nodes = new Dictionary<long, Node>();
        public List<Way> ways = new List<Way>();
        public List<Building> buildings = new List<Building>();
        public HashSet<Node> trees = new HashSet<Node>();
        public HeightMap heightMap;
        public float latMin, latMax, lonMin, lonMax;

        private static readonly UInt32 FileMarker = 0x_4D_61_70_44; // 'MapD'
        private static readonly UInt16 CurrentFileVersion = 1;

        public bool LoadFromFile(string filename) {
            try {
                using (FileStream fs = new FileStream(filename, FileMode.Open)) {
                    using (BinaryReader br = new BinaryReader(fs)) {
                        UInt32 marker = br.ReadUInt32();
                        if (marker != FileMarker) {
                            Console.WriteLine($"Couldn't load {filename}: Invalid file marker.");
                            return false;
                        }
                        UInt16 version = br.ReadUInt16();
                        if (version != CurrentFileVersion) {
                            Console.WriteLine($"Couldn't load {filename}: Invalid version.");
                            return false;
                        }
                        int nNodes = br.ReadInt32();
                        nodes = new Dictionary<long, Node>(nNodes);
                        for (int i = 0; i < nNodes; ++i) {
                            long id = br.ReadInt64();
                            float lat = br.ReadSingle();
                            float lon = br.ReadSingle();
                            float altitude = br.ReadSingle();
                            nodes.Add(id, new Node { id = id, lat = lat, lon = lon, altitude = altitude });
                        }
                        int nWays = br.ReadInt32();
                        ways = new List<Way>(nWays);
                        for (int i = 0; i < nWays; ++i) {
                            long id = br.ReadInt64();
                            float width = br.ReadSingle();
                            nNodes = br.ReadInt32();
                            List<Node> nodes = new List<Node>(nNodes);
                            for (int j = 0; j < nNodes; ++j) {
                                nodes.Add(this.nodes[br.ReadInt64()]);
                            }
                            ways.Add(new Way { id = id, width = width, nodes = nodes });
                        }
                        int nBuildings = br.ReadInt32();
                        buildings = new List<Building>(nBuildings);
                        for (int i = 0; i < nBuildings; ++i) {
                            long id = br.ReadInt64();
                            float height = br.ReadSingle();
                            nNodes = br.ReadInt32();
                            List<Node> nodes = new List<Node>(nNodes);
                            for (int j = 0; j < nNodes; ++j) {
                                nodes.Add(this.nodes[br.ReadInt64()]);
                            }
                            buildings.Add(new Building{ id = id, height = height, nodes = nodes });
                        }
                        int nTrees = br.ReadInt32();
#if NETSTANDARD2_1
                        trees = new HashSet<Node>(nTrees);
#else
                        trees = new HashSet<Node>();
#endif
                        for (int i = 0; i < nTrees; ++i) {
                            trees.Add(this.nodes[br.ReadInt64()]);
                        }
                        if (br.ReadByte() == 1) {
                            heightMap = new HeightMap();
                            heightMap.Load(br);
                        } else {
                            heightMap = null;
                        }
                    }
                    ComputeBoundings();
                    return true;
                }
            } catch (Exception ex) {
                Console.WriteLine($"Error loading file {filename} : {ex.Message}");
                return false;
            }
        }

        public bool SaveToFile(string filename) {
            try {
                using (FileStream fs = new FileStream(filename, FileMode.Create)) {
                    using (BinaryWriter bw = new BinaryWriter(fs)) {
                        bw.Write(FileMarker);
                        bw.Write(CurrentFileVersion);
                        bw.Write(nodes.Count);
                        foreach (Node node in nodes.Values) {
                            bw.Write(node.id);
                            bw.Write(node.lat);
                            bw.Write(node.lon);
                            bw.Write(node.altitude);
                        }
                        bw.Write(ways.Count);
                        foreach (Way way in ways) {
                            bw.Write(way.id);
                            bw.Write(way.width);
                            bw.Write(way.nodes.Count);
                            foreach (Node node in way.nodes) {
                                bw.Write(node.id);
                            }
                        }
                        bw.Write(buildings.Count);
                        foreach (Building building in buildings) {
                            bw.Write(building.id);
                            bw.Write(building.height);
                            bw.Write(building.nodes.Count);
                            foreach (Node node in building.nodes) {
                                bw.Write(node.id);
                            }
                        }
                        bw.Write(trees.Count);
                        foreach(Node tree in trees) {
                            bw.Write(tree.id);
                        }
                        if (heightMap == null) {
                            bw.Write((byte)0);
                        } else {
                            bw.Write((byte)1);
                            heightMap.Save(bw);
                        }
                    }
                    return true;
                }
            } catch (Exception ex) {
                Console.WriteLine($"Error saving to file {filename} : {ex.Message}");
                return false;
            }
        }

        public void ComputeBoundings() {
            latMin = latMax = lonMin = lonMax = 0;
            bool first = true;

            foreach (MapData.Node node in nodes.Values) {
                if (first) {
                    latMin = latMax = node.lat;
                    lonMin = lonMax = node.lon;
                    first = false;
                } else {
                    if (node.lat < latMin)
                        latMin = node.lat;
                    else if (node.lat > latMax)
                        latMax = node.lat;
                    if (node.lon < lonMin)
                        lonMin = node.lon;
                    else if (node.lon > lonMax)
                        lonMax = node.lon;
                }
            }
        }

        public void CheckOrphanNodes() {
            HashSet<Node> usedNodes = new HashSet<Node>(trees);
            usedNodes.UnionWith(ways.SelectMany(w => w.nodes));
            usedNodes.UnionWith(buildings.SelectMany(b => b.nodes));

            HashSet<Node> orphanNodes = new HashSet<Node>(nodes.Values);
            orphanNodes.ExceptWith(usedNodes);

            if (orphanNodes.Count > 0) {
                Console.WriteLine($"Found {orphanNodes.Count} orphan nodes in MapData, removing them");
                nodes = usedNodes.ToDictionary(n => n.id);
            }
        }
    }
}
