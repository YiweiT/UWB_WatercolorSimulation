using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugerController : MonoBehaviour
{
    public enum DisplayOption
    {
        R = 0, G = 1, B = 2, A = 3
    };
    public DisplayOption display1, display2;
    public GameObject debug1, debug2;
    public Shader debugShader;
    Material debugMat;
    RenderTexture myRT, debugRT1, debugRT2;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(debugShader != null);
        Debug.Assert(debug1 != null);
        Debug.Assert(debug2 != null);

        

        Texture2D tex = GetComponent<Renderer>().material.GetTexture("_MainTex") as Texture2D;
        Debug.Assert(tex != null);

        myRT = CreateRenderTexture(tex.width, tex.height);
        debugRT1 = CreateRenderTexture(tex.width, tex.height);
        debugRT2 = CreateRenderTexture(tex.width, tex.height);

        Graphics.Blit(tex, myRT);

        debugMat = new Material(debugShader);
        debugMat.SetTexture("_MainTex", myRT);

        debug1.GetComponent<Renderer>().material.SetTexture("_MainTex", debugRT1);
        debug2.GetComponent<Renderer>().material.SetTexture("_MainTex", debugRT2);
    }

    // Update is called once per frame
    void Update()
    {
        Graphics.Blit(null, debugRT1, debugMat, display1.GetHashCode());
        Graphics.Blit(null, debugRT2, debugMat, display2.GetHashCode());
    
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
