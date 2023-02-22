using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTextureGenerator
{
    // int _opt;
    int _width, _height;
    float _upperBound, _lowerBound; // inclusive

    public RandomTextureGenerator(int width, int height)
    {
        _width = width;
        _height = height;
        // _opt = 0; //  default value;
        _upperBound = 1f;
        _lowerBound = 0f;
    }

    public void SetBounds(float lower, float upper)
    {
        _upperBound = upper;
        _lowerBound = lower;
    }



    public Texture2D GenerateRandomTexture(int opt)
    {
        Texture2D tex = new Texture2D(_width, _height);
        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                float rand = Random.Range(_lowerBound, _upperBound);
                Color c;
                switch (opt)
                {
                    case 0:
                        c = new Color(rand, 0f, 0f, 0f);
                        break;
                    case 1:
                        c = new Color(0f, rand, 0f, 0f);
                        break; 
                    case 2:
                        c = new Color(0f, 0f, rand, 0f);
                        break; 
                    case 3:
                        c = new Color(0f, 0f, 0f, rand);
                        break;                  
                    default:
                        c = new Color(rand, rand, rand, rand);
                        break;
                }
                tex.SetPixel(i, j, c);
            }
        }
        tex.Apply();
        return tex;
    }

    // public Texture2D GenerateDefinedRho()

    public Texture2D GeneratePerlinNoiseTexture(float scale)
    {
        Texture2D _tex = new Texture2D(_width, _height);
       
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Color color = CalculateColor(x, y, scale);
                _tex.SetPixel(x, y, color);
            }
        }

        _tex.Apply();
        return _tex;
    }

    Color CalculateColor(int x, int y, float scale)
    {
        float xCoord = (float) x / _width * scale;
        float yCoord = (float) y / _height * scale;
        float sample = Mathf.PerlinNoise(xCoord, yCoord);
        sample = Mathf.Lerp(_lowerBound, _upperBound, sample);
        return new Color(sample, 0f, 0f, 0f);
    }
}
