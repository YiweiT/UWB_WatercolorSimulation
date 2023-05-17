using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using UnityEngine;

public class FlowSimController : MonoBehaviour
{
    public enum DisplayOpt
    {
        R = 0, G = 1, B = 2, A = 3, ALL = 4, Gray = 5
    };
    public enum Pass{ Canvas=0, R0=1, T0=2, Layer=3, R1=4, T1=5}
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
    const int krt = 5;
    const int colorK = 12;
    // Save the display texture to png file
    public Button saveImgBtn;
    public InputField imgNameInput;
    public Button saveHBtn;
    public InputField heightInput;
    // display quads
    public GameObject quad, colQuad, displayQ;
    public GameObject[] objs = new GameObject[krt];
    public DisplayOpt[] displayOpts = new DisplayOpt[krt];
    public DisplayOpt colTexOpt;
    public float colTexMultiplier;
    public Shader fillShader, debugShader, paintShader, boundaryShader, streamShader, rho_vUpdateShader, collideShader, pigSupplyShader, pigAdvectShader, pigFixtureShader, compShader, kmShader;
    Material fillMat, debugMat, paintMat, boundaryMat, streamMat, rho_vUpdateMat, collideMat, pigSupplyMat, pigAdvectMat, pigFixtureMat, compMat, kmMat;
    public int canvasSize = 256;
    public PenColor _bgCol;
    [Range(0.01f, 1f)]
    public float bgOffset=0.05f;
    [Range(0.01f, 1f)]
    public float bgMul=0.05f;
    public Pass displayPass;
    public float[] _multiplier = new float[krt];
    // Paint Parameters
    public bool waterBrush = false;
    public PenColor brushColor;
    public float brushSize = 0.01f;
    [Range(0.05f, 1f)]
    public float pigmentAmount = 0.5f;
    [Range(0.001f, 0.05f)]
    public float pigDropRate = 0.01f;
    [Range(0.001f, 5f)]
    public float waterAmount = 0.1f;
    // Paper Properties
    [Range(0f, 0.999f)]
    public float heightUpperBound, heightLowerBound;
    public float heightScale;
    // Simulation Parameters
    [Range(0.0000001f, 0.5f)]
    public float evaporationRate = 0.01f;
    [Range(0.9f, 1.5f)]
    public float waterRelaxation = 1f;    
    [Range(0.01f, 0.5f)]
    public float sigma = 0.01f;
    // Pigment movement Parameters
    [Range(0.0001f, 1f)]
    public float drynessPara = 0.1f;
    [Range(0.0001f, 1f)]
    public float deposite_base = 0.1f;
    [Range(0f, 1f)]
    public float granulThres = 0.1f;
    [Range(0.001f, 10f)]
    public float granularity = 0.1f;
    [Range(0.001f, 0.999f)]
    public float re_absorb = 0.1f;

    RenderTexture mask, maskc, currColTex, currColTexc, _temp;
    RenderTexture _driedLayer; // a layer of dried pigments, stored the pigment concentration, reflectance and transmittance
    Dictionary<int, RenderTexture> _colorLayer = new Dictionary<int, RenderTexture>(); // PS, PF, PX
    /// <summary>
    /// rt: r   g   b   a
    /// 0:  f1  f2  f3  f4
    /// 1:  f5  f6  f7  f8
    /// 2:  vx  vy  r   wf
    /// 3:  f0  k   r'  ws 
    /// 4:  h
    /// </summary>
    RenderTexture[] rt = new RenderTexture[krt];
    RenderTexture[] rtc = new RenderTexture[krt];
    RenderTexture[] debugRT = new RenderTexture[krt];
    float _currPigment;
    public bool isDragging;
    RaycastHit hitInfo = new RaycastHit();
    Vector2 prevMousePos = -Vector2.one;
    // Vector2 prevMousePos = Vector2.zero;
    // pigment composition
    Vector4[] K = new Vector4[colorK];
    Vector4[] S = new Vector4[colorK];
    RenderTexture R0, T0, R0c, T0c, R1, T1, displayTex, layerRT;


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

    void RTSetUp()
    {
        _temp = RenderTexture.GetTemporary(canvasSize, canvasSize);
        mask = CreateRenderTexture(canvasSize, canvasSize);
        maskc = CreateRenderTexture(canvasSize, canvasSize);

        currColTexc = CreateRenderTexture(canvasSize, canvasSize);
        colQuad.GetComponent<Renderer>().material.SetTexture("_MainTex", currColTexc);

        for (int i = 0; i < krt; i++)
        {
            rt[i] = CreateRenderTexture(canvasSize, canvasSize);
            rtc[i] = CreateRenderTexture(canvasSize, canvasSize);
            debugRT[i] = CreateRenderTexture(canvasSize, canvasSize);

            objs[i].GetComponent<Renderer>().material.SetTexture("_MainTex", debugRT[i]);
        }

        _driedLayer = CreateRenderTexture(canvasSize, canvasSize);

        // Generate height field
        RandomTextureGenerator g = new RandomTextureGenerator(canvasSize, canvasSize);
        g.SetBounds(heightLowerBound, heightUpperBound);
        Texture2D perlinNoise = g.GeneratePerlinNoiseTexture(heightScale, 0);
        Graphics.Blit(perlinNoise, rt[4]);
        Graphics.Blit(perlinNoise, rtc[4]);

        // Composition
        R0 = CreateRenderTexture(canvasSize, canvasSize);
        T0 = CreateRenderTexture(canvasSize, canvasSize);
        R0c = CreateRenderTexture(canvasSize, canvasSize);
        T0c = CreateRenderTexture(canvasSize, canvasSize);
        R1 = CreateRenderTexture(canvasSize, canvasSize);
        T1 = CreateRenderTexture(canvasSize, canvasSize);


        displayTex = CreateRenderTexture(canvasSize, canvasSize);
        // displayTexc = CreateRenderTexture(canvasSize, canvasSize);
        layerRT = CreateRenderTexture(canvasSize, canvasSize);
        // layerRTc = CreateRenderTexture(canvasSize, canvasSize);
        displayQ.GetComponent<Renderer>().material.SetTexture("_MainTex", displayTex);
        quad.GetComponent<Renderer>().material.SetTexture("_MainTex", displayTex);

    }

    void MatInit()
    {
        fillMat = new Material(fillShader);
        // Fluid Simulation
        paintMat = new Material(paintShader);
        boundaryMat = new Material(boundaryShader);
        streamMat = new Material(streamShader);
        rho_vUpdateMat = new Material(rho_vUpdateShader);
        collideMat = new Material(collideShader);

        // Pigment Movements
        pigSupplyMat = new Material(pigSupplyShader);
        pigAdvectMat = new Material(pigAdvectShader);
        pigFixtureMat = new Material(pigFixtureShader);
        
        // Composition
        kmMat = new Material(kmShader);
        compMat = new Material(compShader);
        

        // debugging
        debugMat = new Material(debugShader);

    }

    void MatSetUp()
    {
        paintMat.SetTexture("_mask", mask);
        paintMat.SetTexture("_RefTex2", rt[2]); // r
        paintMat.SetTexture("_RefTex3", rt[3]); // ws

        boundaryMat.SetTexture("_tex2", rt[2]); // r, 
        boundaryMat.SetTexture("_tex3", rt[3]); // k, r', ws
        boundaryMat.SetTexture("_tex4", rt[4]); // h

        streamMat.SetTexture("_tex0", rt[0]); // f1 - f4
        streamMat.SetTexture("_tex1", rt[1]); // f5 - f6
        streamMat.SetTexture("_tex3", rt[3]); // k

        rho_vUpdateMat.SetTexture("_tex0", rt[0]); // f1 - f4
        rho_vUpdateMat.SetTexture("_tex1", rt[1]); // f5 - f6
        rho_vUpdateMat.SetTexture("_tex2", rt[2]); // r, 
        rho_vUpdateMat.SetTexture("_tex3", rt[3]); // ws --> wf, r

        collideMat.SetTexture("_RefTex0", rt[0]); // f1 - f4
        collideMat.SetTexture("_RefTex1", rt[1]); // f5 - f6
        collideMat.SetTexture("_RefTex2", rt[2]); // v, r
        collideMat.SetTexture("_RefTex3", rt[3]); // f0

        // Pigment Movements
        pigSupplyMat.SetTexture("_tex2", rt[2]);

        pigAdvectMat.SetTexture("_tex0", rt[0]); // f1 - f4
        pigAdvectMat.SetTexture("_tex1", rt[1]); // f5 - f6
        pigAdvectMat.SetTexture("_tex2", rt[2]); // v, r
        pigAdvectMat.SetTexture("_tex3", rt[3]); // k, r'

        pigFixtureMat.SetTexture("_tex0", rt[0]); // f1 - f4
        pigFixtureMat.SetTexture("_tex1", rt[1]); // f5 - f6
        pigFixtureMat.SetTexture("_tex2", rt[2]); // v, r
        pigFixtureMat.SetTexture("_tex3", rt[3]); // k, r'
        pigFixtureMat.SetTexture("_tex4", rt[4]); // h

        // Composition
        compMat.SetTexture("_R0", R0);
        compMat.SetTexture("_T0", T0);
        compMat.SetTexture("_R1", R1);
        compMat.SetTexture("_T1", T1);

    }

    void Background(int c, RenderTexture noise, float offset, float mul)
    {
        // int c = _bgCol.GetHashCode();
        Vector4 a = Divide2Vector4_ComponentWise(K[c] + S[c], S[c]);
        Vector4 b = Sqrt2Vector4_ComponentWise(Vector4.Scale(a, a) - Vector4.one);
        // Debug.Log("chosen bg color: " + _bgCol.ToString() );


        kmMat.SetVector("_S", S[c]);
        kmMat.SetVector("_a", a);
        kmMat.SetVector("_b", b);
        kmMat.SetTexture("_noise", noise);
        kmMat.SetFloat("_offset", offset);
        kmMat.SetFloat("_mul", mul);

        Graphics.Blit(null, displayTex, kmMat, 0);
        Graphics.Blit(null, R0c, kmMat, 1);
        Graphics.Blit(null, T0c, kmMat, 2);
        // Graphics.Blit(displayTexc, displayTex);
        Graphics.Blit(R0c, R0);
        Graphics.Blit(T0c, T0);
    }

    void Start()
    {
        if (heightLowerBound > heightUpperBound)
        {
            float temph = heightLowerBound;
            heightLowerBound = heightUpperBound;
            heightLowerBound = temph;
        }
        InitKS();
        RTSetUp();        
        MatInit();
        MatSetUp();

        Background(_bgCol.GetHashCode(), rt[4], bgOffset, bgMul);
        SetQuadToPixelPerfect(quad, displayTex);

        // save image
        saveImgBtn.onClick.AddListener(SaveImg);

        // save height
        saveHBtn.onClick.AddListener(SaveHeight);
    }

    void SetQuadToPixelPerfect(GameObject q, RenderTexture rt)
    {
        float scale = (Screen.height / 2.0f) / Camera.main.orthographicSize;
        Vector3 newTransform = new Vector3(rt.width / scale, rt.height / scale, q.transform.localScale.z);
        q.transform.localScale = newTransform;
        
    }

    // Update is called once per frame
    void Update()
    {
        MouseDragging();
        BoundaryUpdate();
        Colliding();
        Streaming();

        PigmentMovement();
        if(_colorLayer.Count > 0)
        {
            Composition();
        }

        // debugging
        Debugging();
    }



    void MouseDragging() 
    {
        if(Input.GetMouseButtonDown(0)) {
			isDragging = true;
            _currPigment = pigmentAmount;
		} else if(Input.GetMouseButtonUp(0)) {
			isDragging = false;
            prevMousePos = -Vector2.one;
            // prevMousePos = Vector2.zero;

		}

        // when mouse clicks on the drawing paper, new ink adding to the canvas
        if (isDragging) 
        {
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo)) {
                return;  // did not intersect
            }

            Vector3 mouseInWorld = hitInfo.point;

            // convert mouse position to UV space
            Vector3 toCenter = mouseInWorld - quad.transform.localPosition;
            float mx = (toCenter.x + (quad.transform.localScale.x*0.5f)) / quad.transform.localScale.x;  // assuming square
            float my = (toCenter.y + (quad.transform.localScale.y*0.5f)) / quad.transform.localScale.y;
            // Debug.Log("(" + mx + ", " + my + ")");

            if (prevMousePos.x < 0f)
            {
                prevMousePos = new Vector2(mx, my);
            }


            paintMat.SetFloat("_px", prevMousePos.x);
            paintMat.SetFloat("_py", prevMousePos.y);
            paintMat.SetFloat("_x", mx);
            paintMat.SetFloat("_y", my);
            paintMat.SetFloat("_size", brushSize);
            paintMat.SetFloat("_pigAmt", _currPigment);
            paintMat.SetFloat("_waterAmt", waterAmount);

            // apply water on the paper
            Graphics.Blit(null, rtc[3], paintMat, 1);
            Graphics.Blit(rtc[3], rt[3]);

            if (!waterBrush)
            {
                // check whether the new ink color is still wet
                // if not in the _colorLayer, create a new color-rt item and store in _colorLayer
                RenderTexture colTex = ContainsColor(brushColor.GetHashCode());
                paintMat.SetTexture("_ColTex", colTex);

                // // paintMat.SetTexture("_tex01", rt[0]);
                // Graphics.Blit(null, maskc, paintMat, 0);
                // Graphics.Blit(maskc, mask);

                // apply pigment on the color texture
            
                paintMat.SetFloat("_val", _currPigment);
                Graphics.Blit(null, _temp, paintMat, 2);
                Graphics.Blit(_temp, colTex);
                RenderTexture.ReleaseTemporary(_temp);

                currColTex = colTex;
                _currPigment -= _currPigment * pigDropRate;
            }
            prevMousePos = new Vector2(mx, my);

        }        
    }

    void BoundaryUpdate()
    {
        Graphics.Blit(null, rtc[3], boundaryMat);
        Graphics.Blit(rtc[3], rt[3]);
    }
    void Streaming()
    {
        Graphics.Blit(null, rtc[0], streamMat, 0);
        Graphics.Blit(null, rtc[1], streamMat, 1);

        Graphics.Blit(rtc[0], rt[0]);   
        Graphics.Blit(rtc[1], rt[1]); 

        rho_vUpdateMat.SetFloat("eps", evaporationRate);
        Graphics.Blit(null, rtc[2], rho_vUpdateMat);
        Graphics.Blit(rtc[2], rt[2]);
    }

    void Colliding()
    {
        collideMat.SetFloat("omega", waterRelaxation);

        Graphics.Blit(null, rtc[0], collideMat, 0);
        Graphics.Blit(null, rtc[1], collideMat, 1);       
        Graphics.Blit(null, rtc[3], collideMat, 2);

        Graphics.Blit(rtc[0], rt[0]);
        Graphics.Blit(rtc[1], rt[1]);
        Graphics.Blit(rtc[3], rt[3]);
    }

    void PigmentMovement()
    {
        foreach (RenderTexture colTex in _colorLayer.Values)
        {
            PigSupply(colTex, _temp);
            PigAdvection(colTex, _temp);
            PigFixture(colTex, _temp);
        }
    }

    void PigSupply(RenderTexture colTex, RenderTexture temp)
    {
        pigSupplyMat.SetTexture("_ColTex", colTex);
        Graphics.Blit(null, temp, pigSupplyMat);
        Graphics.Blit(temp, colTex);
        RenderTexture.ReleaseTemporary(temp);
    }

    void PigAdvection(RenderTexture colTex, RenderTexture temp)
    {
        pigAdvectMat.SetTexture("_ColTex", colTex);
        pigAdvectMat.SetFloat("sigma", sigma);

        Graphics.Blit(null, temp, pigAdvectMat);
        Graphics.Blit(temp, colTex);
        RenderTexture.ReleaseTemporary(temp);
    }

    void PigFixture(RenderTexture colTex, RenderTexture temp)
    {
        pigFixtureMat.SetTexture("_ColTex", colTex);
        pigFixtureMat.SetFloat("deposite_base", deposite_base);
        pigFixtureMat.SetFloat("drynessPara", drynessPara);
        pigFixtureMat.SetFloat("granularity", granularity);
        pigFixtureMat.SetFloat("granulThres", granulThres);
        pigFixtureMat.SetFloat("re_absorb", re_absorb);

        Graphics.Blit(null, temp, pigFixtureMat);
        Graphics.Blit(temp, colTex);
        RenderTexture.ReleaseTemporary(temp);
    }

    void Composition()
    {
        Background(_bgCol.GetHashCode(), rt[4], bgOffset, bgMul);
        foreach (KeyValuePair<int, RenderTexture> item in _colorLayer)
        {
            CompLayers(item.Key, item.Value, 0, 1);
        }
    }
    void CompLayers(int c, RenderTexture noise, float offset, float mul)
    {
        Vector4 a = Divide2Vector4_ComponentWise(K[c] + S[c], S[c]);
        Vector4 b = Sqrt2Vector4_ComponentWise(Vector4.Scale(a, a) - Vector4.one);

        // Calculate Layer R1, T1
        kmMat.SetVector("_S", S[c]);
        kmMat.SetVector("_a", a);
        kmMat.SetVector("_b", b);
        kmMat.SetFloat("_offset", offset);
        kmMat.SetFloat("_mul", mul);
        kmMat.SetTexture("_noise", noise);
        Graphics.Blit(null, layerRT, kmMat, 0);
        Graphics.Blit(null, R1, kmMat, 1);
        Graphics.Blit(null, T1, kmMat, 2);
        
        // Composite two layers
        Graphics.Blit(null, displayTex, compMat, 0);
        Graphics.Blit(null, R0c, compMat, 1);
        Graphics.Blit(null, T0c, compMat, 2);

        Graphics.Blit(R0c, R0);
        Graphics.Blit(T0c, T0);
    }
    void Debugging()
    {
        for (int i = 0; i < krt; i++)
        {
            debugMat.SetTexture("_MainTex", rt[i]);
            debugMat.SetFloat("_multiplier", _multiplier[i]);
            Graphics.Blit(null, debugRT[i], debugMat, displayOpts[i].GetHashCode());
        }
        // Debug.Log("Count of color layers: " + _colorLayer.Count);

        // color texture
        debugMat.SetTexture("_MainTex",currColTex);
        debugMat.SetFloat("_multiplier", colTexMultiplier);
        
        Graphics.Blit(null, currColTexc, debugMat, colTexOpt.GetHashCode());

        // display
        switch (displayPass.GetHashCode())
        {
            
            default:
                displayQ.GetComponent<Renderer>().material.SetTexture("_MainTex", displayTex);
                break;
            case 1:
                displayQ.GetComponent<Renderer>().material.SetTexture("_MainTex", R0);
                break;
            case 2:
                displayQ.GetComponent<Renderer>().material.SetTexture("_MainTex", T0);
                break;
            case 3:
                displayQ.GetComponent<Renderer>().material.SetTexture("_MainTex", layerRT);
                break;
            case 4:
                displayQ.GetComponent<Renderer>().material.SetTexture("_MainTex", R1);
                break;
            case 5:
                displayQ.GetComponent<Renderer>().material.SetTexture("_MainTex", T1);
                break;
        }
        
    }

    void OnDestroy()
    {
        for (int i = 0; i < krt; i++)
        {
            rt[i].Release();
            rtc[i].Release();
            debugRT[i].Release();
        }

        foreach (RenderTexture tex in _colorLayer.Values)
        {
            tex.Release();
        }
        _driedLayer.Release();
        mask.Release();
        maskc.Release();

        R0.Release();
        T0.Release();
        R0c.Release();
        T0c.Release();
        R1.Release();
        T1.Release();
    }


    // Check if the given color is in the _colorLayer key set. 
    // If exists, return the corresponding rt
    // else, store the given color in the _colorLayer with a newly created rt, and return this rt
    RenderTexture ContainsColor(int c) 
    {
        if (_colorLayer.ContainsKey(c))
        {
            return _colorLayer[c];
        } else {
            RenderTexture newColTex = CreateRenderTexture(canvasSize, canvasSize);
            _colorLayer.Add(c, newColTex);
            return newColTex;
        }
    }

    void SaveImg()
    {
        String dirPath = Application.dataPath + "/../SaveImages/";
        if(!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        var oldRt = RenderTexture.active;
        RenderTexture.active = displayTex;
        Texture2D tex =  new Texture2D(displayTex.width, displayTex.height, TextureFormat.RGBAFloat, false, true);
        tex.ReadPixels(new Rect(0, 0, displayTex.width, displayTex.height), 0, 0);
        tex.Apply();
        RenderTexture.active = oldRt;
        String timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        if(imgNameInput.text == null)
        {
            imgNameInput.text = "Img";
        }

        File.WriteAllBytes(dirPath + imgNameInput.text + "_" + timeStamp + ".png", tex.EncodeToPNG());

    }

    void SaveHeight()
    {
        String dirPath = Application.dataPath + "/../SaveImages/";
        if(!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        var oldRt = RenderTexture.active;
        RenderTexture.active =debugRT[4];
        Texture2D tex =  new Texture2D(debugRT[4].width,debugRT[4].height, TextureFormat.RGBAFloat, false, true);
        tex.ReadPixels(new Rect(0, 0,debugRT[4].width,debugRT[4].height), 0, 0);
        tex.Apply();
        RenderTexture.active = oldRt;
        // String timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        
        heightInput.text = "Height_" + heightLowerBound + "_" + heightUpperBound + "_" + heightScale;
        

        File.WriteAllBytes(dirPath + heightInput.text  + ".png", tex.EncodeToPNG());

    }

    Vector4 Divide2Vector4_ComponentWise(Vector4 a, Vector4 b)
    {
        return new Vector4(a.x / b.x, a.y / b.y, a.z / b.z , 1f);
    }

    Vector4 Sqrt2Vector4_ComponentWise(Vector4 a)
    {
        return new Vector4(Mathf.Sqrt(a.x), Mathf.Sqrt(a.y), Mathf.Sqrt(a.z), 1f);
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
