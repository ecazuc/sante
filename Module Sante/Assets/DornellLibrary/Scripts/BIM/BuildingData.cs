using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Dornell.BIM {
    public class BuildingData {
        public static readonly bool includeNormals = false;  // set to true if normals are included in BuildingData file
        public Node rootNode;
        public Material[] materials;

        private static readonly UInt32 FileMarker = 0x_42_6C_64_44; // 'BldD'
        private static readonly UInt16 CurrentFileVersion = 1;

        public class Material {
            public Int32 id;
            public string name;
            public float r, g, b, a;

            public Material Load(BinaryReader br) {
                id = br.ReadInt32();
                name = LoadString(br);
                r = br.ReadSingle();
                g = br.ReadSingle();
                b = br.ReadSingle();
                a = br.ReadSingle();
                return this;
            }

            public void Save(BinaryWriter bw) {
                bw.Write(id);
                SaveString(bw, name);
                bw.Write(r);
                bw.Write(g);
                bw.Write(b);
                bw.Write(a);
            }

            public void Save(TextWriter tw) {
                tw.WriteLine($"{id} {name} {r} {g} {b} {a}");
            }
        }

        public class Mesh {
            public Vector3[] vertices;
            public Vector3[] normals;
            public UInt16[] indices;
            public (Int32 material, UInt16 nIndices)[] submeshes;

            public Mesh Load(BinaryReader br) {
                UInt16 nVertices = br.ReadUInt16();
                vertices = new Vector3[nVertices];
                if (includeNormals)
                    normals = new Vector3[nVertices];
                for (UInt16 i = 0; i < nVertices; ++i) {
                    vertices[i] = LoadVector3(br);
                    if (includeNormals)
                        normals[i] = LoadVector3(br);
                }

                UInt16 nIndices = br.ReadUInt16();
                indices = new UInt16[nIndices];
                for (UInt16 i = 0; i < nIndices; ++i) {
                    indices[i] = br.ReadUInt16();
                }

                UInt16 nSubmeshes = br.ReadUInt16();
                submeshes = new (int material, ushort nIndices)[nSubmeshes];
                for (UInt16 i = 0; i < nSubmeshes; ++i) {
                    submeshes[i] = (br.ReadInt32(), br.ReadUInt16());
                }

                return this;
            }

            public void Save(BinaryWriter bw) {
                UInt16 count = Convert.ToUInt16(vertices.Length);
                bw.Write(count);
                for (UInt16 i = 0; i < count; ++i) {
                    SaveVector3(bw, vertices[i]);
                    if (includeNormals)
                        SaveVector3(bw, normals[i]);
                }

                count = Convert.ToUInt16(indices.Length);
                bw.Write(count);
                for (UInt16 i = 0; i < count; ++i) {
                    bw.Write(indices[i]);
                }

                count = Convert.ToUInt16(submeshes.Length);
                bw.Write(count);
                for (UInt16 i = 0; i < count; ++i) {
                    bw.Write(submeshes[i].material);
                    bw.Write(submeshes[i].nIndices);
                }
            }
            public void Save(TextWriter tw) {
                UInt16 count = Convert.ToUInt16(vertices.Length);
                tw.WriteLine(count);
                for (UInt16 i = 0; i < count; ++i) {
                    SaveVector3(tw, vertices[i]);
                    if (includeNormals)
                        SaveVector3(tw, normals[i]);
                }

                count = Convert.ToUInt16(indices.Length);
                tw.WriteLine(count);
                for (UInt16 i = 0; i < count; ++i) {
                    tw.WriteLine(indices[i]);
                }

                count = Convert.ToUInt16(submeshes.Length);
                tw.WriteLine(count);
                for (UInt16 i = 0; i < count; ++i) {
                    tw.WriteLine($"{submeshes[i].material} {submeshes[i].nIndices}");
                }
            }

        };

        public class Node {
            private const byte NodeType_Empty = 0;
            private const byte NodeType_Geometry = 1;
            private const byte NodeType_Location = 2;

            public string guid;
            public string name;
            public Vector3 translation = Vector3.zero;
            public Quaternion rotation = Quaternion.identity;
            public Mesh mesh;
            public Vector3? center;
            public List<Node> children = new List<Node>();

            public Node() {
            }

            public Node(string guid, string name) {
                this.guid = guid;
                this.name = name;
            }

            public Node Load(BinaryReader br) {
                guid = LoadString(br);
                name = LoadString(br);
                translation = LoadVector3(br);
                rotation = LoadQuaternion(br);

                byte nodeType = br.ReadByte();
                switch (nodeType) {
                    case NodeType_Geometry:
                        mesh = new Mesh().Load(br);
                        break;
                    case NodeType_Location:
                        center = LoadVector3(br);
                        break;
                }

                uint nChildren = br.ReadUInt32();
                for (uint i = 0; i < nChildren; ++i) {
                    children.Add(new Node().Load(br));
                }
                return this;
            }

            public void Save(BinaryWriter bw) {
                SaveString(bw, guid);
                SaveString(bw, name);
                SaveVector3(bw, translation);
                SaveQuaternion(bw, rotation);

                if (mesh != null) {
                    bw.Write(NodeType_Geometry);
                    mesh.Save(bw);
                } else if (center != null) {
                    bw.Write(NodeType_Location);
                    SaveVector3(bw, center.Value);
                } else {
                    bw.Write(NodeType_Empty);
                }

                bw.Write(Convert.ToUInt32(children.Count));
                foreach (Node node in children) {
                    node.Save(bw);
                }
            }

            public void Save(TextWriter tw) {
                SaveString(tw, guid);
                SaveString(tw, name);
                SaveVector3(tw, translation);
                SaveQuaternion(tw, rotation);

                if (mesh != null) {
                    tw.WriteLine(NodeType_Geometry);
                    mesh.Save(tw);
                } else if (center != null) {
                    tw.WriteLine(NodeType_Location);
                    SaveVector3(tw, center.Value);
                } else {
                    tw.WriteLine(NodeType_Empty);
                }

                tw.WriteLine(Convert.ToUInt32(children.Count));
                foreach (Node node in children) {
                    node.Save(tw);
                }
            }
        }

        public bool LoadFromFile(string filepath) {
            try {
                using (FileStream fs = new FileStream(filepath, FileMode.Open)) {
                    using (BinaryReader br = new BinaryReader(fs)) {
                        UInt32 marker = br.ReadUInt32();
                        if (marker != FileMarker) {
                            Console.WriteLine($"Couldn't load {filepath}: Invalid file marker.");
                            return false;
                        }
                        UInt16 version = br.ReadUInt16();
                        if (version != CurrentFileVersion) {
                            Console.WriteLine($"Couldn't load {filepath}: Invalid version.");
                            return false;
                        }
                        rootNode = new Node().Load(br);
                        UInt16 materialCount = br.ReadUInt16();
                        materials = new Material[materialCount];
                        for (UInt16 i = 0; i < materialCount; ++i) {
                            materials[i] = new Material().Load(br);
                        }
                    }
                    return true;
                }
            } catch (Exception ex) {
                Debug.LogError($"Error loading file {filepath} : {ex.Message}");
                return false;
            }
        }

        public bool SaveToFile(string filepath, bool binary) {
            try {
                using (FileStream fs = new FileStream(filepath, FileMode.Create)) {
                    if (binary) {
                        using (BinaryWriter bw = new BinaryWriter(fs)) {
                            bw.Write(FileMarker);
                            bw.Write(CurrentFileVersion);
                            rootNode.Save(bw);
                            UInt16 materialCount = Convert.ToUInt16(materials.Length);
                            bw.Write(materialCount);
                            for (UInt16 i = 0; i < materialCount; ++i) {
                                materials[i].Save(bw);
                            }
                            return true;
                        }
                    } else {
                        using (TextWriter tw = new StreamWriter(fs)) {
                            tw.WriteLine(FileMarker);
                            tw.WriteLine(CurrentFileVersion);
                            rootNode.Save(tw);
                            UInt16 materialCount = Convert.ToUInt16(materials.Length);
                            tw.WriteLine(materialCount);
                            for (UInt16 i = 0; i < materialCount; ++i) {
                                materials[i].Save(tw);
                            }
                            return true;
                        }
                    }
                }
            } catch (Exception ex) {
                Debug.LogError($"Error saving file {filepath} : {ex.Message}");
                return false;
            }
        }


        private static Vector3 LoadVector3(BinaryReader br) {
            return new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }
        private static void SaveVector3(BinaryWriter bw, Vector3 v) {
            bw.Write(v.x);
            bw.Write(v.y);
            bw.Write(v.z);
        }
        private static void SaveVector3(TextWriter tw, Vector3 v) {
            tw.WriteLine($"{v.x} {v.y} {v.z}");
        }

        private static Quaternion LoadQuaternion(BinaryReader br) {
            return new Quaternion(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }

        private static void SaveQuaternion(BinaryWriter bw, Quaternion q) {
            bw.Write(q.x);
            bw.Write(q.y);
            bw.Write(q.z);
            bw.Write(q.w);
        }
        private static void SaveQuaternion(TextWriter tw, Quaternion q) {
            tw.WriteLine($"{q.x} {q.y} {q.z} {q.w}");
        }

        private static string LoadString(BinaryReader br) {
            UInt16 size = br.ReadUInt16();
            if (size == 0)
                return String.Empty;
            byte[] chars = br.ReadBytes(size);
            return System.Text.Encoding.UTF8.GetString(chars);
        }

        private static void SaveString(BinaryWriter bw, string s) {
            UInt16 size = (s != null) ? Convert.ToUInt16(s.Length) : (UInt16)0;
            if (size > 0) {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(s);
                bw.Write(Convert.ToUInt16(data.Length));
                bw.Write(data);
            } else {
                bw.Write(size);
            }
        }
        private static void SaveString(TextWriter tw, string s) {
            tw.WriteLine(s);
        }

    }
}
