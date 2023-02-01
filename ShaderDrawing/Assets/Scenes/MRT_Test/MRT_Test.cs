using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MRT 
{
    Color0 = 0,
    Color1 = 1
}

public class MRT_Test : MonoBehaviour
{
    public MRT displayOpt = MRT.Color0;
    public GameObject show_rt, show_prt, drawCanvas;
    public Shader fillShader, paintShader;
    public Color penColor0 = Color.red;
    public Color penColor1 = Color.black;
    public float penSize = 0.01f;
    public RenderTexture[] _mrt;
    RenderBuffer[] colorBuffers;
    Material _fillMat, _paintMat, _canvasMat;
    Camera tempCam;

    const int kmrt = 2;
    const int canvasSize = 2048;

    // Start is called before the first frame update
    void Start()
    {
        _mrt = new RenderTexture[kmrt];
        colorBuffers = new RenderBuffer[kmrt];

        _fillMat = new Material(fillShader);
        _paintMat = new Material(paintShader);

        // init _rt by fillMat
        _fillMat.SetColor("_Color", Color.white);


        // _paintMat.SetTexture("_PreviousState1", _mrt[1]);

        for (int i = 0; i < kmrt; i++)
        {
            _mrt[i] = CreateRenderTexture(canvasSize, canvasSize);
            _mrt[i].Create();
            // init mrt by fillMat
            Graphics.Blit(null, _mrt[i], _fillMat);
            colorBuffers[i] = _mrt[i].colorBuffer;
        }

        // init paintMat
        _paintMat.SetColor("_PenCol0", penColor0);
        _paintMat.SetColor("_PenCol1", penColor1);
        _paintMat.SetFloat("_r", penSize);
        _paintMat.SetTexture("_PreviousState0", _mrt[0]);

        // Camera.main.SetTargetBuffers(colorBuffers, _mrt[0].depthBuffer);

        _canvasMat = drawCanvas.GetComponent<Renderer>().material;
        _canvasMat.SetTexture("_MainTex", _mrt[displayOpt.GetHashCode()]);

        // debugging ...
        show_rt.GetComponent<Renderer>().material.SetTexture("_MainTex", _mrt[0]);
        show_prt.GetComponent<Renderer>().material.SetTexture("_MainTex", _mrt[1]);


    }

    // Update is called once per frame
    void Update()
    {
        _canvasMat.SetTexture("_MainTex", _mrt[displayOpt.GetHashCode()]);
    }

    RaycastHit hitInfo = new RaycastHit();
    void OnMouseDrag() { // requires the MeshCollider to be defined on the Quad       
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo)) {
            return;  // did not intersect
        }

        Vector3 mouseInWorld = hitInfo.point;

        // convert mouse position to UV space
        Vector3 toCenter = mouseInWorld - drawCanvas.transform.localPosition;
        float mx = (toCenter.x + (drawCanvas.transform.localScale.x*0.5f)) / drawCanvas.transform.localScale.x;  // assuming square
        float my = (toCenter.y + (drawCanvas.transform.localScale.y*0.5f)) / drawCanvas.transform.localScale.y;
        _paintMat.SetFloat("_x", mx);
        _paintMat.SetFloat("_y", my);
        Debug.Log("Click: x=" + mx + " y=" + my);

        // // Additional Cam trying: https://forum.unity.com/threads/multiple-render-target-mrt-questions.262262/
        // if (tempCam == null) {
        //     GameObject go = new GameObject("Temp Camera", typeof(Camera));
        //     tempCam = go.GetComponent<Camera>();
        //     tempCam.enabled = false;
        // }

        // tempCam.CopyFrom(Camera.main);
        // tempCam.clearFlags = CameraClearFlags.SolidColor;
        // tempCam.backgroundColor = Color.black;

        // tempCam.SetTargetBuffers(colorBuffers, _mrt[0].depthBuffer);

        // HasNewInk = true;
        _paintMat.SetInt("_hasNewInk", 1); // this is point-less for now
        // Graphics.Blit(null, _rt, _paintMat); // draw with paintMat
        // Graphics.Blit(_rt, _prt); // make a copy of what we currently have
        _paintMat.SetInt("_hasNewInk", 0);
    }


    RenderTexture CreateRenderTexture (int width, int height) {
		RenderTexture rt = new RenderTexture(width, height, 0);
		rt.format = RenderTextureFormat.ARGBFloat;
		rt.wrapMode = TextureWrapMode.Repeat;
		rt.filterMode = FilterMode.Point;
		rt.Create();
		return rt;
	}

    /// <summary>
    /// Renders into a array of render textures using multi-target blit.
    /// Up to 4 render targets are supported in Unity but some GPU's can
    /// support up to 8 so this may change in the future. You MUST set up
    /// the materials shader correctly for multitarget blit for this to work.
    /// </summary>
    /// <param name="des">The destination render textures.</param>
    /// <param name="mat">The amterial to use</param>
    /// <param name="pass">Which pass of the materials shader to use.</param>
    public void MultiTargetBlit(RenderTexture[] des, Material mat, int pass = 0)
    {
        RenderBuffer[] rb = new RenderBuffer[des.Length];
 
        // Set targets needs the color buffers so make a array from
        // each textures buffer.
        for(int i = 0; i < des.Length; i++)
            rb[i] = des[i].colorBuffer;
 
        //Set the targets to render into.
        //Will use the depth buffer of the
        //first render texture provided.
        Graphics.SetRenderTarget(rb, des[0].depthBuffer);
 
        GL.Clear(true, true, Color.clear);
 
        GL.PushMatrix();
        GL.LoadOrtho();
     
        mat.SetPass(pass);
 
        GL.Begin(GL.QUADS);
        GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(0.0f, 0.0f, 0.1f);
        GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1.0f, 0.0f, 0.1f);
        GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, 0.1f);
        GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(0.0f, 1.0f, 0.1f);
        GL.End();
 
        GL.PopMatrix();
 
    }
}
