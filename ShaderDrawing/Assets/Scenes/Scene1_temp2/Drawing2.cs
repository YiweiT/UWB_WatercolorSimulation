using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Drawing2 : MonoBehaviour
{
    public bool isDragging;
    public RawImage displayImg;
    public Color _penColor = Color.black;
    public float _penSize = 0.02f;
    public RenderTexture _rt; // the render texture used to draw on screen in OnGUI function
    public Shader paintShader;
    public Shader fillShader;
    public Texture2D white;
    
  
    
    Material _paintMat, _fillMat;
    // Start is called before the first frame update
    void Start()
    {
        displayImg.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
        
        isDragging = false;
        _rt = CreateRenderTexture(Screen.width, Screen.height);
        _paintMat = new Material(paintShader);
        _fillMat = new Material(fillShader);
        Graphics.Blit(null, _rt, _fillMat);

        displayImg.texture = _rt;

        white = new Texture2D(1, 1);
		white.SetPixel(0, 0, Color.white);
        _paintMat.SetTexture("Texture", white);
        _paintMat.SetColor("PenColor", _penColor);
        _paintMat.SetFloat("Radius", _penSize);
    }

    // Update is called once per frame
    void Update()
    {
        _paintMat.SetColor("_PenCol", _penColor);
        _paintMat.SetFloat("_r", _penSize);
        if (Input.GetMouseButtonDown(0)) 
        {
            isDragging = true;
        } else if (Input.GetMouseButtonUp(0)) 
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 mousePos = Input.mousePosition;
            float mx = mousePos.x / Screen.width;
            float my = mousePos.y / Screen.height;
            Debug.Log("x: " + mx + ", y: " + my);
            _paintMat.SetFloat("_x", mx);
            _paintMat.SetFloat("_y", my);
            
            // Create a temp render texture
            RenderTexture temp = RenderTexture.GetTemporary(_rt.width, _rt.height, 0, RenderTextureFormat.Default);
            // _rt is the source render texture which maintains the previous drawing 
            // temp is the current dest render texture which will contain the current drawing 
            Graphics.Blit(_rt, temp, _paintMat);
            // empty _rt
            _rt.Release();
            // swap temp and _rt
            Graphics.Blit(temp, _rt);
            displayImg.texture = _rt;
            
        } 
        

    }

    // void OnRenderImage(RenderTexture src, RenderTexture dest)
    // {
    //     Graphics.Blit(src, _rt, _fillMat);
    //     Graphics.Blit(_rt, dest, _paintMat);
    // }
    // void OnGUI()
    // {
    //     if(!Event.current.type.Equals(EventType.Repaint)) return;
    //     Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _rt);
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
