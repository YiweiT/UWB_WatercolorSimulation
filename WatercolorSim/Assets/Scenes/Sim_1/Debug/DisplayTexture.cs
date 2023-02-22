using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayTexture : MonoBehaviour
{
    public enum DisplayOption
    {
        R = 0, G = 1, B = 2, A = 3
    };
    public DisplayOption displayOption;
    public Shader debugShader;
    RenderTexture rt, initRT;
    Texture myTex;
    Material debugMat, myMat;

    // Start is called before the first frame update
    void Start()
    {

        myMat = GetComponent<Renderer>().material;
        // myTex = myMat.GetTexture("_MainTex");


        debugMat = new Material(debugShader);



        
        
    }

    // Update is called once per frame
    void Update()
    {
        if (myTex == null)
        {
            if (myMat.GetTexture("_MainTex") != null)
            {
                myTex = myMat.GetTexture("_MainTex");
                rt = new RenderTexture(myTex.width, myTex.height, 16, RenderTextureFormat.ARGB32);
                initRT = new RenderTexture(myTex.width, myTex.height, 16, RenderTextureFormat.ARGB32);
                Graphics.Blit(myTex, initRT);
        
                debugMat.SetTexture("_MainTex", initRT);
            }
        }

        Graphics.Blit(null, rt, debugMat, displayOption.GetHashCode());
        myMat.SetTexture("_MainTex", rt);
    }
}
