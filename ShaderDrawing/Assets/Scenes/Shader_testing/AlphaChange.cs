using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlphaChange : MonoBehaviour
{
    public Slider r, g, b, a;
    public Shader alphaShader;
    public int rtSize = 500;
    RenderTexture rt;
    Material displayMat, alphaMat;
    

    // Start is called before the first frame update
    void Start()
    {
        displayMat = GetComponent<Renderer>().material;
        
        rt = CreateRenderTexture(rtSize, rtSize);
        displayMat.SetTexture("_MainTex", rt);
        alphaMat = new Material(alphaShader);
        // displayMat = alphaMat;

    }

    // Update is called once per frame
    void Update()
    {
        alphaMat.SetFloat("r", r.value);
        alphaMat.SetFloat("g", g.value);
        alphaMat.SetFloat("b", b.value);
        alphaMat.SetFloat("a", a.value);
        
        Graphics.Blit(null, rt, alphaMat);

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
