using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class WindController : MonoBehaviour
{

    // Start is called before the first frame update
    [Range(1.0f, 50.0f)] public float stiffness=10.0f;
    [Range(0.5f,5.0f)] public float stretch = 1.0f;

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
    public ComputeShader texitotex;
    public ComputeShader textobuffer;
    public ComputeShader advectbackward;
    public ComputeShader windgen;
    public ComputeShader obstacle;
    public ComputeShader applynoise;
    public Material[] mats;
    public Camera camera;
    public Texture noise;
    public bool step;
    public bool enablestep;
    public bool genonce;
    public float enableobstacle;
    private bool gen;
    public float m_dt;
    public bool enabledebug;
    public int[] tmp;
    public int numtrees;
    int numthreadsx = 8;
    int numthreadsy = 8;
    int numthreadsz = 8;
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
        windinit.SetInt("width", (int)size.x);
        windinit.SetInt("height", (int)size.y);
        windinit.SetInt("depth", (int)size.z);
        windinit.Dispatch(kernel, (int)size.x/numthreadsx, (int)size.y / numthreadsy, (int)size.z / numthreadsz);



        kernel = diffusion.FindKernel("CSMain");
        diffusion.SetTexture(kernel, "Wind", windrt);  
        diffusion.SetInt( "width", (int)size.x);
        diffusion.SetInt( "height", (int)size.y);
        diffusion.SetInt("depth", (int)size.z);

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

        kernel = windgen.FindKernel("CSMain");
        windgen.SetTexture(kernel, "Wind", windrt);
        

        kernel = advectforward.FindKernel("CSMain");
        advectforward.SetTexture(kernel, "Wind", windrt);
        advectforward.SetTexture(kernel, "Result", backrt);
        advectforward.SetTexture(kernel, "Windix", windrtix);
        advectforward.SetTexture(kernel, "Windiy", windrtiy);
        advectforward.SetTexture(kernel, "Windiz", windrtiz);
        advectforward.SetInt("width", (int)size.x);
        advectforward.SetInt("height", (int)size.y);
        advectforward.SetInt("depth", (int)size.z);
        advectforward.SetFloat("m_dt", m_dt);

        kernel = advectbackward.FindKernel("CSMain");
        advectbackward.SetTexture(kernel, "Wind", windrt);
        advectbackward.SetInt("width", (int)size.x);
        advectbackward.SetInt("height", (int)size.y);
        advectbackward.SetInt("depth", (int)size.z);
        advectbackward.SetFloat("m_dt", m_dt);

        kernel = texitotex.FindKernel("CSMain");
        texitotex.SetTexture(kernel, "Wind", windrt);
        texitotex.SetTexture(kernel, "Windix", windrtix);
        texitotex.SetTexture(kernel, "Windiy", windrtiy);
        texitotex.SetTexture(kernel, "Windiz", windrtiz);
        texitotex.SetInt("width", (int)size.x);
        texitotex.SetInt("height", (int)size.y);
        texitotex.SetInt("depth", (int)size.z);
        texitotex.SetFloat("m_dt", m_dt);

        kernel = textobuffer.FindKernel("CSMain");
        textobuffer.SetTexture(kernel, "src", windrt);
        textobuffer.SetBuffer(kernel, "dst", windbuffer);
        textobuffer.SetInt("width", (int)size.x);
        textobuffer.SetInt("height", (int)size.y);
        textobuffer.SetInt("depth", (int)size.z);
        textobuffer.SetFloat("m_dt", m_dt);

        numtrees = 64;
        ComputeBuffer bbmin = new ComputeBuffer(numtrees, 16);
        float[] bbmindata = new float[numtrees*4];
        ComputeBuffer bbmax = new ComputeBuffer(numtrees, 16);
        float[] bbmaxdata = new float[numtrees * 4];
        for (int i=0;i<8;++i)
        {
            for(int j=0;j<8;++j)
            {

                bbmindata[(i * 8 + j)*4] = i*2-0.5f;
                bbmindata[(i * 8 + j) * 4 + 1] = 1;
                bbmindata[(i * 8 + j) * 4 + 2] = j * 2 - 0.5f;
                bbmindata[(i * 8 + j) * 4 + 3] = 0;
                bbmaxdata[(i * 8 + j) * 4] = i * 2+0.5f;
                bbmaxdata[(i * 8 + j) * 4 + 1] = 3;
                bbmaxdata[(i * 8 + j) * 4 + 2] = j * 2 + 0.5f;
                bbmaxdata[(i * 8 + j) * 4 + 3] = 0;

            }
        }
        bbmin.SetData(bbmindata);
        bbmax.SetData(bbmaxdata);
        kernel = obstacle.FindKernel("CSMain");
        obstacle.SetTexture(kernel, "Wind", windrt);
        obstacle.SetBuffer(kernel, "mincs", bbmin);
        obstacle.SetBuffer(kernel, "maxcs", bbmax);
        obstacle.SetInt("width", (int)size.x);
        obstacle.SetInt("height", (int)size.y);
        obstacle.SetInt("depth", (int)size.z);
        obstacle.SetFloat("m_dt", m_dt);
        obstacle.SetInt("n", numtrees);

        kernel = applynoise.FindKernel("CSMain");
        applynoise.SetTexture(kernel, "Wind", windrt);
        applynoise.SetTexture(kernel, "noise", noise);
        applynoise.SetInt("width", (int)size.x);
        applynoise.SetInt("height", (int)size.y);
        applynoise.SetInt("depth", (int)size.z);
        applynoise.SetFloat("m_dt", m_dt);

    }
    void Start()
    {
        initcompute();
        gen = true;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        foreach (var mat in mats)
        {
            mat.SetTexture("_MyTex", windrt);
            mat.SetVector("_size", new Vector4(size.x, size.y, size.z, 0.0f));
            mat.SetFloat("_stiffness", stiffness);
            mat.SetFloat("_stretch", stretch);
        }
        if (!enablestep|| step)
        {
            int kernel = windgen.FindKernel("CSMain");
            windgen.SetFloat("time", Time.time);

            if (genonce )
            {
                if(gen)
                {

                    windgen.Dispatch(kernel, (int)size.x / numthreadsx, (int)size.y / numthreadsy, (int)size.z / numthreadsz);
                    gen = false;
                }
            }
            else
            {

                windgen.Dispatch(kernel, (int)size.x / numthreadsx, (int)size.y / numthreadsy, (int)size.z / numthreadsz);

            }
            
            
            


            


            kernel = diffusion.FindKernel("CSMain");
            for (int i = 0; i < 3; ++i)
            {
                diffusion.Dispatch(kernel, (int)size.x / numthreadsx, (int)size.y / numthreadsy, (int)size.z / numthreadsz);
            }

            kernel = advectforward.FindKernel("CSMain");
            advectforward.Dispatch(kernel, (int)size.x / numthreadsx, (int)size.y / numthreadsy, (int)size.z / numthreadsz);

            kernel = texitotex.FindKernel("CSMain");
            texitotex.Dispatch(kernel, (int)size.x / numthreadsx, (int)size.y / numthreadsy, (int)size.z / numthreadsz);



            kernel = advectbackward.FindKernel("CSMain");
            advectbackward.Dispatch(kernel, (int)size.x / numthreadsx, (int)size.y / numthreadsy, (int)size.z / numthreadsz);

            kernel = obstacle.FindKernel("CSMain");
            obstacle.Dispatch(kernel, (int)size.x / numthreadsx, (int)size.y / numthreadsy, (int)size.z / numthreadsz);

            kernel = applynoise.FindKernel("CSMain");
            applynoise.Dispatch(kernel, (int)size.x / numthreadsx, (int)size.y / numthreadsy, (int)size.z / numthreadsz);

            kernel = textobuffer.FindKernel("CSMain");
            textobuffer.Dispatch(kernel, (int)size.x / numthreadsx, (int)size.y / numthreadsy, (int)size.z / numthreadsz);

            windbuffer.GetData(tmp);


            step = false;
        }
    }
    void Update()
    {

        //if(step)
        //{
            
        //    int kernel = diffusion.FindKernel("CSMain");
        //    diffusion.Dispatch(kernel, 2, 2, 2);

        //    kernel = advectforward.FindKernel("CSMain");
        //    advectforward.Dispatch(kernel, 2, 2, 2);
        //    //windbuffer.GetData(tmp);
        //    kernel = buffertotex.FindKernel("CSMain");
        //    buffertotex.Dispatch(kernel, 2, 2, 2);
            
            
        //    step = false;
        //}

        
    }

    

    private void OnDrawGizmos()
    {
        float floatsize = (float)(1 << 16);
        if(tmp.Length>0)
        {
            var colors = wind.GetPixels();
            for (var i = 0; i < wind.depth; ++i)
            {
                for (var j = 0; j < wind.height; ++j)
                {
                    for (var k = 0; k < wind.width; ++k)
                    {
                        if(j<3)
                        {
                            var originpos = new Vector3(((float)i / wind.depth) * size.z, ((float)j / wind.height) * size.y, ((float)k / wind.width) * size.x);
                            int off = (i * wind.width * wind.height + j * wind.width + k) * 3;
                            Vector3 vel = new Vector3((float)tmp[off] / floatsize, (float)tmp[off + 1] / floatsize, (float)tmp[off + 2] / floatsize);
                            Gizmos.color = new Color(vel.magnitude, vel.magnitude, vel.magnitude);
                            Gizmos.DrawLine(originpos, originpos + vel);
                        }
                        
                        
                    }
                }
            }
        }
        
    }


}
