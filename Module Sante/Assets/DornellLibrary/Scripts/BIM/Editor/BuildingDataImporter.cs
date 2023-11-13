using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if UNITY_2019  // shall be NETSTANDARD2_1 but it seems it's not defined in editor scripts...
using UnityEditor.Experimental.AssetImporters;
#else
using UnityEditor.AssetImporters;
#endif

namespace Dornell.BIM {
    [ScriptedImporter(1, "buildingData")]
    public class BuildingDataImporter : ScriptedImporter {
        private Dictionary<Int32, Material> buildingMaterials;
        private HashSet<string> nodeNames;
        private Shader standardShader;
        private AssetImportContext assetImportContext;
        private string locationTag = null;

        public override void OnImportAsset(AssetImportContext ctx) {
            assetImportContext = ctx;
            BuildingData buildingData = new BuildingData();
            if (buildingData.LoadFromFile(assetImportContext.assetPath)) {
                standardShader = Shader.Find("Standard");

                // create materials
                buildingMaterials = new Dictionary<int, Material>();
                foreach (BuildingData.Material material in buildingData.materials) {
                    buildingMaterials.Add(material.id, CreateMaterial(material.id, material.name, new Color(material.r, material.g, material.b, material.a)));
                }

                // create nodes
                nodeNames = new HashSet<string>();
                string name = System.IO.Path.GetFileNameWithoutExtension(assetImportContext.assetPath);
                GameObject root = new GameObject(name);
                CreateNode(buildingData.rootNode, root.transform);
                ctx.AddObjectToAsset(name, root);
                ctx.SetMainObject(root);
            } else {
                Debug.LogError($"Unable to load building data from {assetImportContext.assetPath}");
            }
        }

        private Material CreateMaterial(int id, string name, Color color) {
            Material material = new Material(standardShader);
            material.name = name;
            material.color = color;
            if (color.a < 1) {
                material.SetFloat("_Mode", 3);  // 3 is supposed to be BlendMode.Transparent
                                                // code taken from StandardShaderGUI.cs, see https://docs.unity3d.com/2021.3/Documentation/Manual/StandardShaderMaterialParameterRenderingMode.html
                                                // upgrading unity may require changes in this part
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0.0f);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                if (material.renderQueue < (int)UnityEngine.Rendering.RenderQueue.GeometryLast + 1 || material.renderQueue > (int)UnityEngine.Rendering.RenderQueue.Overlay - 1) {
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                }
            }
            assetImportContext.AddObjectToAsset($"Mat{id}", material);
            return material;
        }

        private void CreateNode(BuildingData.Node node, Transform parentTransform) {
            GameObject goNode = new GameObject();
            if (node.mesh != null) {
                // regular node : create mesh and required compononents (renderer, filter...)
                Material[] materials = new Material[node.mesh.submeshes.Length];
                Mesh mesh = new Mesh();
                mesh.subMeshCount = node.mesh.submeshes.Length;
                mesh.SetVertices(node.mesh.vertices);
                if (BuildingData.includeNormals)
                    mesh.SetNormals(node.mesh.normals);

                UInt16 submeshStart = 0;
                int submeshIndex = 0;
                foreach (var submesh in node.mesh.submeshes) {
                    if (!buildingMaterials.TryGetValue(submesh.material, out materials[submeshIndex])) {
                        Material material = CreateMaterial(0, "DefaultMaterial", new Color(0.5f, 0.5f, 0.5f, 1));
                        buildingMaterials.Add(0, material);
                        materials[submeshIndex] = material;
                    }
                    mesh.SetTriangles(node.mesh.indices, submeshStart, submesh.nIndices, submeshIndex);
                    submeshStart += submesh.nIndices;
                    ++submeshIndex;
                }

                goNode.AddComponent<MeshRenderer>().sharedMaterials = materials;
                if (!BuildingData.includeNormals)
                    mesh.RecalculateNormals();
                mesh.RecalculateTangents();
                goNode.AddComponent<MeshFilter>().mesh = mesh;
                assetImportContext.AddObjectToAsset($"Mesh{node.guid}", mesh);
                GameObjectUtility.SetStaticEditorFlags(goNode, StaticEditorFlags.NavigationStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.BatchingStatic);
            } else if (node.center.HasValue) {
                // space node : add a subnode with its location
                GameObject centerObject = new GameObject(node.name);
                centerObject.transform.SetParent(goNode.transform, false);
                centerObject.transform.localPosition = node.center.Value;
                if (locationTag == null) {
                    GetLocationTag();
                }
                centerObject.tag = locationTag;
            }

            if (nodeNames.Contains(node.name)) {
                int i = 2;
                string altName;
                do {
                    altName = $"{node.name}_({i++})";
                } while (nodeNames.Contains(altName));
                goNode.name = altName;
            } else {
                goNode.name = node.name; //$"{node.name}({node.guid})";
            }
            goNode.transform.position = node.translation;
            goNode.transform.rotation = node.rotation;
            goNode.transform.SetParent(parentTransform, true);
            assetImportContext.AddObjectToAsset(node.guid, goNode);
            nodeNames.Add(goNode.name);

            // create subnodes
            foreach (BuildingData.Node subnode in node.children) {
                CreateNode(subnode, goNode.transform);
            }
        }

        private bool GetLocationTag() {
            string expectedLocationTag = Locations.LocationTag;
            UnityEngine.Object tagManager = AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset");
            if (!tagManager)
                return false;

            SerializedObject serializedObject = new SerializedObject(tagManager);
            SerializedProperty tags = serializedObject.FindProperty("tags");
            if (!tags.isArray)
                return false;

            // check whether tag already exists
            int nTags = tags.arraySize;
            for (int i = 0; i < nTags; ++i) {
                SerializedProperty tag = tags.GetArrayElementAtIndex(i);
                if (expectedLocationTag.Equals(tag.stringValue)) {
                    locationTag = tag.stringValue;
                    return true;
                }
            }

            // location tag is missing : add it
            tags.InsertArrayElementAtIndex(nTags);
            tags.GetArrayElementAtIndex(nTags).stringValue = expectedLocationTag;

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            locationTag = expectedLocationTag;
            return true;
        }

    }
}