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
    public RenderTexture windrtix;
    public RenderTexture windrtiy;
    public RenderTexture windrtiz;
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
    public RenderTexture creatert3d(int width,int height,int depth,RenderTextureFormat format)
    {
        var myrt = new RenderTexture(width, height, 0, format);
        myrt.enableRandomWrite = true;
        myrt.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        myrt.volumeDepth = depth;
        myrt.isVolume = true;
        myrt.Create();
        return myrt;
    }
    void initcompute()
    {
        

        step = false;
        windrt = creatert3d((int)size.x, (int)size.y, (int)size.z, RenderTextureFormat.ARGBFloat);
        
        backrt = creatert3d((int)size.x, (int)size.y, (int)size.z, RenderTextureFormat.ARGBFloat);

        windrtix = creatert3d((int)size.x, (int)size.y, (int)size.z, RenderTextureFormat.RInt);

        windrtiy = creatert3d((int)size.x, (int)size.y, (int)size.z, RenderTextureFormat.RInt);

        windrtiz = creatert3d((int)size.x, (int)size.y, (int)size.z, RenderTextureFormat.RInt);
        foreach (var mat in mats)
        {
            mat.SetTexture("_MyTex", windrt);
            mat.SetVector("_size", new Vector4(size.x, size.y, size.z, 0.0f));
        }

        windbuffer = new ComputeBuffer(16 * 16 * 16*3, 4);
        tmp = new int[16*16*16*3];
        for(int i=0;i<16*16*16;++i)
        {
            tmp[i*3] = 0;
            tmp[i * 3+1] = 0;
            tmp[i * 3+2] = 0;

        }
        windbuffer.SetData(tmp);

        int kernel = windinit.FindKernel("CSMain");
        windinit.SetTexture(kernel, "Wind", windrt);
        windinit.SetTexture(kernel, "Back", backrt);
        windinit.SetTexture(kernel, "Windix", windrtix);
        windinit.SetTexture(kernel, "Windiy", windrtiy);
        windinit.SetTexture(kernel, "Windiz", windrtiz);
        windinit.SetInt("width", 16);
        windinit.SetInt("height", 16);
        windinit.SetInt("depth", 16);
        windinit.Dispatch(kernel, 2, 2, 2);



        kernel = diffusion.FindKernel("CSMain");
        diffusion.SetTexture(kernel, "Wind", windrt);  
        diffusion.SetInt( "width", 16);
        diffusion.SetInt( "height", 16);
        diffusion.SetInt("depth", 16);

        //kernel = advectforward.FindKernel("CSMain");
        //advectforward.SetTexture(kernel, "Wind", windrt);
        //advectforward.SetTexture(kernel, "Result", backrt);
        //advectforward.SetBuffer(kernel, "WindBuffer", windbuffer);
        //advectforward.SetInt("width", 16);
        //advectforward.SetInt("height", 16);
        //advectforward.SetInt("depth", 16);
        //advectforward.SetFloat("m_dt", 1.0f);

        //kernel = buffertotex.FindKernel("CSMain");
        //buffertotex.SetTexture(kernel, "Result", windrt);
        //buffertotex.SetBuffer(kernel, "buffer", windbuffer);
        //buffertotex.SetInt("width", 16);
        //buffertotex.SetInt("height", 16);
        //buffertotex.SetInt("depth", 16);
        //buffertotex.SetFloat("m_dt", 1.0f);

        kernel = advectforward.FindKernel("CSMain");
        advectforward.SetTexture(kernel, "Wind", windrt);
        advectforward.SetTexture(kernel, "Result", backrt);
        advectforward.SetTexture(kernel, "Windix", windrtix);
        advectforward.SetTexture(kernel, "Windiy", windrtiy);
        advectforward.SetTexture(kernel, "Windiz", windrtiz);
        advectforward.SetInt("width", 16);
        advectforward.SetInt("height", 16);
        advectforward.SetInt("depth", 16);
        advectforward.SetFloat("m_dt", 1.0f);

        kernel = buffertotex.FindKernel("CSMain");
        buffertotex.SetTexture(kernel, "Wind", windrt);
        buffertotex.SetTexture(kernel, "Windix", windrtix);
        buffertotex.SetTexture(kernel, "Windiy", windrtiy);
        buffertotex.SetTexture(kernel, "Windiz", windrtiz);
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

        //if(step)
        {
            
            int kernel = diffusion.FindKernel("CSMain");
            diffusion.Dispatch(kernel, 2, 2, 2);

            kernel = advectforward.FindKernel("CSMain");
            advectforward.Dispatch(kernel, 2, 2, 2);
            //windbuffer.GetData(tmp);
            kernel = buffertotex.FindKernel("CSMain");
            buffertotex.Dispatch(kernel, 2, 2, 2);
            
            
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
