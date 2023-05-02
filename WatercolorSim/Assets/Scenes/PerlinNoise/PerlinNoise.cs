using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise : MonoBehaviour
{
    public enum TextureOpt
    {
        Shader = 0,
        CPU = 1
    };
    public TextureOpt opt;
    public enum Pass 
    {
        pass0 = 0,
        pass1 = 1
    };
    public Pass pass;
    public int width = 1080;
    public int height = 1080;
    // [Range(1f, 20f)]
    public float scale = 200;
    RenderTexture rt;
    public Shader shader;
    Material mat;
    Renderer renderer;
    Texture2D tex;
    RandomTextureGenerator generator;
    // Start is called before the first frame update
    void Start()
    {
        rt = new RenderTexture(width, height, 0);
        renderer = GetComponent<Renderer>();
        mat = new Material(shader);
        generator = new RandomTextureGenerator(width, height);
        
        // tex = generator.GenerateRandomTexture(2);
        
    }

    private void Update() {
        if (opt.GetHashCode() == 0) {
            Graphics.Blit(null, rt, mat, pass.GetHashCode());
            renderer.material.mainTexture = rt;
        } else {
            // renderer.material.mainTexture = GenerateTexture();
            tex = generator.GeneratePerlinNoiseTexture(scale, 0);

            renderer.material.mainTexture = tex;
        }
        
        
    }

    private Texture GenerateTexture1()
    {
        Texture2D tex = new Texture2D(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color color = CalculateColor(x, y);
                tex.SetPixel(x, y, color);
            }
        }

        tex.Apply();
        return tex;
    }

    private Color CalculateColor(int x, int y)
    {
        float xCoord = (float) x / width * scale;
        float yCoord = (float) y / height * scale;
        float sample = Mathf.PerlinNoise(xCoord, yCoord);
        return new Color(sample, sample, sample);
    }


}
