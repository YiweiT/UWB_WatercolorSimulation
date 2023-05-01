using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KM_Model : MonoBehaviour
{
    public enum PenColor
    {
        QuinacridoneRose = 0,
        IndianRed = 1,
        CadmiumYellow = 2,
        HookersGreen = 3,
        CeruleanBlue = 4,
        BurntUmber = 5,
        CadmiumRed = 6,
        BrilliantOrange = 7,
        HansaYellow = 8,
        PhthaloGreen = 9,
        FrenchUltramarine = 10,
        InterferenceLilac = 11
    }

    const int krt = 12;
    public PenColor chosenColor;
    [Range(0.001f, 0.1f)]
    public float brushSize;
    [Range(0.001f, 0.1f)]
    public float pigmentPerStroke;
    public int canvasSize = 256;
    public Shader kmShader, compShader;
    Material kmMat, compMat;

    Vector4[] K = new Vector4[krt];
    Vector4[] S = new Vector4[krt];
    RenderTexture r0, t0, r0c, t0c, mainTex, mainTexc;
    // Start is called before the first frame update
    void Start()
    {
        InitKS();
        RTSetUp();
        MatSetUp();
    }
   void InitKS()
    {
        K[0] = new Vector4(0.22f, 1.47f, 0.57f, 0f);
        S[0] = new Vector4( 0.05f, 0.003f, 0.03f, 0f);

        K[1]= new Vector4(0.46f, 1.07f, 1.50f, 0f);
        S[1]= new Vector4(1.28f, 0.38f, 0.21f, 0f); 

        K[2] = new Vector4(0.10f, 0.36f, 3.45f, 0f); 
        S[2] = new Vector4(0.97f, 0.65f, 0.007f, 0f); 

        K[3] = new Vector4(1.62f, 0.61f, 1.64f, 0f); 
        S[3] = new Vector4(0.01f, 0.012f, 0.003f, 0f); 

        K[4] = new Vector4(1.52f, 0.32f, 0.25f, 0f); 
        S[4] = new Vector4(0.06f, 0.26f, 0.40f, 0f); 

        K[5] = new Vector4(0.74f, 1.54f, 2.10f, 0f); 
        S[5] = new Vector4(0.09f, 0.09f, 0.004f, 0f); 

        K[6] = new Vector4(0.14f, 1.08f, 1.68f, 0f); 
        S[6] = new Vector4(0.77f, 0.015f, 0.018f, 0f); 

        K[7] = new Vector4(0.13f, 0.81f, 3.45f, 0f); 
        S[7] = new Vector4(0.005f, 0.009f, 0.007f, 0f); 

        K[8] = new Vector4(0.06f, 0.21f, 1.78f, 0f); 
        S[8] = new Vector4(0.50f, 0.88f, 0.009f, 0f); 

        K[9] = new Vector4(1.55f, 0.47f, 0.63f, 0f); 
        S[9] = new Vector4(0.01f, 0.05f, 0.035f, 0f); 

        K[10] = new Vector4(0.86f, 0.86f, 0.06f, 0f); 
        S[10] = new Vector4(0.005f, 0.005f, 0.09f, 0f); 

        K[11] = new Vector4(0.08f, 0.11f, 0.07f, 0f); 
        S[11] = new Vector4(1.25f, 0.42f, 1.43f, 0f);
    }

    private void RTSetUp()
    {
        r0 = CreateRenderTexture(canvasSize, canvasSize); 
        t0 = CreateRenderTexture(canvasSize, canvasSize); 
        r0c = CreateRenderTexture(canvasSize, canvasSize); 
        t0c = CreateRenderTexture(canvasSize, canvasSize); 
        mainTex = CreateRenderTexture(canvasSize, canvasSize); 
        mainTexc = CreateRenderTexture(canvasSize, canvasSize);
    }

    private void MatSetUp()
    {
        kmMat = new Material(kmShader);
        compMat = new Material(compShader);

    }

    void Background()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    Vector4 Divide2Vector4_ComponentWise(Vector4 a, Vector4 b)
    {
        return new Vector4(a.x / b.x, a.y / b.y, a.z / b.z , a.w / b.w);
    }

    Vector4 Sqrt2Vector4_ComponentWise(Vector4 a)
    {
        return new Vector4(Mathf.Sqrt(a.x), Mathf.Sqrt(a.y), Mathf.Sqrt(a.z), Mathf.Sqrt(a.w));
    }
    RenderTexture CreateRenderTexture (int width, int height) {
		RenderTexture rt = new RenderTexture(width, height, 0);
		rt.format = RenderTextureFormat.ARGBFloat;
		rt.wrapMode = TextureWrapMode.Clamp;
		rt.filterMode = FilterMode.Point;
		rt.Create();
        // Graphics.Blit(null, _rt[i], _fillMat);
		return rt;
	}
}
