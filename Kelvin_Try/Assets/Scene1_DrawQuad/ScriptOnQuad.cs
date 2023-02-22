using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptOnQuad : MonoBehaviour
{
    // Quad has a SimpleShowTexture mat/shader

    //RenderTexture _rt, _prt;  // current and previous rt
    
    public Shader fillShader;   // from Scene0
    public Shader paintShader, boundaryShader; // from Scene0
    public float brushSize = 0.02f;

    // public GameObject Show_RT, Show_PRT; // for showing _rt
    Texture2D white;
    bool isDragging;
    
    Material _fillMat, _paintMat, _myMat, _boundaryMat;
    
    RenderTexture[] _rts; 
    RenderTexture[] _rtc; // copies of _rts, used as result rts 

    // for debugging
    public GameObject Show_RT, Show_PRT;

    // **************** for drawing support
    // Assuming this Quad is square
    const int CanvasSize = 100; // too large?
    // bool HasNewInk = false;

    void Start()
    {
        _fillMat = new Material(fillShader);
        _paintMat = new Material(paintShader);
        _boundaryMat = new Material(boundaryShader);

        // init _rt by fillMat
        _fillMat.SetColor("_Color", Color.white);

        // _rt = CreateRenderTexture(CanvasSize, CanvasSize);
        // _prt = CreateRenderTexture(CanvasSize, CanvasSize);

                _rts = new RenderTexture[3];
        _rtc = new RenderTexture[3];
        for (int i = 0; i < 3; i++)
        {
            _rts[i] = CreateRenderTexture(CanvasSize, CanvasSize);
            _rtc[i] = CreateRenderTexture(CanvasSize, CanvasSize);

            Graphics.Blit(null, _rts[i], _fillMat);
            Graphics.Blit(null, _rtc[i], _fillMat);

        }




        // Graphics.Blit(null, _rt, _fillMat);
        // Graphics.Blit(null, _prt, _fillMat);

        _paintMat.SetColor("_PenCol", Color.red);
        _paintMat.SetFloat("_brushSize", brushSize);
        _paintMat.SetTexture("_PreviousState", _rts[2]); // always paint onto the same one
        _paintMat.SetTexture("_tex1", _rts[0]);
        _paintMat.SetTexture("_tex2", _rts[1]);

        _myMat = GetComponent<Renderer>().material;
        _myMat.SetTexture("_MainTex", _rts[2]);  // we simply visualize _rt

        // debugging ...
        Show_RT.GetComponent<Renderer>().material.SetTexture("_MainTex", _rts[0]);
        Show_PRT.GetComponent<Renderer>().material.SetTexture("_MainTex", _rts[1]);
    }

    // Update is called once per frame
    void Update() 
    { 
        if(Input.GetMouseButtonDown(0)) {
			isDragging = true;
		} else if(Input.GetMouseButtonUp(0)) {
			isDragging = false;
		}

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
            _paintMat.SetFloat("_x", mx);
            _paintMat.SetFloat("_y", my);
            Debug.Log("Click: x=" + mx + " y=" + my);
            // HasNewInk = true;
            _paintMat.SetInt("_hasNewInk", 1); // this is point-less for now

            // drawing on canvas
            RenderTexture temp = RenderTexture.GetTemporary(CanvasSize, CanvasSize);
            // Graphics.Blit(null, _rt, _paintMat, 1); // draw with paintMat
            // Graphics.Blit(_rt, _prt); // make a copy of what we currently have
            Graphics.Blit(null, temp, _paintMat, 0); // draw with paintMat
             // make a copy of what we currently have

            // update _rts[0] through pass 0
            _paintMat.SetFloat("_x", mx);
            _paintMat.SetFloat("_y", my);
            _paintMat.SetInt("_hasNewInk", 1); 
            Graphics.Blit(null, _rtc[0], _paintMat, 0);
            

            // update _rts[1] through pass 1
            
            Graphics.Blit(null, _rtc[1], _paintMat, 1);
            

            //swap
            Graphics.Blit(_rtc[0], _rts[0]);
            Graphics.Blit(_rtc[1], _rts[1]);
            Graphics.Blit(temp, _rts[2]);
            _paintMat.SetInt("_hasNewInk", 0);
            RenderTexture.ReleaseTemporary(temp);
        }
    }

    // void OnPostRender() { }

    RaycastHit hitInfo = new RaycastHit();
    // void OnMouseDrag() { // requires the MeshCollider to be defined on the Quad       
    //     if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo)) {
    //         return;  // did not intersect
    //     }

    //     Vector3 mouseInWorld = hitInfo.point;

    //     // convert mouse position to UV space
    //     Vector3 toCenter = mouseInWorld - transform.localPosition;
    //     float mx = (toCenter.x + (transform.localScale.x*0.5f)) / transform.localScale.x;  // assuming square
    //     float my = (toCenter.y + (transform.localScale.y*0.5f)) / transform.localScale.y;
    //     _paintMat.SetFloat("_x", mx);
    //     _paintMat.SetFloat("_y", my);
    //     Debug.Log("Click: x=" + mx + " y=" + my);
    //     // HasNewInk = true;
    //     _paintMat.SetInt("_hasNewInk", 1); // this is point-less for now
    //     Graphics.Blit(null, _rt, _paintMat); // draw with paintMat
    //     Graphics.Blit(_rt, _prt); // make a copy of what we currently have
    //     _paintMat.SetInt("_hasNewInk", 0);
    // }
    

    RenderTexture CreateRenderTexture (int width, int height) {
		RenderTexture rt = new RenderTexture(width, height, 0);
		rt.format = RenderTextureFormat.ARGBFloat;
		rt.wrapMode = TextureWrapMode.Repeat;
		rt.filterMode = FilterMode.Point;
		rt.Create();
		return rt;
	}
}
