using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayTexture : MonoBehaviour
{
    public enum DisplayOption
    {
        R = 0, G = 1, B = 2, A = 3
    };
    public GameObject mainDisplay;
    public DisplayOption displayOption;
    public Shader debugShader;
    RenderTexture rt;
    Texture2D myTex;
    Material debugMat, myMat;

    // Start is called before the first frame update
    void Start()
    {
        Material mainMat = mainDisplay.GetComponent<Renderer>().material;
        Debug.Assert(mainMat != null);
        string[] names = mainMat.GetTexturePropertyNames();
        foreach (string name in names)
        {
            Texture2D tex = mainMat.GetTexture(name) as Texture2D;
            Debug.Log("Get Texture " + name);            
            Debug.Assert(tex != null);
        } 
        // myTex = mainDisplay.GetComponent<Renderer>().material.GetTexture("_MainTex") as Texture2D;
        myMat = GetComponent<Renderer>().material;
        // myTex = myMat.GetTexture("_MainTex");
        // Debug.Assert(myTex != null);
        rt = new RenderTexture(myTex.width, myTex.height, 16, RenderTextureFormat.ARGB32);
        
        // debugMat = new Material(Shader.Find("Custom/Debug"));
        debugMat = new Material(debugShader);
        debugMat.SetTexture("_MainTex", myTex);

        myMat.SetTexture("_MainTex", rt);
        
        
    }

    // Update is called once per frame
    void Update()
    {
        // if (myTex == null)
        // {
        //     if (myMat.GetTexture("_MainTex") != null)
        //     {
        //         myTex = myMat.GetTexture("_MainTex");
                
        //         initRT = new RenderTexture(myTex.width, myTex.height, 16, RenderTextureFormat.ARGB32);
        //         Graphics.Blit(myTex, initRT);
        
        //         debugMat.SetTexture("_MainTex", initRT);
        //     }
        // }
        RenderTexture temp = RenderTexture.GetTemporary(myTex.width, myTex.height, 0);
        Graphics.Blit(null, temp, debugMat, displayOption.GetHashCode());
        Graphics.Blit(rt, temp);
        RenderTexture.ReleaseTemporary(temp);
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
