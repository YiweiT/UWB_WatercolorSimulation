using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptOnQuad : MonoBehaviour
{
    // Quad has a SimpleShowTexture mat/shader

    RenderTexture _rt, _prt;  // current and previous rt
    
    public Shader fillShader;   // from Scene0
    public Shader paintShader; // from Scene0

    // public GameObject Show_RT, Show_PRT; // for showing _rt
    Texture2D white;
    
    Material _fillMat, _paintMat, _myMat;
    // Start is called before the first frame update

    // for debugging
    public GameObject Show_RT, Show_PRT;

    // **************** for drawing support
    // Assuming this Quad is square
    const int CanvasSize = 2048; // too large?
    // bool HasNewInk = false;

    void Start()
    {
        _rt = CreateRenderTexture(CanvasSize, CanvasSize);
        _prt = CreateRenderTexture(CanvasSize, CanvasSize);

        _fillMat = new Material(fillShader);
        _paintMat = new Material(paintShader);

        // init _rt by fillMat
        _fillMat.SetColor("_Color", Color.blue);
        Graphics.Blit(null, _rt, _fillMat);
        Graphics.Blit(null, _prt, _fillMat);

        _paintMat.SetColor("_PenCol", Color.red);
        _paintMat.SetFloat("_r", 0.01f);
        _paintMat.SetTexture("_PreviousState", _prt); // always paint onto the same one

        _myMat = GetComponent<Renderer>().material;
        _myMat.SetTexture("_MainTex", _rt);  // we simply visualize _rt

        // debugging ...
        Show_RT.GetComponent<Renderer>().material.SetTexture("_MainTex", _rt);
        Show_PRT.GetComponent<Renderer>().material.SetTexture("_MainTex", _prt);
    }

    // Update is called once per frame
    // void Update() { }

    // void OnPostRender() { }

    RaycastHit hitInfo = new RaycastHit();
    void OnMouseDrag() { // requires the MeshCollider to be defined on the Quad       
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
        _paintMat.SetInteger("_hasNewInk", 1); // this is point-less for now
        Graphics.Blit(null, _rt, _paintMat); // draw with paintMat
        Graphics.Blit(_rt, _prt); // make a copy of what we currently have
        _paintMat.SetInteger("_hasNewInk", 0);
    }

    RenderTexture CreateRenderTexture (int width, int height) {
		RenderTexture rt = new RenderTexture(width, height, 0);
		rt.format = RenderTextureFormat.ARGBFloat;
		rt.wrapMode = TextureWrapMode.Repeat;
		rt.filterMode = FilterMode.Point;
		rt.Create();
		return rt;
	}
}
