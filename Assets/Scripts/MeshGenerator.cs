using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator {
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _meshHeightCurve, int levelOfDetail) {
        AnimationCurve meshHeightCurve = new AnimationCurve(_meshHeightCurve.keys);

        int width = heightMap.GetUpperBound(0) + 1;
        int height = heightMap.GetUpperBound(1) + 1;

        int meshSimplificationIncrement = levelOfDetail == 0 ? 1 : levelOfDetail * 2;
        int verticesPerLine = ((width - 1) / meshSimplificationIncrement) + 1;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
        int vertexIndex = 0;

        float halfWidth = (width - 1) / 2f;
        float halfHeight = (height - 1) / 2f;

        for (int y = 0; y < height; y += meshSimplificationIncrement) {
            for (int x = 0; x < width; x += meshSimplificationIncrement) {
                meshData.vertices[vertexIndex] = new Vector3(
                    x - halfWidth,
                    meshHeightCurve.Evaluate(heightMap[x, y]) * heightMultiplier,
                    y - halfHeight
                );
                meshData.uvs[vertexIndex] = new Vector2(1f - (float)x / width, 1f - (float)y / height);

                if (x < width - 1 && y < height - 1) {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine, vertexIndex + verticesPerLine + 1);
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + 1);
                }

                vertexIndex++;
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
        triangles[trianglesIndex + 0] = a;
        triangles[trianglesIndex + 1] = b;
        triangles[trianglesIndex + 2] = c;
        trianglesIndex += 3;
    }

    public bool HasValidCollisionGeometry() {
        if (vertices == null || triangles == null || triangles.Length < 3) {
            return false;
        }

        for (int i = 0; i < triangles.Length; i += 3) {
            int a = triangles[i];
            int b = triangles[i + 1];
            int c = triangles[i + 2];

            if (a == b || b == c || a == c) {
                continue;
            }

            Vector3 cross = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]);
            if (cross.sqrMagnitude > 1e-8f) {
                return true;
            }
        }

        return false;
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
