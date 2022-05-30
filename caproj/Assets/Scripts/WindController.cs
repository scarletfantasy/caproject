using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class WindController : MonoBehaviour
{
    
    // Start is called before the first frame update
    public Texture3D wind;
    public RenderTexture windrt;
    public ComputeBuffer windbuffer;
    public RenderTexture backrt;
    public Vector3 size;
    public ComputeShader diffusion;
    public ComputeShader windinit;
    public ComputeShader advectforward;
    public ComputeShader buffertotex;
    public Material[] mats;
    public Camera camera;
    public bool step;
    public int[] tmp;
    private void OnButtonClicked()
    {
        Debug.Log("Clicked!");
    }
    void initcompute()
    {
        

        step = false;
        windrt = new RenderTexture(16, 16, 0, RenderTextureFormat.ARGB32);
        windrt.enableRandomWrite = true;
        windrt.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        windrt.volumeDepth = 16;
        windrt.isVolume = true;
        windrt.Create();

        backrt = new RenderTexture(16, 16, 0, RenderTextureFormat.ARGB32);
        backrt.enableRandomWrite = true;
        backrt.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        backrt.volumeDepth = 16;
        backrt.isVolume = true;
        backrt.Create();

        windbuffer = new ComputeBuffer(16 * 16 * 16*3, 4);
        tmp = new int[16*16*16*3];
        for(int i=0;i<16*16*16*3;++i)
        {
            tmp[i] = 0;
            
        }
        windbuffer.SetData(tmp);

        int kernel = windinit.FindKernel("CSMain");
        windinit.SetTexture(kernel, "Wind", windrt);
        windinit.SetTexture(kernel, "Back", backrt);
        windinit.SetInt("width", 16);
        windinit.SetInt("height", 16);
        windinit.SetInt("depth", 16);
        windinit.Dispatch(kernel, 2, 2, 2);



        kernel = diffusion.FindKernel("CSMain");
        diffusion.SetTexture(kernel, "Wind", windrt);  
        diffusion.SetInt( "width", 16);
        diffusion.SetInt( "height", 16);
        diffusion.SetInt("depth", 16);

        kernel = advectforward.FindKernel("CSMain");
        advectforward.SetTexture(kernel, "Wind", windrt);
        advectforward.SetTexture(kernel, "Result", backrt);
        advectforward.SetBuffer(kernel, "WindBuffer", windbuffer);
        advectforward.SetInt("width", 16);
        advectforward.SetInt("height", 16);
        advectforward.SetInt("depth", 16);
        advectforward.SetFloat("m_dt", 1.0f);

        kernel = buffertotex.FindKernel("CSMain");
        buffertotex.SetTexture(kernel, "Result", windrt);
        buffertotex.SetBuffer(kernel, "buffer", windbuffer);
        buffertotex.SetInt("width", 16);
        buffertotex.SetInt("height", 16);
        buffertotex.SetInt("depth", 16);
        buffertotex.SetFloat("m_dt", 1.0f);


    }
    void Start()
    {
        initcompute();
    }

    // Update is called once per frame
    void Update()
    {

        if(step)
        {
            
            int kernel = diffusion.FindKernel("CSMain");
            diffusion.Dispatch(kernel, 2, 2, 2);

            kernel = advectforward.FindKernel("CSMain");
            advectforward.Dispatch(kernel, 2, 2, 2);
            windbuffer.GetData(tmp);
            kernel = buffertotex.FindKernel("CSMain");
            buffertotex.Dispatch(kernel, 2, 2, 2);
            
            foreach(var mat in mats)
            {
                mat.SetTexture("_MyTex", windrt);
                mat.SetVector("_size", new Vector4(size.x,size.y,size.z, 0.0f));
            }
            step = false;
        }

        
    }

    

    private void OnDrawGizmos()
    {
        
        if(tmp.Length>0)
        {
            var colors = wind.GetPixels();
            for (var i = 0; i < wind.depth; ++i)
            {
                for (var j = 0; j < wind.height; ++j)
                {
                    for (var k = 0; k < wind.width; ++k)
                    {
                        var originpos = new Vector3(((float)i / wind.depth ) * size.z, ((float)j / wind.height ) * size.y, ((float)k / wind.width ) * size.x);
                        int off = (i * wind.width * wind.height + j * wind.width + k)*3;
                        Gizmos.DrawLine(originpos, originpos + new Vector3((float)tmp[off]/256.0f, (float)tmp[off+1] / 256.0f, (float)tmp[off+2] / 256.0f));
                        
                    }
                }
            }
        }
        
    }


}
