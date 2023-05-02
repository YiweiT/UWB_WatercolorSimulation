using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class Simulator : MonoBehaviour
{
    // constants
    const int krt = 5;
    public int canvasSize = 100;

    public enum DisplayOption
    {
        R = 0, G = 1, B = 2, A = 3
    };

    // public variables (showing in editor)
    public Shader _mouseClickShader, _fillShader, _boundaryShader, _streamShader, _rhoUpdateShader, _collideShader;
    public Color _penColor = Color.red;
    public float _brushSize = 0.2f;
    [Range(0f, 0.99f)]
    public float heightUpperBound, heightLowerBound;
    public float heightScale;


    // private variables (not showing in the editor)
    /*
     A set of rts, used to store variables in fluid simulation process. 
     Each rt stores 4 variables at R, G, B, A channels. 
     Each variable is a floating number, ranged from [0, 1].
       _rt[0]: f1, f2, f3, f4 (distribution functions on E, N, W, S)
       _rt[1]: f5, f6, f7, f8 (distribution functions on NE, NW, SW, SE)
       _rt[2]: vx, vy, rho, wf (velocity in x-dir, velocity in y-dir, amount of water in flow layer, and amount of water applied on the surface layer)
       _rt[3]: f0, k, rho', ws (stable dist. fun., blocking factor, rho in the last frame, amount of water transferred from surface to flow layer)
       _rt[4]: h
    */ 
    RenderTexture[] _rt; 
    RenderTexture[] _rtc; // copies of _rts, used as result rts 
    Material _mouseClickMat, _fillMat, _boundaryMat, _streamMat, _rhoUpdateMat, _collideMat;
    Dictionary<Color, RenderTexture> _colorLayer; // PS, PF, PX
    // List<Color> _colorLayers; // list of colors corresponding to the pigment layers
    // List<RenderTexture> _colorRT; // a list of rts representing pigment layers, each rt stores pigment concentration of surface layer, flow layer, and fixture layer
    RenderTexture _driedLayer; // a layer of dried pigments, stored the pigment concentration, reflectance and transmittance.
    bool _isDragging;
    RaycastHit hitInfo = new RaycastHit();
    RenderTexture _myRT;

    // debugging
    public GameObject[] debugs = new GameObject[krt+1];
    public DisplayOption[] displayOpts = new DisplayOption[krt];
    public Shader _debugShader;
    Material _debugMat;
    RenderTexture[] displayRTs = new RenderTexture[krt];



    // Start is called before the first frame update
    void Start()
    {
        if (heightLowerBound > heightUpperBound)
        {
            float temp = heightLowerBound;
            heightLowerBound = heightUpperBound;
            heightLowerBound = temp;
        }

        InitiateMats();
        SetUpRTs(canvasSize, canvasSize);
        SetUpMats();
        
        Graphics.Blit(null, _myRT, _fillMat);
        GetComponent<Renderer>().material.SetTexture("_MainTex", _myRT);

        // SetUp lists for color layers
        _colorLayer = new Dictionary<Color, RenderTexture>();

        // Debugging ...
        

    }

    // Update is called once per frame
    void Update()
    {
        MouseClick();
        BoundaryUpdate();
        Streaming();
        Colliding();

        // debugging ...
        Debugging();
    }
    
    void Debugging()
    {
        for (int i = 0; i < krt; i++)
        {
            Debug.Assert(displayRTs[i] != null);
            Debug.Assert(_debugMat != null);

            _debugMat.SetTexture("_MainTex", _rt[i]); 
            Graphics.Blit(null, displayRTs[i], _debugMat, displayOpts[i].GetHashCode());
        }
    }

    void MouseClick()
    {
        if(Input.GetMouseButtonDown(0)) {
			_isDragging = true;
		} else if(Input.GetMouseButtonUp(0)) {
			_isDragging = false;
		}

        // when mouse clicks on the drawing paper, new ink adding to the canvas
        if (_isDragging) 
        {
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo)) {
                return;  // did not intersect
            }

            Vector3 mouseInWorld = hitInfo.point;

            // convert mouse position to UV space
            Vector3 toCenter = mouseInWorld - transform.localPosition;
            float mx = (toCenter.x + (transform.localScale.x*0.5f)) / transform.localScale.x;  // assuming square
            float my = (toCenter.y + (transform.localScale.y*0.5f)) / transform.localScale.y;
            // Debug.Log("(" + mx + ", " + my + ")");

            // check whether the new ink color is still wet
            // if not in the _colorLayer, create a new color-rt item and store in _colorLayer
            RenderTexture tex2 = ContainsColor(_penColor);

            // pass tex2 to the mouseClick shader for pass 1
            _mouseClickMat.SetTexture("_ColTex", tex2);
            // Pass mx, my, and brushSize to the mouseClick shader
            _mouseClickMat.SetFloat("_x", mx);
            _mouseClickMat.SetFloat("_y", my);
            _mouseClickMat.SetFloat("_brushSize", _brushSize);
            _mouseClickMat.SetInt("_hasNewInk", 1);
            _mouseClickMat.SetColor("_penCol", _penColor);

            // pass 0: update ws, result to _rtc[2]
            Graphics.Blit(null, _rtc[3], _mouseClickMat, 0);
            // pass 1: update ps, result to temp rt
            RenderTexture temp = RenderTexture.GetTemporary(canvasSize, canvasSize);
            Graphics.Blit(null, temp, _mouseClickMat, 1);
            // pass 2: draw on the paper, result to temp2 rt
            RenderTexture temp1 = RenderTexture.GetTemporary(canvasSize, canvasSize);
            Graphics.Blit(null, temp1, _mouseClickMat, 2);
            // swap reference and result
            Graphics.Blit(_rtc[3], _rt[3]);
            Graphics.Blit(temp, tex2);
            Graphics.Blit(temp1, _myRT);
            // clear out the temp rt
            RenderTexture.ReleaseTemporary(temp);
            RenderTexture.ReleaseTemporary(temp1);
            // Reset mouseClick shader references
            _mouseClickMat.SetTexture("_ColTex", null);
            _mouseClickMat.SetInt("_hasNewInk", 0);


            debugs[krt].GetComponent<Renderer>().material.SetTexture("_MainTex", tex2);

        }
    }

    // Step 2: Boundary Evolution + Blocking Factor Update + ws Update
    // 1. Determine the boundary cell (a dry cell whose 8 neighbor cells do not have enough water to flow), and set its blocking factor to 1
    // 2. For the flow cell, set its block factor to be height field
    // 3. Update ws = max(ws - wf, 0)
    // 4. Update rho'
    void BoundaryUpdate()
    {
        // result to _rtc[3]
        Graphics.Blit(null, _rtc[3], _boundaryMat);
        // Copy the result to reference texture
        Graphics.Blit(_rtc[3], _rt[3]); 
    }

    // Step 3: Streaming
    // 1. Streaming on N, S, W, E direction, considering evaporation at boundary
    // 2. Streaming with NE, NW, SE, SW direction, considering evaporation at boundary
    // 3. Calculate: density with evaporation, velocity in two direction, wf
    void Streaming()
    {
        // Step 3.1
        Graphics.Blit(null, _rtc[0], _streamMat, 0);
        // Step 3.2
        Graphics.Blit(null, _rtc[1], _streamMat, 1);
        // Copy the result to reference textures
        Graphics.Blit(_rtc[0], _rt[0]);
        Graphics.Blit(_rtc[1], _rt[1]);

        // Step 3.3
        Graphics.Blit(null, _rtc[2], _rhoUpdateMat);
        Graphics.Blit(_rtc[2], _rt[2]);
    }

    // Step 4: Colliding
    // 1. Calculate new distribution functions for f1 - f4
    // 2. Calculate new distribution functions for f5 - f8
    // 3. Calculate new distribution function f0
    void Colliding()
    {
        Graphics.Blit(null, _rtc[0], _collideMat, 0);
        Graphics.Blit(null, _rtc[1], _collideMat, 1);
        Graphics.Blit(null, _rtc[3], _collideMat, 2);

        Graphics.Blit(_rtc[0], _rt[0]);
        Graphics.Blit(_rtc[1], _rt[1]);
        Graphics.Blit(_rtc[3], _rt[3]);
    }
    void Test(RenderTexture rt) 
    {
        Texture2D tex2 = new Texture2D(canvasSize, canvasSize);
        RenderTexture.active = rt;
        tex2.ReadPixels(new Rect(0, 0, canvasSize, canvasSize), 0, 0);
        tex2.Apply();
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < canvasSize; i++)
        {
            for (int j = 0; j < canvasSize; j++)
            {
                Color c = tex2.GetPixel(i, j);
                // if (c.g > 0f) {
                    sb.Append("(" + i + ", " + j + ", "+ c.g.ToString() + ") ");
                // }
                
            }
            sb.Append(" \n ");
        }
        print(sb);
        RenderTexture.active = null;
    }

    void SetUpRTs(int width, int height) 
    {
        _rt = new RenderTexture[krt];
        _rtc = new RenderTexture[krt];
        for (int i = 0; i < krt; i++)
        {
            _rt[i] = CreateRenderTexture(width, height);
            _rtc[i] = CreateRenderTexture(width, height);
            Graphics.Blit(null, _rt[i], _fillMat);
            Graphics.Blit(null, _rtc[i], _fillMat);

            // debugging
            displayRTs[i] = CreateRenderTexture(width, height); 
            Graphics.Blit(null, displayRTs[i], _fillMat);
            debugs[i].GetComponent<Renderer>().material.SetTexture("_MainTex", displayRTs[i]);
        }

        _driedLayer = CreateRenderTexture(width, height);
        _myRT = CreateRenderTexture(width, height);

        // Test Step1 by using random value for rho
        RandomTextureGenerator g = new RandomTextureGenerator(width, height);
        // Texture2D randomRho = g.GenerateRandomTexture(2);
        // Graphics.Blit(randomRho, _rt[2]);

        // Generate perlin noise for height field
        g.SetBounds(heightLowerBound, heightUpperBound);
        Texture2D perlinNoise = g.GeneratePerlinNoiseTexture(heightScale, 0);
        Graphics.Blit(perlinNoise, _rt[4]);

        // debugging


    }


    // Set up the reference parameters, such as textures, colors, and floating variables in the shaders
    void SetUpMats()
    {

        // Set fill color to be white
        // _fillMat.SetColor("_Color", Color.white);
        // Set reference textures in the materials
        // for mouseClick mat: 
        // _RefTex2: vx, vy, rho, wf --> _rt[2]
        // _RefTex3: f0, k, rho', ws --> _rt[3] 
        _mouseClickMat.SetTexture("_RefTex2", _rt[2]);
        _mouseClickMat.SetTexture("_RefTex3", _rt[3]);
        _mouseClickMat.SetTexture("_PrevTex", _myRT);

        // _boundaryMat takes three reference textures
        // _RefTex2: vx, vy, rho, wf --> _rt[2]
        // _RefTex3: f0, k, rho', ws --> _rt[3]
        // _RefTex4: h               --> _rt[4]
        _boundaryMat.SetTexture("_RefTex2", _rt[2]);
        _boundaryMat.SetTexture("_RefTex3", _rt[3]);
        _boundaryMat.SetTexture("_RefTex4", _rt[4]);

        // _StreamMat1 takes 3 reference textures
        // _RefTex0: f1, f2, f3, f4 --> _rt[0]
        // _RefTex1: f5, f6, f7, f8 --> _rt[1]
        // _RefTex3: f0, k, rho', ws --> _rt[3]
        _streamMat.SetTexture("_RefTex0", _rt[0]);
        _streamMat.SetTexture("_RefTex1", _rt[1]);
        _streamMat.SetTexture("_RefTex3", _rt[3]);

        // Stream step 3.3: update rho, velocity and wf
        // it takes 3 reference textures, and output to _rt[2]
        // _RefTex0: f1, f2, f3, f4 --> _rt[0]
        // _RefTex1: f5, f6, f7, f8 --> _rt[1]
        // _RefTex3: f0, k, rho', ws --> _rt[3]
        _rhoUpdateMat.SetTexture("_RefTex0", _rt[0]);
        _rhoUpdateMat.SetTexture("_RefTex1", _rt[1]);
        _rhoUpdateMat.SetTexture("_RefTex3", _rt[3]);

        // Collision step requires 2 reference textures
        // _RefTex2: vx, vy, rho, wf --> _rt[2]
        // _RefTex3: f0, k, rho', ws --> _rt[3] (pass 2 result)
        _collideMat.SetTexture("_RefTex2", _rt[2]);
        _collideMat.SetTexture("_RefTex3", _rt[3]);
    }

    // Check if the given color is in the _colorLayer key set. 
    // If exists, return the corresponding rt
    // else, store the given color in the _colorLayer with a newly created rt, and return this rt
    RenderTexture ContainsColor(Color c) 
    {
        if (_colorLayer.ContainsKey(c))
        {
            return _colorLayer[c];
        } else {
            RenderTexture temp = CreateRenderTexture(canvasSize, canvasSize);
            _colorLayer.Add(c, temp);
            return temp;
        }
    }

    void InitiateMats()
    {
        // Create materials from shaders
        _mouseClickMat = new Material(_mouseClickShader);
        _fillMat = new Material(_fillShader);
        _boundaryMat = new Material(_boundaryShader);
        _streamMat = new Material(_streamShader);
        _rhoUpdateMat = new Material(_rhoUpdateShader);
        _collideMat = new Material(_collideShader);

        // debugging
        _debugMat = new Material(_debugShader);
    }

    RenderTexture CreateRenderTexture (int width, int height) {
		RenderTexture rt = new RenderTexture(width, height, 0);
		rt.format = RenderTextureFormat.ARGBFloat;
		rt.wrapMode = TextureWrapMode.Repeat;
		rt.filterMode = FilterMode.Point;
		rt.Create();
        // Graphics.Blit(null, _rt[i], _fillMat);
		return rt;
	}
}
