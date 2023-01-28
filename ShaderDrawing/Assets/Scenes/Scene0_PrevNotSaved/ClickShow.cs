using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickShow : MonoBehaviour
{
    public bool isDragging;
    public RenderTexture _rt;
    public Material _paintMat;
    public Material _fillMat;
    public Texture2D white;
    // Start is called before the first frame update
    void Start()
    {
        isDragging = false;
        // _rt = CreateRenderTexture(Screen.width, Screen.height);
        // Graphics.Blit(null, _rt, _fillMat);
        white = new Texture2D(1, 1);
		white.SetPixel(0, 0, Color.white);
        _paintMat.SetTexture("Texture", white);
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

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest, _paintMat);
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
