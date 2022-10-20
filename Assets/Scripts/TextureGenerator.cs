using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class TextureGenerator
{
    public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height) {
        Array.Reverse(colourMap);

        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap) {
        int width = heightMap.GetUpperBound(0) + 1;
        int height = heightMap.GetUpperBound(1) + 1;

        Color[] colourMap = new Color[width * height];

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x,y]);    
            }
        }

        return TextureFromColourMap(colourMap, width, height);
    }
}
