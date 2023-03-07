using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DisplayOpt
{
    R = 0, G = 1, B = 2, A = 3
};
public class Operator : MonoBehaviour
{
    public Shader fillShader, paintShader, boundaryShader, streamShader, rho_vUpdateShader, collideShader;

    Material fillMat, paintMat, boundaryMat, streamMat, rho_vUpdateMat, collideMat;
    public GameObject mainDisplay;
    public float size = 0.05f;
    public int canvasSize = 256;
    [Range(0f, 1f)]
    public float paintVal = 0.1f;
    [Range(0.001f, 0.999f)]
    public float heightUpperBound, heightLowerBound;
    public float heightScale;
    const int krt = 4;
    RenderTexture[] rt = new RenderTexture[krt];
    RenderTexture[] rtc = new RenderTexture[krt];
    RenderTexture myRT, myRTc;
    bool isDragging;
    RaycastHit hitInfo = new RaycastHit();

    // debugging ...
    public Shader debugShader;
    Material debugMat;
    public DisplayOpt[] displayOpt = new DisplayOpt[krt];
    public GameObject[] debugDisplay = new GameObject[krt];
    RenderTexture[] debugRT = new RenderTexture[krt];

    void RTSetUp()
    {

        myRT = CreateRenderTexture(canvasSize, canvasSize);
        myRTc = CreateRenderTexture(canvasSize, canvasSize);
        Graphics.Blit(null, myRT, fillMat);
        Graphics.Blit(null, myRTc, fillMat);
        mainDisplay.GetComponent<Renderer>().material.SetTexture("_MainTex", myRT);

        for (int i = 0; i < krt; i++)
        {
            rt[i] = CreateRenderTexture(canvasSize, canvasSize);
            rtc[i] = CreateRenderTexture(canvasSize, canvasSize);
            debugRT[i] = CreateRenderTexture(canvasSize, canvasSize);

            Graphics.Blit(null, rt[i], fillMat);
            Graphics.Blit(null, rtc[i], fillMat);
            Graphics.Blit(null, debugRT[i], fillMat);
            
        }   

        // Generate height field
        RandomTextureGenerator g = new RandomTextureGenerator(canvasSize, canvasSize);
        g.SetBounds(heightLowerBound, heightUpperBound);
        Texture2D perlinNoise = g.GeneratePerlinNoiseTexture(heightScale, 2);
        Graphics.Blit(perlinNoise, rt[3]);
        Graphics.Blit(perlinNoise, rtc[3]);

        // debugging ...
        for (int i = 0; i < krt; i++)
        {
            debugDisplay[i].GetComponent<Renderer>().material.SetTexture("_MainTex", rt[i]);
        }
    }

    void MatInitiate()
    {
        fillMat = new Material(fillShader);
        paintMat = new Material(paintShader);
        boundaryMat = new Material(boundaryShader );
        streamMat = new Material(streamShader);
        rho_vUpdateMat = new Material(rho_vUpdateShader);
        collideMat = new Material(collideShader);

        // debugging ...
        debugMat = new Material(debugShader);
    }

    void MatSetUp()
    {
        paintMat.SetTexture("_mask", myRT);
        paintMat.SetTexture("_tex0", rt[0]);
        paintMat.SetTexture("_tex1", rt[1]);
        paintMat.SetTexture("_tex3", rt[3]);

        boundaryMat.SetTexture("_tex2", rt[2]);
        boundaryMat.SetTexture("_tex3", rt[3]);

        streamMat.SetTexture("_tex0", rt[0]);
        streamMat.SetTexture("_tex1", rt[1]);
        streamMat.SetTexture("_tex3", rt[3]);

        rho_vUpdateMat.SetTexture("_tex0", rt[0]);
        rho_vUpdateMat.SetTexture("_tex1", rt[1]);
        rho_vUpdateMat.SetTexture("_tex3", rt[3]);

        collideMat.SetTexture("_RefTex2", rt[2]);
        collideMat.SetTexture("_RefTex3", rt[3]);

          
    }
    // Start is called before the first frame update
    void Start()
    {
        MatInitiate();
        RTSetUp();
        MatSetUp();
    }

    // Update is called once per frame
    void Update()
    {
        MouseDragging();
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
            debugMat.SetTexture("_MainTex", rt[i]);
            Graphics.Blit(null, debugRT[i], debugMat, displayOpt[i].GetHashCode());
        }
    }

    void Colliding()
    {
        
        Graphics.Blit(null, rtc[0], collideMat, 0);
        Graphics.Blit(null, rtc[1], collideMat, 1);       
        Graphics.Blit(null, rtc[3], collideMat, 2);

        Graphics.Blit(rtc[0], rt[0]);
        Graphics.Blit(rtc[1], rt[1]);
        Graphics.Blit(rtc[3], rt[3]);

    }

    void Streaming()
    {
        Graphics.Blit(null, rtc[0], streamMat, 0);
        Graphics.Blit(null, rtc[1], streamMat, 1);

        Graphics.Blit(rtc[0], rt[0]);   
        Graphics.Blit(rtc[1], rt[1]); 

        Graphics.Blit(null, rtc[2], rho_vUpdateMat);
        Graphics.Blit(rtc[2], rt[2]);
    }

    void BoundaryUpdate()
    {
        Graphics.Blit(null, rtc[3], boundaryMat);
        Graphics.Blit(rtc[3], rt[3]);
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
            paintMat.SetFloat("_size", size);
            paintMat.SetFloat("_val", paintVal);

            
            // paintMat.SetTexture("_tex01", rt[0]);
            Graphics.Blit(null, rtc[0], paintMat, 0);
            Graphics.Blit(rtc[0], rt[0]);   
            // RenderTexture.ReleaseTemporary(temp);

            // paintMat.SetTexture("_tex01", rt[1]);
            Graphics.Blit(null, rtc[1], paintMat, 1);
            Graphics.Blit(rtc[1], rt[1]);   

            Graphics.Blit(null, myRTc, paintMat, 2);
            Graphics.Blit(myRTc, myRT);

            Graphics.Blit(null, rtc[3], paintMat, 3);
            Graphics.Blit(rtc[3], rt[3]);   
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
