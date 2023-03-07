using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullStream : MonoBehaviour
{
    public enum DrawShape
    {
        Circle = 0, Rectangle = 1
    };    

    public enum DisplayOption
    {
        R = 0, G = 1, B = 2, A = 3
    };

    public DisplayOption display1, display2;
    public GameObject blockFactor, heightField, mainDisplay, debug1, debug2;
    public Shader paintShader, fillShader, boundaryShader, streamShader, debugShader, streamShader2;
    // user input
    public int canvasSize = 256;
    public float drawWidth, drawHeight;
    [Range(0f, 1f)]
    public float RhoVal;
    public DrawShape drawShape = 0;
    [Range(0.001f, 0.999f)]
    public float heightUpperBound, heightLowerBound;
    public float heightScale;
    

    Material paintMat, fillMat, myMat, boundaryMat, streamMat, debugMat, streamMat2;
    RenderTexture rt, rt0, rt1, bfRT, hfRT, debugRT1, debugRT2, rho_vRT;
    RenderTexture[] rts;
    const int kRTs = 6;
    bool isDragging;
    RaycastHit hitInfo = new RaycastHit();
    // Start is called before the first frame update
    void Start()
    {
        // initiate materials
        paintMat = new Material(paintShader);
        fillMat = new Material(fillShader);
        boundaryMat = new Material(boundaryShader);
        streamMat = new Material(streamShader);
        streamMat2 = new Material(streamShader2);
        debugMat = new Material(debugShader);

        // rt = CreateRenderTexture(canvasSize, canvasSize);
        // rt0 = CreateRenderTexture(canvasSize, canvasSize); // f1 - f4
        // rt1 = CreateRenderTexture(canvasSize, canvasSize); // f5 - f8
        // bfRT= CreateRenderTexture(canvasSize, canvasSize);
        // hfRT= CreateRenderTexture(canvasSize, canvasSize);
        // debugRT1= CreateRenderTexture(canvasSize, canvasSize);
        // debugRT2= CreateRenderTexture(canvasSize, canvasSize);
        /// <summary>
        /// rts[] contains 6 rts
        /// 0: displaying painting strokes
        /// 1: f1 - f4
        /// 2: f5 - f8
        /// 3: vx, vy, rho
        /// 4: f0, k,
        /// 5: h
        /// </summary>
        rts = new RenderTexture[kRTs];
        for (int i = 0; i < kRTs; i++)
        {
            rts[i] = CreateRenderTexture(canvasSize, canvasSize);
            Graphics.Blit(null, rts[i], fillMat);
        }

        // Generate Perlin Noise for height field
        RandomTextureGenerator g = new RandomTextureGenerator(canvasSize, canvasSize);
        g.SetBounds(heightLowerBound, heightUpperBound);
        Texture2D perlineNoise = g.GeneratePerlinNoiseTexture(heightScale, 0);
        Graphics.Blit(perlineNoise, rts[kRTs-1]);


        // mainDisplay.GetComponent<Renderer>().material.SetTexture("_MainTex", rt);
        // blockFactor.GetComponent<Renderer>().material.SetTexture("_MainTex", bfRT);
        // heightField.GetComponent<Renderer>().material.SetTexture("_MainTex", hfRT);
        // debug1.GetComponent<Renderer>().material.SetTexture("_MainTex", debugRT1);
        // debug2.GetComponent<Renderer>().material.SetTexture("_MainTex", debugRT2);
        
        paintMat.SetTexture("_PrevTex0", rts[0]);
        paintMat.SetTexture("_PrevTex1", rts[1]);
        paintMat.SetTexture("_PrevTex3", rts[3]);

        boundaryMat.SetTexture("_RefTex2", rts[2]); // rho
        boundaryMat.SetTexture("_RefTex3", rts[3]); // k
        boundaryMat.SetTexture("_RefTex4", rts[5]); // h

        // streamMat.SetTexture("_RefTex0", rt); // f2, f4
        streamMat.SetTexture("_RefTex3", bfRT); // k

        streamMat2.SetTexture("_RefTex0", rt0); // f1 - f4
        streamMat2.SetTexture("_RefTex1", rt1); // f5 - f8
        streamMat2.SetTexture("_RefTex2", rt); // f0
        


    }

    // Update is called once per frame
    void Update()
    {
        MouseDragging();
        DetectBoundary();
        Streaming();
        Debugging();
    }

    void Debugging()
    {
        // debugging
        // debugMat.SetTexture("_MainTex", rt0); 
        // Graphics.Blit(null, debugRT1, debugMat, display1.GetHashCode());
        debugMat.SetTexture("_MainTex", rt1); 
        Graphics.Blit(null, debugRT2, debugMat, display2.GetHashCode());
        debugMat.SetTexture("_MainTex", null); 
    }

    void Streaming()
    {
         RenderTexture temp = RenderTexture.GetTemporary(canvasSize, canvasSize, 0);

         streamMat.SetTexture("_RefTex0", rt0); // f1 - f4
         Graphics.Blit(null, temp, streamMat, 0);
        //  Graphics.Blit(temp, rt);
         Graphics.Blit(temp, rt0); // f1 - f4
         RenderTexture.ReleaseTemporary(temp);

        streamMat.SetTexture("_RefTex0", rt1); 
         Graphics.Blit(null, temp, streamMat, 1);
         Graphics.Blit(temp, rt1); // f5 - f8        
         RenderTexture.ReleaseTemporary(temp);
    }

    void DetectBoundary()
    {
         RenderTexture temp = RenderTexture.GetTemporary(canvasSize, canvasSize, 0);
         Graphics.Blit(null, temp, boundaryMat);
         Graphics.Blit(temp, bfRT);
         RenderTexture.ReleaseTemporary(temp);
    }
    void MouseDragging() 
    {
        if(Input.GetMouseButtonDown(0)) {
			isDragging = true;
		} else if(Input.GetMouseButtonUp(0)) {
			isDragging = false;
		}

        // when mouse clicks on the drawing paper, new ink adding to the canvas
        if (isDragging) 
        {
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo)) {
                return;  // did not intersect
            }

            Vector3 mouseInWorld = hitInfo.point;

            // convert mouse position to UV space
            Vector3 toCenter = mouseInWorld - mainDisplay.transform.localPosition;
            
            float mx = (toCenter.x + (mainDisplay.transform.localScale.x*0.5f)) / mainDisplay.transform.localScale.x;  // assuming square
            float my = (toCenter.y + (mainDisplay.transform.localScale.y*0.5f)) / mainDisplay.transform.localScale.y;
            Debug.Log("(" + mx + ", " + my + ")");

            paintMat.SetFloat("_x", mx);
            paintMat.SetFloat("_y", my);
            paintMat.SetFloat("_w", drawWidth);
            paintMat.SetFloat("_h", drawHeight);
            paintMat.SetFloat("_val", RhoVal);
            // paintMat.SetInt("_hasNewInk", 1);
            
            RenderTexture temp = RenderTexture.GetTemporary(canvasSize, canvasSize, 0);
            Graphics.Blit(null, temp, paintMat, drawShape.GetHashCode());
            Graphics.Blit(temp, rt);
            Graphics.Blit(temp, rt0);
            Graphics.Blit(temp, rt1);

            RenderTexture.ReleaseTemporary(temp);

        }        
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
