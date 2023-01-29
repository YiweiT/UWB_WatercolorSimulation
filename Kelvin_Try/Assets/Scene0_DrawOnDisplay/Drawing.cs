using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drawing : MonoBehaviour
{
    public bool isDragging;
    RenderTexture _rt, _prt;  // current and previous rt
    
    public Shader paintShader;
    public Shader fillShader;

    public GameObject Show_RT, Show_PRT; // for showing _rt
    Texture2D white;
    
    Material _paintMat, _fillMat;
    // Start is called before the first frame update
    void Start()
    {
        isDragging = false;
        _rt = CreateRenderTexture(Screen.width, Screen.height);
        _prt = CreateRenderTexture(Screen.width, Screen.height);

        _paintMat = new Material(paintShader);
        _fillMat = new Material(fillShader);

        // init _rt by fillMat
        _fillMat.SetColor("_Color", Color.blue);
        Graphics.Blit(null, _rt, _fillMat);
        Graphics.Blit(null, _prt, _fillMat);

        _paintMat.SetColor("_PenCol", Color.red);
        _paintMat.SetFloat("_r", 0.01f);
        
        _paintMat.SetTexture("_PreviousState", _prt); // always paint onto the same one

        // debug: to see if _rt is probably drawn on
        Show_RT.GetComponent<Renderer>().material.SetTexture("_MainTex", _rt);
        Show_PRT.GetComponent<Renderer>().material.SetTexture("_MainTex", _prt);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            
            Vector3 mousePos = Input.mousePosition;
            float mx = mousePos.x / Screen.width;
            float my = mousePos.y / Screen.height;
            Debug.Log("x: " + mx + ", y: " + my);
            _paintMat.SetFloat("_x", mx);
            _paintMat.SetFloat("_y", my);
            _paintMat.SetInteger("_hasNewInk", 1);
            
            Graphics.Blit(null, _rt, _paintMat);
            Graphics.Blit(_rt, _prt); // save current
            _paintMat.SetInteger("_hasNewInk", 0);
        } 
        

    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(_rt, dest);
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
