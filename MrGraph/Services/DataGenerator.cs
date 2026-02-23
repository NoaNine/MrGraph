using MrGraph.Services.Interface;
using System;

namespace MrGraph.Services;

public class DataGenerator : IDataGenerator
{
    private readonly Random _random = new();

    public void GenerateData(float[] data)
    {
        const float noiseFloor = -90f;

        for (int i = 0; i < data.Length; i++)
        {
            float noise = (float)(_random.NextDouble() * 10 - 5);
            data[i] = noiseFloor + noise;
        }

        int center = 400;
        int width = 20;

        for (int i = -width; i <= width; i++)
        {
            int index = center + i;
            if (index >= 0 && index < data.Length)
            {
                float shape = 1f - Math.Abs(i) / (float)width;
                data[index] = -40f + shape * 10f;
            }
        }
    }
}
