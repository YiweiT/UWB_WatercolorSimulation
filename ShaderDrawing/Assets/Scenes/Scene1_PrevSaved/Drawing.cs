using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drawing : MonoBehaviour
{
    public bool isDragging;
    public RenderTexture _rt;
    public Shader paintShader;
    public Shader fillShader;
    public Texture2D white;
    
    Material _paintMat, _fillMat;
    // Start is called before the first frame update
    void Start()
    {
        isDragging = false;
        _rt = CreateRenderTexture(Screen.width, Screen.height);
        _paintMat = new Material(paintShader);
        _fillMat = new Material(fillShader);
        Graphics.Blit(null, _rt, _fillMat);
        // white = new Texture2D(1, 1);
		// white.SetPixel(0, 0, Color.white);
        // _paintMat.SetTexture("Texture", white);
    }

    // Update is called once per frame
    void Update()
    {
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
            
            // Graphics.Blit(null, _rt, _paintMat);
            // RenderTexture temp = RenderTexture.GetTemporary(_rt.width, _rt.height, 0, RenderTextureFormat.Default);
            // Graphics.Blit(_rt, temp, _paintMat);
            // Graphics.Blit(temp, _rt);
            // RenderTexture.ReleaseTemporary(temp);
        } 
        

    }

    // void OnRenderImage(RenderTexture src, RenderTexture dest)
    // {
    //     Graphics.Blit(src, _rt, _fillMat);
    //     Graphics.Blit(_rt, dest, _paintMat);
    // }
    void OnGUI()
    {
        
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
