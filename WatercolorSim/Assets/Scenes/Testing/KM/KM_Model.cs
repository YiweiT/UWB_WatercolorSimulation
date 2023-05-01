using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KM_Model : MonoBehaviour
{
    public enum Pass{Canvas=0, R0=1, T0=2, Layer=3, R1=4, T1=5}
    public enum Draw{Line=0, Circle=1}
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
    public Draw drawOpt;
    const int krt = 12;
    public GameObject display, maskQuad;
    public PenColor chosenColor;
    public Pass displayPass;
    // [Range(0.001f, 0.1f)]
    // public float brushSize;
    // [Range(0.001f, 0.1f)]
    // public float pigmentPerStroke;
    [Range(0.001f, 1f)]
    public float offset=0.1f;
    [Range(0.001f, 1f)]
    public float mul=0.2f;
    public int canvasSize = 256;
    public Shader km_bgShader, paintShader, compShader;
    Material km_bgMat, paintMat, compMat;
    Vector4[] K = new Vector4[krt];
    Vector4[] S = new Vector4[krt];
    RenderTexture R0, T0, R0c, T0c,R1, T1, R1c, T1c, mainTex, mainTexc, mask, maskc, layerRT;
    public float pigPerStroke = 0.5f;
    [Range(0.001f, 0.05f)]
    public float pigDropRate = 0.01f;
    public float brushSize = 0.01f;
    float _currPigment;
    bool isDragging;
    RaycastHit hitInfo = new RaycastHit();
    Vector2 prevMousePos= -Vector2.one;
    // Start is called before the first frame update
    void Start()
    {
        InitKS();
        RTSetUp();
        MatSetUp();

        Background();
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
        R0 = CreateRenderTexture(canvasSize, canvasSize); 
        T0 = CreateRenderTexture(canvasSize, canvasSize); 
        R0c = CreateRenderTexture(canvasSize, canvasSize); 
        T0c = CreateRenderTexture(canvasSize, canvasSize); 
        R1 = CreateRenderTexture(canvasSize, canvasSize); 
        T1 = CreateRenderTexture(canvasSize, canvasSize); 
        R1c = CreateRenderTexture(canvasSize, canvasSize); 
        T1c = CreateRenderTexture(canvasSize, canvasSize); 
        mainTex = CreateRenderTexture(canvasSize, canvasSize); 
        mainTexc = CreateRenderTexture(canvasSize, canvasSize);
        // noiseRT = CreateRenderTexture(canvasSize, canvasSize);
        layerRT = CreateRenderTexture(canvasSize, canvasSize);
        mask = CreateRenderTexture(canvasSize, canvasSize);
        maskc = CreateRenderTexture(canvasSize, canvasSize);

        display.GetComponent<Renderer>().material.SetTexture("_MainTex", mainTex);
        maskQuad.GetComponent<Renderer>().material.SetTexture("_MainTex", mask);
    }

    private void MatSetUp()
    {
        km_bgMat = new Material(km_bgShader);
        compMat = new Material(compShader);
        paintMat = new Material(paintShader);

        RandomTextureGenerator g = new RandomTextureGenerator(canvasSize, canvasSize);
        g.SetBounds(0.01f, 0.85f);
        Texture2D perlinNoise = g.GeneratePerlinNoiseTexture(20f, 0);
        km_bgMat.SetTexture("_noise", perlinNoise);

        compMat.SetTexture("_R0", R0);
        compMat.SetTexture("_T0", T0);

        paintMat.SetTexture("_mask", mask);
    }

    void Background()
    {
        int c = chosenColor.GetHashCode();
        // Debug.Log("chosen color: " + chosenColor.ToString() + " - K=" + K[c] + ", S = " + S[c]);
        Vector4 a = Divide2Vector4_ComponentWise(K[c] + S[c], S[c]);
        Vector4 b = Sqrt2Vector4_ComponentWise(Vector4.Scale(a, a) - Vector4.one);
        // Debug.Log("a = " + a.ToString() + ", b = " + b.ToString());

        km_bgMat.SetVector("_S", S[c]);
        km_bgMat.SetVector("_a", a);
        km_bgMat.SetVector("_b", b);
        km_bgMat.SetFloat("_offset", offset);
        km_bgMat.SetFloat("_mul", mul);

        Graphics.Blit(null, mainTex, km_bgMat, 0);
        Graphics.Blit(null, R0, km_bgMat, 1);
        Graphics.Blit(null, T0, km_bgMat, 2);

        // Graphics.Blit(mainTexc, mainTex);
        // Graphics.Blit(R0c, R0);
        // Graphics.Blit(T0c, T0);

    }

    void BGUpdate()
    {
        int c = chosenColor.GetHashCode();
        // Debug.Log("chosen color: " + chosenColor.ToString() + " - K=" + K[c] + ", S = " + S[c]);
        Vector4 a = Divide2Vector4_ComponentWise(K[c] + S[c], S[c]);
        Vector4 b = Sqrt2Vector4_ComponentWise(Vector4.Scale(a, a) - Vector4.one);
        // Debug.Log("a = " + a.ToString() + ", b = " + b.ToString());

        km_bgMat.SetVector("_S", S[c]);
        km_bgMat.SetVector("_a", a);
        km_bgMat.SetVector("_b", b);
        km_bgMat.SetFloat("_offset", 0);
        km_bgMat.SetFloat("_mul", 1);
        km_bgMat.SetTexture("_noise", mask);

        Graphics.Blit(null, layerRT, km_bgMat, 0);
        Graphics.Blit(null, R1, km_bgMat, 1);
        Graphics.Blit(null, T1, km_bgMat, 2);

        compMat.SetTexture("_R1", R1);
        compMat.SetTexture("_T1", T1);

        Graphics.Blit(null, mainTexc, compMat, 0);
        Graphics.Blit(null, R0c, compMat, 1);
        Graphics.Blit(null, T0c, compMat, 2);

        Graphics.Blit(mainTexc, mainTex);
        Graphics.Blit(R0c, R0);
        Graphics.Blit(T0c, T0);        
    }

    void MouseDragging() 
    {
        if(Input.GetMouseButtonDown(0)) {
			isDragging = true;
            _currPigment = pigPerStroke;
		} else if(Input.GetMouseButtonUp(0)) {
			isDragging = false;
            prevMousePos = -Vector2.one;

		}

        // when mouse clicks on the drawing paper, new ink adding to the canvas
        if (isDragging) 
        {
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo)) {
                return;  // did not intersect
            }

            Vector3 mouseInWorld = hitInfo.point;

            // convert mouse position to UV space
            Vector3 toCenter = mouseInWorld - display.transform.localPosition;
            float mx = (toCenter.x + (display.transform.localScale.x*0.5f)) / display.transform.localScale.x;  // assuming square
            float my = (toCenter.y + (display.transform.localScale.y*0.5f)) / display.transform.localScale.y;
            Debug.Log("(" + mx + ", " + my + ")");

            if (prevMousePos.x < 0f)
            {
                prevMousePos = new Vector2(mx, my);
            }
            
            paintMat.SetFloat("_px", prevMousePos.x);
            paintMat.SetFloat("_py", prevMousePos.y);
            paintMat.SetFloat("_x", mx);
            paintMat.SetFloat("_y", my);
            paintMat.SetFloat("_size", brushSize);
            paintMat.SetFloat("_val", _currPigment);

            // paintMat.SetTexture("_tex01", rt[0]);
            Graphics.Blit(null, maskc, paintMat, 0);
            Graphics.Blit(maskc, mask);
            _currPigment -= _currPigment * pigDropRate;
            prevMousePos = new Vector2(mx, my);
        }        
    }

    void debug()
    {
        // display
        switch (displayPass.GetHashCode())
        {
            
            default:
                display.GetComponent<Renderer>().material.SetTexture("_MainTex", mainTex);
                break;
            case 1:
                display.GetComponent<Renderer>().material.SetTexture("_MainTex", R0);
                break;
            case 2:
                display.GetComponent<Renderer>().material.SetTexture("_MainTex", T0);
                break;
            case 3:
                display.GetComponent<Renderer>().material.SetTexture("_MainTex", layerRT);
                break;
            case 4:
                display.GetComponent<Renderer>().material.SetTexture("_MainTex", R1);
                break;
            case 5:
                display.GetComponent<Renderer>().material.SetTexture("_MainTex", T1);
                break;
        }
    }
    // Update is called once per frame
    void Update()
    {
        // if(Input.GetMouseButtonDown(0))
        // {
        //     BGUpdate();
        // }
        MouseDragging();
        
        BGUpdate();

        
        debug();
        // Background();
        
    }
    Vector4 Divide2Vector4_ComponentWise(Vector4 a, Vector4 b)
    {
        return new Vector4(a.x / b.x, a.y / b.y, a.z / b.z , 1);
    }

    Vector4 Sqrt2Vector4_ComponentWise(Vector4 a)
    {
        return new Vector4(Mathf.Sqrt(a.x), Mathf.Sqrt(a.y), Mathf.Sqrt(a.z), 1);
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
