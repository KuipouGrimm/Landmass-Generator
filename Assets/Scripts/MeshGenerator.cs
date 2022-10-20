using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator {
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _meshHeightCurve, int levelOfDetail) {
        AnimationCurve meshHeightCurve = new AnimationCurve(_meshHeightCurve.keys);
        
        int width = heightMap.GetUpperBound(0) + 1;
        int height = heightMap.GetUpperBound(1) + 1;

        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        int meshSimplificationImcrement = levelOfDetail == 0? 1 : levelOfDetail * 2;
        int verticesPerLine = ((width - 1) / meshSimplificationImcrement) + 1;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
        int vertexIndex = 0;

        for (int y = 0; y < height; y += meshSimplificationImcrement) {
            for (int x = 0; x < width; x += meshSimplificationImcrement) {
                
                //meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightMap[x,y], topLeftZ + y);
                //meshData.uvs[vertexIndex] = new Vector2(x/(float)width, y/(float)height); //ficou deslocado do centro e invertido em 180 graus
                float halfWidth = (width - 1) / 2f;
                float halfHeight = (height - 1) / 2f;
                meshData.vertices[vertexIndex] = new Vector3(x - halfWidth, meshHeightCurve.Evaluate(heightMap[x,y]) * heightMultiplier, y - halfHeight);
                meshData.uvs[vertexIndex] = new Vector2(1f - (float)x/width, 1f - (float)y/height);

                if (x < width - 1 && y < height - 1) {
                    //meshData.AddTriangle(vertexIndex, vertexIndex + width + 1, vertexIndex + width);
                    //meshData.AddTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + 1);
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine, vertexIndex + verticesPerLine + 1);
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + 1);
                }

                vertexIndex += 1;  
            }
        }

        return meshData;
    }
}

public class MeshData {
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    int trianglesIndex;

    public MeshData(int meshWidth, int meshHeight) {
        vertices = new Vector3[meshHeight * meshWidth];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c) {
        triangles[trianglesIndex +0] = a;
        triangles[trianglesIndex +1] = b;
        triangles[trianglesIndex +2] = c;
        trianglesIndex += 3;
    }

    public Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}