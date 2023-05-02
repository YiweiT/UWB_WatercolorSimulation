using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Scrtwpns.Mixbox;

public class colorMix : MonoBehaviour
{
    public enum Mode {
        MixboxSlider = 0,
        LerpSlider = 1,
        Shader = 2
    };
    public Mode mode;
    public Slider ratio;
    public Text ratioText;
    public RawImage col1Img, col2Img, display;
    public Color color1 = Color.blue;
    public Color color2 = Color.yellow;
    public Shader shader;
    Material mat;
    RenderTexture rt;
    int width, height;

    
    // public float ratio = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        display = this.GetComponent<RawImage>();
        ratio.value = 0.5f;
        if (mode.GetHashCode() == 2)
        {
            mat = new Material(shader);
            mat.SetColor("_Color1", color1);
            mat.SetColor("_Color2", color2);
            width = (int) display.rectTransform.rect.width;
            height = (int) display.rectTransform.rect.height;
            
            rt = CreateRenderTexture(width, height);
        }

        
        
        
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
        col1Img.color = color1;
        col2Img.color = color2;


        ratioText.text = ratio.value.ToString();
        if (mode.GetHashCode() == 0) {
            // mixbox lerp using slider
            display.texture = null;
            Color colorMix = Mixbox.Lerp(color1, color2, ratio.value);
            display.color = colorMix;
            
            // ratioText.text = ratio.value.ToString();
        } else if (mode.GetHashCode() == 1)
        {
            // color lerp using slider
            display.texture = null;
            display.color = Color.Lerp(color1, color2, ratio.value);
        } 
        else if (mode.GetHashCode() == 2)
        {
            // color lerp in shader
            mat.SetColor("_Color1", color1);
            mat.SetColor("_Color2", color2);
            Graphics.Blit(null, rt, mat, 0);
            display.texture = rt;
        } 
        
        
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
