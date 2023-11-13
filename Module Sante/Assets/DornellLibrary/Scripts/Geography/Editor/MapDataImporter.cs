using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
#if UNITY_2019  // shall be NETSTANDARD2_1 but it seems it's not defined in editor scripts...
using UnityEditor.Experimental.AssetImporters;
#else
using UnityEditor.AssetImporters;
#endif

namespace Dornell.Geography {

    [ScriptedImporter(1, "mapData")]
    public class MapDataImporter : ScriptedImporter {
        private AssetImportContext assetImportContext;
        private Lambert93 lambert93;

        private IGeodeticFrame geodeticFrame;


        private T GetAsset<T>(string name) where T:class {
            foreach(string guid in AssetDatabase.FindAssets(name)) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.LoadMainAssetAtPath(path) is T result)
                    return result;
            }
            return null;
        }

        private (T, string) GetAssetAndPath<T>(string name) where T : class {
            foreach (string guid in AssetDatabase.FindAssets(name)) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.LoadMainAssetAtPath(path) is T result)
                    return (result, path);
            }
            return (null, null);
        }

        public override void OnImportAsset(AssetImportContext ctx) {
            assetImportContext = ctx;

            MapData mapData = new MapData();
            if (mapData.LoadFromFile(assetImportContext.assetPath)) {
                //Debug.Log($"Loading mapdata, found {mapData.nodes.Count} nodes, {mapData.ways.Count} ways, {mapData.buildings.Count} buildings, {mapData.trees.Count} trees.");

                string name = System.IO.Path.GetFileNameWithoutExtension(assetImportContext.assetPath);
                GameObject mapRoot = new GameObject($"{name}_MapData");
                ctx.AddObjectToAsset(mapRoot.name, mapRoot);
                ctx.SetMainObject(mapRoot);

                lambert93 = new Lambert93();
                geodeticFrame = new GeodeticFrameLambert93((mapData.latMin + mapData.latMax) / 2, (mapData.lonMin + mapData.lonMax) / 2);

                Material groundMaterial = GetAsset<Material>($"{name}_GroundMaterial");
                CreateGround(mapData, mapRoot.transform, groundMaterial);

                if (mapData.ways.Count > 0) {
                    GameObject roads = new GameObject();
                    roads.transform.SetParent(mapRoot.transform, true);
                    roads.name = "Roads";
                    assetImportContext.AddObjectToAsset(roads.name, roads);
                    string roadMaterialName = $"{name}_RoadMaterial";
                    Material roadMaterial = GetAsset<Material>(roadMaterialName);
                    if (roadMaterial == null) {
                        Debug.LogError($"Unable to find road material {roadMaterialName}");
                    }

                    foreach (MapData.Way way in mapData.ways) {
                        CreateRoad(way, roads.transform, roadMaterial);
                    }
                }

                if (mapData.trees.Count > 0) {
                    GameObject trees = new GameObject();
                    trees.transform.SetParent(mapRoot.transform, true);
                    trees.name = "Trees";
                    string treeModelName = $"{name}_Tree";
                    (GameObject treePrefab, string treePrefabPath) = GetAssetAndPath<GameObject>(treeModelName);
                    if (treePrefab != null) {
                        assetImportContext.DependsOnSourceAsset(treePrefabPath);
                        assetImportContext.AddObjectToAsset(trees.name, trees);
                        foreach (MapData.Node node in mapData.trees) {
                            CreateTree(node, trees.transform, treePrefab);
                        }
                    } else {
                        Debug.LogError($"Unable to find tree model {treeModelName}");
                    }
                }

                if (mapData.buildings.Count > 0) {
                    GameObject buildings = new GameObject();
                    buildings.transform.SetParent(mapRoot.transform, true);
                    buildings.name = "Buildings";
                    string buildingMaterialName = $"{name}_BuildingMaterial";
                    assetImportContext.AddObjectToAsset(buildings.name, buildings);
                    Material buildingMaterial = GetAsset<Material>(buildingMaterialName);
                    if (buildingMaterial == null) {
                        Debug.LogError($"Unable to find building material model {buildingMaterialName}");
                    }

                    foreach (MapData.Building building in mapData.buildings) {
                        CreateBuilding(building, buildings.transform, buildingMaterial);
                    }
                }
            } else {
                Debug.LogError($"Unable to load map data from {assetImportContext.assetPath}");
            }
        }

        private bool CreateGround(MapData mapData, Transform parentTransform, Material groundMaterial) {
            if (!(mapData?.heightMap != null)) {
                Debug.Log("Can't create ground : No heightmap !");
                return false;
            }
            try {
                GameObject ground = new GameObject();
                ground.transform.SetParent(parentTransform, true);
                ground.name = "Ground";
                assetImportContext.AddObjectToAsset(ground.name, ground);

                (double xMin, double yMin) = lambert93.GeographicToCartesian(mapData.latMin, mapData.lonMin);
                (double xMax, double yMax) = lambert93.GeographicToCartesian(mapData.latMax, mapData.lonMax);
                (double xRef, double yRef) = lambert93.GeographicToCartesian((mapData.latMin + mapData.latMax) / 2, (mapData.lonMin + mapData.lonMax) / 2);

                int x0 = Convert.ToInt32(xMin);
                int x1 = Convert.ToInt32(xMax);
                int y0 = Convert.ToInt32(yMin);
                int y1 = Convert.ToInt32(yMax);

                // create ground, tile by tile
                int maxTileWidth = mapData.heightMap.width;
                int maxTileHeight = mapData.heightMap.height;
                while (maxTileWidth > 100)
                    maxTileWidth >>= 2;
                while (maxTileHeight > 100)
                    maxTileHeight >>= 2;
                for (int ty = y0; ty < y1; ty += maxTileHeight) {
                    int ty1 = Math.Min(y1, ty + maxTileHeight);
                    int tileHeight = ty1 - ty + 1;
                    for (int tx = x0; tx < x1; tx += maxTileWidth) {
                        int tx1 = Math.Min(x1, tx + maxTileWidth);
                        int tileWidth = tx1 - tx + 1;

                        // create tile
                        int nVertices = tileWidth * tileHeight;
                        Vector3[] vertices = new Vector3[nVertices];
                        //Vector3[] normals = new Vector3[nVertices];
                        Vector2[] uvs = new Vector2[nVertices];
                        int[] indices = new int[(tileWidth - 1) * (tileHeight - 1) * 6];

                        // create a new mesh representing the ground
                        Mesh mesh = new Mesh();
                        int index = 0;
                        for (int y = ty; y <= ty1; ++y) {
                            for (int x = tx; x <= tx1; ++x) {
                                if (!mapData.heightMap.TryGetAltitude(x, y, out float altitude)) {
                                    Debug.LogError($"Unavailable altitude : {x}, {y}");
                                    return false;
                                }
                                vertices[index] = new Vector3(Convert.ToSingle(x - xRef), altitude, Convert.ToSingle(y - yRef));
                                uvs[index++] = new Vector2((x - tx), (y - ty));
                            }
                        }

                        index = 0;
                        for (int y = 0; y < tileHeight - 1; ++y) {
                            for (int x = 0; x < tileWidth - 1; ++x) {
                                indices[index++] = y * tileWidth + x;
                                indices[index++] = (y + 1) * tileWidth + x;
                                indices[index++] = (y + 1) * tileWidth + x + 1;
                                indices[index++] = y * tileWidth + x;
                                indices[index++] = (y + 1) * tileWidth + x + 1;
                                indices[index++] = y * tileWidth + x + 1;
                            }
                        }

                        mesh.SetVertices(vertices);
                        //            mesh.SetNormals(normals);
                        mesh.SetUVs(0, uvs);
                        mesh.SetTriangles(indices, 0);
                        mesh.RecalculateBounds();
                        mesh.RecalculateNormals();
                        mesh.RecalculateTangents();

                        GameObject tile = new GameObject();
                        tile.transform.SetParent(ground.transform, true);
                        tile.name = $"Tile_{tx - x0}_{ty - y0}";
                        MeshRenderer meshRenderer = tile.AddComponent<MeshRenderer>();
                        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        meshRenderer.sharedMaterial = groundMaterial;
                        tile.AddComponent<MeshFilter>().mesh = mesh;
                        tile.AddComponent<MeshCollider>().sharedMesh = mesh;
                        assetImportContext.AddObjectToAsset($"{tile.name}_Mesh", mesh);
                        GameObjectUtility.SetStaticEditorFlags(tile, StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.BatchingStatic);
                        assetImportContext.AddObjectToAsset(tile.name, tile);
                    }
                }

                return true;
            } catch (Exception ex) {
                Debug.LogError($"Unable to create ground : {ex.Message}");
                return false;
            }
        }

        private void CreateRoad(MapData.Way way, Transform parentTransform, Material roadMaterial) {
            // create a new mesh representing the way
            Mesh mesh = new Mesh();
            Vector3[] vertices = new Vector3[way.nodes.Count * 2];
            Vector2[] uvs = new Vector2[way.nodes.Count * 2];
            int[] indices = new int[(way.nodes.Count - 1) * 6];
            float roadWidth = way.width;
            float cumulatedDistance = 0.0f;

            for (int i = 0; i < way.nodes.Count; ++i) {
                float altitude = way.nodes[i].altitude + 0.1f;
                if (i == 0) {
                    Vector2 p0 = NodeToPos(way.nodes[i]);
                    Vector2 p1 = NodeToPos(way.nodes[i + 1]);
                    Vector2 direction = (p1 - p0).normalized * roadWidth;
                    vertices[i * 2 + 0] = new Vector3(p0.x - direction.y, altitude, p0.y + direction.x);
                    vertices[i * 2 + 1] = new Vector3(p0.x + direction.y, altitude, p0.y - direction.x);
                } else if (i == way.nodes.Count - 1) {
                    Vector2 p0 = NodeToPos(way.nodes[i - 1]);
                    Vector2 p1 = NodeToPos(way.nodes[i]);
                    Vector2 direction = (p1 - p0).normalized * roadWidth;
                    vertices[i * 2 + 0] = new Vector3(p1.x - direction.y, altitude, p1.y + direction.x);
                    vertices[i * 2 + 1] = new Vector3(p1.x + direction.y, altitude, p1.y - direction.x);
                    cumulatedDistance += (p1 - p0).magnitude;
                } else {
                    Vector2 p0 = NodeToPos(way.nodes[i - 1]);
                    Vector2 p1 = NodeToPos(way.nodes[i]);
                    Vector2 p2 = NodeToPos(way.nodes[i + 1]);
                    Vector2 va = (p1 - p0).normalized * roadWidth;  // width-normalized direction vector (p0p1)
                    Vector2 vb = (p2 - p1).normalized * roadWidth;  // width-normalized direction vector (p1p2)
                    Vector2 vbt = Vector2.Perpendicular(vb);

                    if (va == vb) {
                        vertices[i * 2 + 0] = new Vector3(p1.x - va.y, altitude, p1.y + va.x);
                        vertices[i * 2 + 1] = new Vector3(p1.x + va.y, altitude, p1.y - va.x);
                    } else {
                        // given 2 lines (point A0, direction vector a) and (point B0, direction vector b), the intersection p is computed as : 
                        // s = ( bperp * (B0 - A0 ) / ( bperp * a )
                        // p = A0 + s * a
                        Vector2 pa = new Vector2(p1.x - va.y, p1.y + va.x);
                        Vector2 pb = new Vector2(p1.x - vb.y, p1.y + vb.x);
                        float s = Vector2.Dot(vbt, (pb - pa)) / Vector2.Dot(vbt, va);
                        Vector2 p = pa + s * va;
                        vertices[i * 2 + 0] = new Vector3(p.x, altitude, p.y);

                        pa = new Vector2(p1.x + va.y, p1.y - va.x);
                        pb = new Vector2(p1.x + vb.y, p1.y - vb.x);
                        s = Vector2.Dot(vbt, (pb - pa)) / Vector2.Dot(vbt, va);
                        p = pa + s * va;
                        vertices[i * 2 + 1] = new Vector3(p.x, altitude, p.y);
                    }
                    cumulatedDistance += (p1 - p0).magnitude;
                }

                uvs[i * 2 + 0] = new Vector2(cumulatedDistance, 0);
                uvs[i * 2 + 1] = new Vector2(cumulatedDistance, roadWidth);

                if (i < way.nodes.Count - 1) {
                    indices[i * 6 + 0] = i * 2 + 0;
                    indices[i * 6 + 1] = i * 2 + 2;
                    indices[i * 6 + 2] = i * 2 + 1;
                    indices[i * 6 + 3] = i * 2 + 2;
                    indices[i * 6 + 4] = i * 2 + 3;
                    indices[i * 6 + 5] = i * 2 + 1;
                }
            }
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(indices, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            GameObject road = new GameObject();
            road.transform.SetParent(parentTransform, true);
            road.name = $"Way {way.id}";
            MeshRenderer meshRenderer = road.AddComponent<MeshRenderer>();
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.sharedMaterial = roadMaterial;
            road.AddComponent<MeshFilter>().mesh = mesh;
            assetImportContext.AddObjectToAsset($"{road.name}_Mesh", mesh);
            GameObjectUtility.SetStaticEditorFlags(road, StaticEditorFlags.NavigationStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.BatchingStatic);
            assetImportContext.AddObjectToAsset(road.name, road);
        }

        private void CreateTree(MapData.Node node, Transform parentTransform, GameObject treePrefab) {
            // instantiate a road from prefab and assign its mesh to the one we just created
            Vector2 position = NodeToPos(node);
            GameObject tree = PrefabUtility.InstantiatePrefab(treePrefab) as GameObject;
            tree.transform.position = new Vector3(position.x, node.altitude, position.y);
            // todo : replace Random with some deterministic way of getting different trees
            tree.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
            float scale = UnityEngine.Random.Range(0.5f, 1.0f);
            tree.transform.localScale = new Vector3(scale, scale, scale);
            tree.transform.parent = parentTransform;
            tree.name = $"Tree {node.id}";
            assetImportContext.AddObjectToAsset(tree.name, tree);
        }

        private void CreateBuilding(MapData.Building building, Transform parentTransform, Material buildingMaterial) {
            // create a new mesh representing the way
            Mesh mesh = new Mesh();
            int nVertices = (building.nodes.Count - 1) * 4;
            Vector3[] vertices = new Vector3[nVertices];
            Vector3[] normals = new Vector3[nVertices];
            Vector2[] uvs = new Vector2[nVertices];
            int[] indices = new int[(building.nodes.Count - 1) * 6];
            float height = (building.height > 0) ? building.height : 5.0f;
            int i;

            bool flipFace = Geometry.IsPolygonClockwise(building.nodes.Select(n => NodeToPos(n)).ToList());
            int index1 = flipFace ? 2 : 1;
            int index2 = flipFace ? 1 : 2;

            float altitudeMin, altitudeMax;
            altitudeMin = altitudeMax = building.nodes[0].altitude;
            for (i = 1; i < building.nodes.Count; ++i) {
                float altitude = building.nodes[i].altitude;
                if (altitude < altitudeMin)
                    altitudeMin = altitude;
                else if (altitude > altitudeMax)
                    altitudeMax = altitude;
            }

            for (i = 0; i < building.nodes.Count - 1; ++i) {
                Vector2 p0 = NodeToPos(building.nodes[i]);
                Vector2 p1 = NodeToPos(building.nodes[i + 1]);
                float distance = (p1 - p0).magnitude;
                float altitude0 = building.nodes[i].altitude;
                float altitude1 = building.nodes[i + 1].altitude;
                vertices[i * 4 + 0] = new Vector3(p0.x, altitudeMax + height, p0.y);
                vertices[i * 4 + 1] = new Vector3(p0.x, altitudeMin, p0.y);
                vertices[i * 4 + 2] = new Vector3(p1.x, altitudeMax + height, p1.y);
                vertices[i * 4 + 3] = new Vector3(p1.x, altitudeMin, p1.y);

                Vector2 direction = p1 - p0;
                Vector3 normal = Vector3.Cross(Vector3.up, new Vector3(direction.x, 0, direction.y)).normalized;
                if (flipFace)
                    normal = -normal;
                normals[i * 4 + 0] = normal;
                normals[i * 4 + 1] = normal;
                normals[i * 4 + 2] = normal;
                normals[i * 4 + 3] = normal;

                uvs[i * 4 + 0] = new Vector2(0, height);
                uvs[i * 4 + 1] = new Vector2(0, 0);
                uvs[i * 4 + 2] = new Vector2(distance, height);
                uvs[i * 4 + 3] = new Vector2(distance, 0);

                indices[i * 6 + 0] = i * 4 + 0;
                indices[i * 6 + 1] = i * 4 + index2;
                indices[i * 6 + 2] = i * 4 + index1;
                indices[i * 6 + 3] = i * 4 + index2;
                indices[i * 6 + 4] = i * 4 + 3;
                indices[i * 6 + 5] = i * 4 + index1;
            }

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(indices, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            GameObject buildingObject = new GameObject();
            buildingObject.transform.SetParent(parentTransform, true);
            buildingObject.name = $"Building {building.id}";
            MeshRenderer meshRenderer = buildingObject.AddComponent<MeshRenderer>();
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.sharedMaterial = buildingMaterial;
            buildingObject.AddComponent<MeshFilter>().mesh = mesh;
            buildingObject.AddComponent<MeshCollider>().sharedMesh = mesh;
            assetImportContext.AddObjectToAsset($"{buildingObject.name}_Mesh", mesh);
            GameObjectUtility.SetStaticEditorFlags(buildingObject, StaticEditorFlags.NavigationStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.BatchingStatic);
            assetImportContext.AddObjectToAsset(buildingObject.name, buildingObject);
        }


        private Vector2 NodeToPos(in MapData.Node node) {
            (float x, float y) = geodeticFrame.GeographicToCartesian(node.lat, node.lon);
            return new Vector2(x, y);
        }

    }

}