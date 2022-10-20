using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: No tutorial o Sebastian Lague pareceu ter tido dificuldades com o heightMap, em garantir que esteja entre 0 e 1, ou se isso é um problema
//E isso o fez criar a solução envolvendo o NormalizeMode aqui. Estudar o que acontece nessa parte e ver se é necessária alguma otimização

public static class Noise {
    public enum NormalizeMode {Local, Global, Test};

    public static float[,] GenerateNoiseMap(
        int mapWidth, 
        int mapHeight, 
        int seed,
        float scale, 
        int octaves, 
        float persistance, 
        float lacunarity,
        Vector2 offset,
        NormalizeMode normalizeMode,
        float globalNormalizeModeOffset,
        float testNormalizeModeIntensity,
        float testNormalizeModePeaks
    ) {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float amplitude = 1f;
        float frequency = 1f;
        
        float maxPossibleHeight = 0f;

        for (int i = 0; i < octaves; i++) {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        if (scale <= 0) {
            scale = 0.0001f;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0;  x < mapWidth; x++) {

                amplitude = 1f;
                frequency = 1f;
                float noiseHeight = 0f;

                for (int i = 0; i < octaves; i++) {
                    //halfWidth / halfHeight é usado para que o scale sempre vá em direção ao centro do mapa. Sem esse ajuste sempre irá para o canto superior direito. 
                    //E se quiser que o scale vá para algum outro ponto, user defined? Essas 2 variáveis teriam que ter um valor entre 0 e mapWidth / mapHeight;
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = (Mathf.PerlinNoise(sampleX, sampleY) * 2) - 1f;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxLocalNoiseHeight) {
                    maxLocalNoiseHeight = noiseHeight;
                }
                if (noiseHeight < minLocalNoiseHeight) {
                    minLocalNoiseHeight = noiseHeight;
                }

                noiseMap[x,y] = noiseHeight;

                if (normalizeMode == NormalizeMode.Global) {
                    float normalizedHeight = (noiseMap[x,y] + 1) / (2f * maxPossibleHeight / globalNormalizeModeOffset);
                    noiseMap[x,y] = Mathf.Clamp(normalizedHeight, 0f, int.MaxValue);
                }
            }
        }
        
        if (normalizeMode == NormalizeMode.Local) {
            for (int y = 0; y < mapHeight; y++) {
                for (int x = 0;  x < mapWidth; x++) {
                    noiseMap[x,y] = Mathf.InverseLerp (minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x,y]);
                }
            }
        }
        else if (normalizeMode == NormalizeMode.Test) {
            float intensity = testNormalizeModeIntensity;
            float peakness = testNormalizeModePeaks;
            for (int y = 0; y < mapHeight; y++) {
                for (int x = 0;  x < mapWidth; x++) {
                    float normalizedHeightMap = (1 + peakness) / (1 + Mathf.Exp((noiseMap[x,y] * -1) * intensity));
                    float flipThenAbsThenUnflip = 1 - Mathf.Abs(1 - normalizedHeightMap);

                    noiseMap[x,y] = flipThenAbsThenUnflip;
                }
            }
        }

        return noiseMap;
    }
}
