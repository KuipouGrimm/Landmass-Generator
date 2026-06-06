using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode {NoiseMap, ColourMap, Mesh}
    public DrawMode drawMode;
    public Noise.NormalizeMode normalizeMode;

    public const int mapChunckSize = 241;

    [Range(0,6)] public int editorPreviewLOD;

    [Min(0.001f)] public float noiseScale;

    [Range(1,10)] public int octaves;
    [Range(0,1)] public float persistance;
    [Min(1)] public float lacunarity;

    [Range(0.1f,10)] public float globalNormalizeModeOffset;
    [Range(0.1f,10)] public float testIntensity;
    [Range(-0.5f,0.5f)] public float testPeaks;

    [Range(-0.4f, 0.4f)] public float terrainElevation;
    [Range(0.1f, 3f)] public float terrainRelief = 1.4f;

    public int seed;
    public Vector2 offset;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;

    public TerrainTypes[] regions;

    Queue<MapDataRequest> mapDataRequestQueue = new Queue<MapDataRequest>();
    Queue<MeshDataRequest> meshDataRequestQueue = new Queue<MeshDataRequest>();
    bool isProcessingMapData;
    bool isProcessingMeshData;

    public void DrawMapInEditor() {
        MapData mapData = GenerateMapData(Vector2.zero);

        MapDisplay display = FindFirstObjectByType<MapDisplay>();

        if (drawMode == DrawMode.NoiseMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColourMap) {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunckSize, mapChunckSize));
        }
        else if (drawMode == DrawMode.Mesh) {
            display.DrawMesh(
                MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), 
                TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunckSize, mapChunckSize)
            );
        }
    }

    public void RequestMapData(Vector2 center, Action<MapData> callback) {
        mapDataRequestQueue.Enqueue(new MapDataRequest(center, callback));

        if (!isProcessingMapData) {
            StartCoroutine(ProcessMapDataQueue());
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback) {
        meshDataRequestQueue.Enqueue(new MeshDataRequest(mapData, lod, callback));

        if (!isProcessingMeshData) {
            StartCoroutine(ProcessMeshDataQueue());
        }
    }

    IEnumerator ProcessMapDataQueue() {
        isProcessingMapData = true;

        while (mapDataRequestQueue.Count > 0) {
            MapDataRequest request = mapDataRequestQueue.Dequeue();
            MapData mapData = GenerateMapData(request.center);
            request.callback(mapData);
            yield return null;
        }

        isProcessingMapData = false;
    }

    IEnumerator ProcessMeshDataQueue() {
        isProcessingMeshData = true;

        while (meshDataRequestQueue.Count > 0) {
            MeshDataRequest request = meshDataRequestQueue.Dequeue();
            MeshData meshData = MeshGenerator.GenerateTerrainMesh(
                request.mapData.heightMap,
                meshHeightMultiplier,
                meshHeightCurve,
                request.lod
            );
            request.callback(meshData);
            yield return null;
        }

        isProcessingMeshData = false;
    }

    MapData GenerateMapData(Vector2 center) {
        float[,] noiseMap = Noise.GenerateNoiseMap(
            mapChunckSize, 
            mapChunckSize, 
            seed, 
            noiseScale, 
            octaves, 
            persistance, 
            lacunarity, 
            center + offset,
            normalizeMode,
            globalNormalizeModeOffset,
            testIntensity,
            testPeaks,
            terrainElevation,
            terrainRelief
        );

        Color[] colourMap = new Color[mapChunckSize * mapChunckSize];   
        for (int y = 0; y < mapChunckSize; y++) {
            for (int x = 0; x < mapChunckSize; x++) {
                float currentHeight = noiseMap[x,y];

                for (int i = 0; i < regions.Length; i++) {
                    if (currentHeight >= regions[i].height) {
                        colourMap[y * mapChunckSize + x] = regions[i].colour;
                    }
                    else {
                        break;
                    }
                }
            }
        }

        return new MapData(noiseMap, colourMap);
    }

    /* Achei melhor colocar as limitações das variáveis na declaração delas acima.
    public void OnValidate() {
        if (lacunarity < 1) {
            lacunarity = 1f;
        }
        if (octaves < 0) {
            octaves = 0;
        }
    }
    */

    struct MapDataRequest {
        public readonly Vector2 center;
        public readonly Action<MapData> callback;

        public MapDataRequest(Vector2 center, Action<MapData> callback) {
            this.center = center;
            this.callback = callback;
        }
    }

    struct MeshDataRequest {
        public readonly MapData mapData;
        public readonly int lod;
        public readonly Action<MeshData> callback;

        public MeshDataRequest(MapData mapData, int lod, Action<MeshData> callback) {
            this.mapData = mapData;
            this.lod = lod;
            this.callback = callback;
        }
    }

}

[System.Serializable]
public struct TerrainTypes {
    public string name;
    public float height;
    public Color colour;
}

public struct MapData {
    public readonly float[,] heightMap;
    public readonly Color[] colourMap;
    
    public MapData(float[,] heightMap, Color[] colourMap) {
        this.heightMap = heightMap;
        this.colourMap = colourMap;
    }
}