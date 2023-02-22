using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MainFunction : MonoBehaviour
{
    public enum DrawShape
    {
        Circle = 0, Rectangle = 1
    };
    
    public GameObject debug1, debug2;
    public Shader paintShader, fillShader, boundaryShader, streamShader;
    // user input
    public int canvasSize = 256;
    public float drawWidth, drawHeight;
    [Range(0f, 1f)]
    public float RhoVal;
    public DrawShape drawShape = 0;
    [Range(0.001f, 0.999f)]
    public float heightUpperBound, heightLowerBound;
    public float heightScale;
    

    Material paintMat, fillMat, myMat, boundaryMat, streamMat;
    RenderTexture rt, debugRT1, debugRT2;
    bool isDragging;
    RaycastHit hitInfo = new RaycastHit();
    // Start is called before the first frame update
    void Start()
    {
        rt = CreateRenderTexture(canvasSize, canvasSize);
        debugRT1= CreateRenderTexture(canvasSize, canvasSize);
        debugRT2= CreateRenderTexture(canvasSize, canvasSize);

        // Generate Perlin Noise for height field
        RandomTextureGenerator g = new RandomTextureGenerator(canvasSize, canvasSize);
        g.SetBounds(heightLowerBound, heightUpperBound);
        Texture2D perlineNoise = g.GeneratePerlinNoiseTexture(heightScale);
        Graphics.Blit(perlineNoise, debugRT2);

        paintMat = new Material(paintShader);
        fillMat = new Material(fillShader);
        boundaryMat = new Material(boundaryShader);
        streamMat = new Material(streamShader);

        Graphics.Blit(null, rt, fillMat);
        // Graphics.Blit(null, debugRT1, fillMat);


        myMat = GetComponent<Renderer>().material;
        myMat.SetTexture("_MainTex", rt);

        debug1.GetComponent<Renderer>().material.SetTexture("_MainTex", debugRT1);
        debug2.GetComponent<Renderer>().material.SetTexture("_MainTex", debugRT2);
        
        paintMat.SetTexture("_PrevTex", rt);

        boundaryMat.SetTexture("_RefTex2", rt); // f2, f4
        boundaryMat.SetTexture("_RefTex3", debugRT1); // k
        boundaryMat.SetTexture("_RefTex4", debugRT2); // h

        streamMat.SetTexture("_RefTex0", rt); // f2, f4
        streamMat.SetTexture("_RefTex3", debugRT1); // k

    }

    // Update is called once per frame
    void Update()
    {
        MouseDragging();
        DetectBoundary();
        Streaming();
    }

    void Streaming()
    {
         RenderTexture temp = RenderTexture.GetTemporary(canvasSize, canvasSize, 0);
         Graphics.Blit(null, temp, streamMat);
         Graphics.Blit(temp, rt);
         RenderTexture.ReleaseTemporary(temp);
    }

    void DetectBoundary()
    {
         RenderTexture temp = RenderTexture.GetTemporary(canvasSize, canvasSize, 0);
         Graphics.Blit(null, temp, boundaryMat);
         Graphics.Blit(temp, debugRT1);
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
            Vector3 toCenter = mouseInWorld - transform.localPosition;
            float mx = (toCenter.x + (transform.localScale.x*0.5f)) / transform.localScale.x;  // assuming square
            float my = (toCenter.y + (transform.localScale.y*0.5f)) / transform.localScale.y;
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
            RenderTexture.ReleaseTemporary(temp);

        }        
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
