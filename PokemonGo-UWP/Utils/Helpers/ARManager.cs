using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Geolocation;
using Windows.Devices.Sensors;
using Windows.Media.Capture;
using Windows.UI.Xaml.Controls;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.IO;
using SharpDX.Mathematics.Interop;
using SharpDX.D3DCompiler;
using System.Collections.Generic;
using SharpDX.WIC;

using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using FeatureLevel = SharpDX.Direct3D.FeatureLevel;
using InputElement = SharpDX.Direct3D11.InputElement;
using Utilities = SharpDX.Utilities;
using PokemonGo_UWP.Entities;
using System.Threading;

namespace PokemonGo_UWP.Utils
{
  [StructLayout(LayoutKind.Sequential)]
  internal struct CommonBuffer
  {
    public Matrix view;
    public Matrix projection;
  };

  [StructLayout(LayoutKind.Sequential)]
  internal struct ModelBuffer
  {
    public Matrix model;
  };

  [StructLayout(LayoutKind.Sequential)]
  internal struct VertexPositionTex
  {
    public VertexPositionTex(Vector3 pos, Vector2 tex)
    {
      this.pos = pos;
      this.tex = tex;
    }

    public Vector3 pos;
    public Vector2 tex;
  };

  internal class mxAsset
  {
    public String name;
    public virtual void Release() { }
  }

  internal class mxModel : mxAsset
  {
    public override void Release()
    {
      SharpDX.Utilities.Dispose(ref vertexBuffer);
      SharpDX.Utilities.Dispose(ref indexBuffer);
    }

    public Buffer vertexBuffer = null;
    public Buffer indexBuffer = null;
    public int indexCount = 0;
    public int stride = 0;

    public virtual void Bind() { }
    public virtual void Draw() { }
  };

  internal class mxModelFull<T> : mxModel where T: struct
  {
    public mxRenderManager renderMan;
    public ushort[] indices;
    public T[] vertices;

    public override void Bind()
    {
      renderMan.dxContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, stride, 0));
      renderMan.dxContext.InputAssembler.SetIndexBuffer(indexBuffer, Format.R16_UInt, 0);
      renderMan.dxContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
    }
    public override void Draw()
    { 
      renderMan.dxContext.DrawIndexed(indexCount, 0, 0);
    }
  }

  internal class mxInstance
  {
    public mxModel model = null;
    public mxDataBuffer data;
    public List<mxTexture> textures = new List<mxTexture>();

    public mxBuffer<T> access<T>() where T: struct { return (mxBuffer<T>)data; }

    public void AttachTexture(mxTexture tex)
    {
      textures.Add(tex);
    }

    public void Draw()
    {
      foreach (var tex in textures) tex.Bind();
      model.Bind();
      data.Bind();
      model.Draw();
    }
  }

  class mxDataBuffer : mxAsset
  {
    public mxRenderManager parent;
    public BufferDescription desc;
    public Buffer buffer;

    public override void Release()
    {
      SharpDX.Utilities.Dispose(ref buffer);
    }

    public virtual void Bind() { }
  }

  class mxBuffer<T> : mxDataBuffer where T: struct
  {
    public T data;

    public override void Bind()
    {
      parent.dxContext.UpdateSubresource(ref data, buffer);
      parent.shaderMan.BindData(this);
    }
  }

  class mxTexture : mxAsset
  {
    public mxRenderManager parent;

    public String slot;
    public Texture2D texture;
    public ShaderResourceView textureView;
    public SamplerState sampler;

    public override void Release()
    {
      SharpDX.Utilities.Dispose(ref texture);
      SharpDX.Utilities.Dispose(ref textureView);
      SharpDX.Utilities.Dispose(ref sampler);
    }

    public void Bind()
    {
      parent.shaderMan.BindTexture(this);
    }
  }

  internal class mxRenderManager
  {
    Windows.UI.Xaml.Media.Imaging.SurfaceImageSource source = null;
    public Device dxDevice = null;
    public DeviceContext dxContext = null;
    public ISurfaceImageSourceNative dxOutput = null;

    private RenderTargetView dxRenderTarget = null;
    private ViewportF dxViewport;
    private Texture2D dxDepthBuffer = null;
    private DepthStencilView dxDepthView = null;

    private Rectangle drawArea;
    public mxShaderManager shaderMan = new mxShaderManager();

    public Color clearColor = Color.Black;
    public bool useDepthBuffer = true;

    private Dictionary<String, mxAsset> assets = new Dictionary<string, mxAsset>();

    ~mxRenderManager()
    {
      Release();
    }

    public void Init(int width, int height, Windows.UI.Xaml.Media.Imaging.SurfaceImageSource s)
    {
      Reset();
      source = s;
      drawArea = new Rectangle(0, 0, width, height);

      var creationFlags = DeviceCreationFlags.BgraSupport; //required for compatibility with Direct2D.
      FeatureLevel[] featureLevels = { FeatureLevel.Level_11_1, FeatureLevel.Level_11_0, FeatureLevel.Level_10_1, FeatureLevel.Level_10_0, FeatureLevel.Level_9_3, FeatureLevel.Level_9_2, FeatureLevel.Level_9_1 };

      dxDevice = new Device(DriverType.Hardware, creationFlags, featureLevels);
      dxContext = dxDevice.ImmediateContext;

      dxOutput = ComObject.QueryInterface<ISurfaceImageSourceNative>(source);
      dxOutput.Device = dxDevice.QueryInterface<SharpDX.DXGI.Device>();

      dxViewport = new ViewportF(0, 0, drawArea.Width, drawArea.Height);

      shaderMan.renderMan = this;
    }

    public void StartFrame()
    {
      SharpDX.Utilities.Dispose(ref dxRenderTarget);

      try
      {
        RawPoint offset;
        using (var surface = dxOutput.BeginDraw(drawArea, out offset))
        {
          using (var d3DTexture = surface.QueryInterface<Texture2D>())
          {
            dxRenderTarget = new RenderTargetView(dxDevice, d3DTexture);
          }

          if (dxViewport.X != offset.X) dxViewport.X = offset.X;
          if (dxViewport.Y != offset.Y) dxViewport.Y = offset.Y;
          dxContext.Rasterizer.SetViewport(dxViewport);

          // Create depth/stencil buffer descriptor.
          if (dxDepthView == null && useDepthBuffer == true)
          {
            Texture2DDescription depthStencilDesc = new Texture2DDescription()
            {
              Format = Format.D24_UNorm_S8_UInt,
              Width = surface.Description.Width,
              Height = surface.Description.Height,
              ArraySize = 1,
              MipLevels = 1,
              BindFlags = BindFlags.DepthStencil,
              SampleDescription = new SampleDescription(1, 0),
              Usage = ResourceUsage.Default,
            };
            dxDepthBuffer = new Texture2D(dxDevice, depthStencilDesc);
            dxDepthView = new DepthStencilView(dxDevice, dxDepthBuffer);
          }
        }
      }
      catch (SharpDXException ex)
      {
        if (ex.ResultCode == SharpDX.DXGI.ResultCode.DeviceRemoved ||
            ex.ResultCode == SharpDX.DXGI.ResultCode.DeviceReset)
        {
          // If the device has been removed or reset, attempt to recreate it and continue drawing.
          Init(drawArea.Width, drawArea.Height, source);
          StartFrame();
        }
        else
        {
          Reset();
        }
      }

      dxContext.ClearRenderTargetView(dxRenderTarget, clearColor);

      if (useDepthBuffer)
      {
        dxContext.ClearDepthStencilView(dxDepthView, DepthStencilClearFlags.Depth, 1.0f, 0);
        dxContext.OutputMerger.SetRenderTargets(dxDepthView, dxRenderTarget);
      }
      else
      {
        dxContext.OutputMerger.SetRenderTargets(dxRenderTarget);
      }
    }

    public void EndFrame()
    {
      try
      {
        dxOutput.EndDraw();
      }
      catch (SharpDXException ex)
      {
        Release();
      }
    }

    public void Release()
    {
      Reset();
      shaderMan.Release();
      foreach (var item in assets.Values) item.Release();
      // reset things back to initial state
      shaderMan = new mxShaderManager(); 
      assets = new Dictionary<string, mxAsset>();
    }

    public void Reset()
    { 
      SharpDX.Utilities.Dispose(ref dxDevice);
      SharpDX.Utilities.Dispose(ref dxOutput);
      dxContext = null;
      SharpDX.Utilities.Dispose(ref dxRenderTarget);
      SharpDX.Utilities.Dispose(ref dxDepthBuffer);
      SharpDX.Utilities.Dispose(ref dxDepthView);
    }

    public mxBuffer<T> CreateBuffer<T>(String n) where T: struct
    {
      mxBuffer<T> obj = new mxBuffer<T>() { parent = this, name = n };
      obj.desc = new BufferDescription() { SizeInBytes = SharpDX.Utilities.SizeOf<T>(), BindFlags = BindFlags.ConstantBuffer };
      obj.buffer = new Buffer(dxDevice, obj.desc);

      assets[n] = obj;
      return obj;
    }

    public mxModel CreateModel<T>(String n, T[] verts, ushort[] indices) where T:struct
    {
      mxModelFull<T> model = new mxModelFull<T>() { name = n, vertices = verts, indices = indices, renderMan = this };

      var vertexBufferDesc = new BufferDescription() { SizeInBytes = SharpDX.Utilities.SizeOf<T>() * verts.Length, BindFlags = BindFlags.VertexBuffer };
      model.vertexBuffer = Buffer.Create(dxDevice, verts, vertexBufferDesc);

      var indexBufferDesc = new BufferDescription() { SizeInBytes = sizeof(ushort) * indices.Length, BindFlags = BindFlags.IndexBuffer };
      model.indexBuffer = Buffer.Create(dxDevice, indices, indexBufferDesc);

      model.indexCount = indices.Length;
      model.stride = SharpDX.Utilities.SizeOf<T>();

      assets[n] = model;
      return model;
    }

    public mxTexture CreateTexture(String n, String s, String filename)
    {
      mxTexture tex = new mxTexture() { parent = this, slot = s, name = n };

      NativeFileStream fileStream = new NativeFileStream(Windows.ApplicationModel.Package.Current.InstalledLocation.Path + filename, NativeFileMode.Open, NativeFileAccess.Read);
      ImagingFactory imagingFactory = new ImagingFactory();
      BitmapDecoder bitmapDecoder = new BitmapDecoder(imagingFactory, fileStream, DecodeOptions.CacheOnDemand);
      BitmapFrameDecode frame = bitmapDecoder.GetFrame(0);
      FormatConverter converter = new FormatConverter(imagingFactory);
      converter.Initialize(frame, PixelFormat.Format32bppPRGBA);

      int stride = converter.Size.Width * 4;
      using (var buffer = new DataStream(converter.Size.Height * stride, true, true))
      {
        converter.CopyPixels(stride, buffer);
        Texture2DDescription texdesc = new Texture2DDescription()
        {
          Width = converter.Size.Width,
          Height = converter.Size.Height,
          ArraySize = 1,
          BindFlags = BindFlags.ShaderResource,
          Usage = ResourceUsage.Immutable,
          CpuAccessFlags = CpuAccessFlags.None,
          Format = Format.R8G8B8A8_UNorm,
          MipLevels = 1,
          OptionFlags = ResourceOptionFlags.None,
          SampleDescription = new SampleDescription(1, 0)
        };
        tex.texture = new Texture2D(dxDevice, texdesc, new DataRectangle(buffer.DataPointer, stride));
        tex.textureView = new ShaderResourceView(dxDevice, tex.texture);
        SamplerStateDescription sampledesc = new SamplerStateDescription()
        {
          Filter = Filter.MinMagMipLinear,
          AddressU = TextureAddressMode.Wrap,
          AddressV = TextureAddressMode.Wrap,
          AddressW = TextureAddressMode.Wrap,
          BorderColor = Color.Black,
          ComparisonFunction = Comparison.Never,
          MaximumAnisotropy = 16,
          MipLodBias = 0,
          MinimumLod = -float.MaxValue,
          MaximumLod = float.MaxValue
        };
        tex.sampler = new SamplerState(dxDevice, sampledesc);
      }

      assets[n] = tex;
      return tex;
    }

    public bool GetAsset<T>(String name, ref T obj)
    {
      if (assets.ContainsKey(name) == false) return false;
      obj = (T)(object)assets[name];
      return true;
    }
  }
  
  internal enum mxShaderType { None, Pixel, Vertex }

  internal class mxShader
  {
    public virtual void Release()
    {
      SharpDX.Utilities.Dispose(ref bytecode);
    }

    public Dictionary<String, int> dataBuffers = new Dictionary<string, int>();
    public Dictionary<String, int> texSamplers = new Dictionary<string, int>();

    public mxShaderType type = mxShaderType.None;
    public String name;
    public String filename;
    public ShaderBytecode bytecode = null;

    public void DefineData(String name, int i)
    {
      dataBuffers[name] = i;
    }

    public void DefineTexture(String slot, int i)
    {
      texSamplers[slot] = i;
    }
  }

  internal class mxVertexShader : mxShader
  {
    public override void Release()
    {
      base.Release();
      SharpDX.Utilities.Dispose(ref shader);
      SharpDX.Utilities.Dispose(ref layout);
    }

    public VertexShader shader = null;
    public InputLayout layout = null;
  }

  internal class mxPixelShader : mxShader
  {
    public override void Release()
    {
      base.Release();
      SharpDX.Utilities.Dispose(ref shader);
    }

    public PixelShader shader = null;
  }

  internal class mxEffect
  {
    public mxShaderManager manager = null;
    public String name;
    public mxShader vertex = null;
    public mxShader pixel = null;

    public void Bind()
    {
      if (manager == null) return;
      manager.BindEffect(this);
    }
  }

  internal class mxShaderManager
  {
    public mxRenderManager renderMan = null;
    public mxShader curVertex = null;
    public mxShader curPixel = null;

    public Dictionary<String, mxShader> shaders = new Dictionary<string, mxShader>();
    public Dictionary<String, mxEffect> effects = new Dictionary<string, mxEffect>();

    public bool Register(String filename, String name, InputElement[] layout)
    {
      if (renderMan == null) return false;
      if (filename.Contains(".vs.hlsl"))
      {
        mxVertexShader shader = new mxVertexShader() { name = name, filename = filename, type = mxShaderType.Vertex };
        shader.bytecode = new ShaderBytecode(ShaderBytecode.CompileFromFile(filename, "main", "vs_5_0"));
        shader.shader = new VertexShader(renderMan.dxDevice, shader.bytecode);
        shader.layout = new InputLayout(renderMan.dxDevice, shader.bytecode, layout);
        shaders[name] = shader;
        return true;
      }
      return false;
    }

    public bool Register(String filename, String name)
    {
      if (renderMan == null) return false;

      if (filename.Contains("ps.hlsl"))
      {
        mxPixelShader shader = new mxPixelShader() { name = name, filename = filename, type = mxShaderType.Pixel };
        shader.bytecode = new ShaderBytecode(ShaderBytecode.CompileFromFile(filename, "main", "ps_5_0"));
        shader.shader = new PixelShader(renderMan.dxDevice, shader.bytecode);
        shaders[name] = shader;
        return true;
      }
      return false;
    }

    public bool Register(String name, String vname, String pname)
    {
      mxShader v = GetShader(vname);
      mxShader p = GetShader(pname);
      mxEffect e;
      if (v == null || v.type != mxShaderType.Vertex) return false;
      if (p == null || p.type != mxShaderType.Pixel) return false;
      e = new mxEffect() { name = name, vertex = v, pixel = p, manager = this };
      effects[name] = e;
      return true;
    }

    public mxShader GetShader(String name)
    {
      if (shaders.ContainsKey(name)) return shaders[name];
      return null;
    }

    public mxEffect GetEffect(String name)
    {
      if (effects.ContainsKey(name)) return effects[name];
      return null;
    }

    public void BindEffect(mxEffect e)
    {
      if (renderMan == null) return;
      if (e.vertex != null && curVertex != e.vertex)
      {
        renderMan.dxContext.InputAssembler.InputLayout = ((mxVertexShader)e.vertex).layout;
        renderMan.dxContext.VertexShader.Set(((mxVertexShader)e.vertex).shader);
        curVertex = e.vertex;
      }
      if (e.pixel != null && curPixel != e.pixel)
      {
        renderMan.dxContext.PixelShader.Set(((mxPixelShader)e.pixel).shader);
        curPixel = e.pixel;
      }
    }

    public void BindData<T>(mxBuffer<T> data) where T : struct
    {
      if (curVertex.dataBuffers.ContainsKey(data.name) != false)
        renderMan.dxContext.VertexShader.SetConstantBuffer(curVertex.dataBuffers[data.name], data.buffer);
      if (curPixel.dataBuffers.ContainsKey(data.name) != false)
        renderMan.dxContext.PixelShader.SetConstantBuffer(curPixel.dataBuffers[data.name], data.buffer);
    }

    public void BindTexture(mxTexture tex)
    {
      if (curVertex.texSamplers.ContainsKey(tex.slot) != false)
      {
        int i = curVertex.texSamplers[tex.slot];
        renderMan.dxContext.VertexShader.SetShaderResource(i, tex.textureView);
        renderMan.dxContext.VertexShader.SetSampler(i, tex.sampler);
      }
      if (curPixel.texSamplers.ContainsKey(tex.slot) != false)
      {
        int i = curPixel.texSamplers[tex.slot];
        renderMan.dxContext.PixelShader.SetShaderResource(i, tex.textureView);
        renderMan.dxContext.PixelShader.SetSampler(i, tex.sampler);
      }
    } 

    public void Release()
    {
      renderMan = null;
      curPixel = null;
      curVertex = null;
      foreach (var item in shaders.Values) item.Release();
      shaders = null;
      effects = null;
    }
  }

  class arPokemon
  {
    public mxInstance instance;
    public Geopoint geo;
    public Vector3 position;
    public float scale;
    public ulong id;
  }

  class arCamera
  {
    public Vector3 eye = new Vector3(0.0f, 0.0f, 0.0f); // Define camera position
    public Vector3 forward = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 up = new Vector3(0.0f, 0.0f, 0.0f); // Define up direction.
    public Vector3 target = new Vector3(0.0f, 0.0f, 0.0f); // Define focus position.
    public Matrix lookat;
  }

  class ARManager : Windows.UI.Xaml.Media.Imaging.SurfaceImageSource
  {
    private MediaCapture mMediaCapture = null;
    private bool mIsInitialized = false;
    private bool mIsActive = false;
    private CaptureElement mElement = null;
    private readonly SimpleOrientationSensor mOrientationSensor = SimpleOrientationSensor.GetDefault();

    // Direct3D objects
    private mxRenderManager renderMan = new mxRenderManager();

    private mxBuffer<CommonBuffer> dataCommon;
    private mxModel plane = null;
    private mxModel billboard = null;
    private mxInstance floor = null;
    private Dictionary<ulong, arPokemon> pokemons = new Dictionary<ulong, arPokemon>();
    private Dictionary<string, arPokemon> pokestops = new Dictionary<string, arPokemon>();
    private List<arPokemon> testGuys = new List<arPokemon>();
    private arCamera camera = new arCamera();

    private int width = 0;
    private int height = 0;
//    private Geopoint playerPos;

//    Geolocator geo = new Geolocator();
    public CompassHelper compass = new CompassHelper();

    public ARManager(int pixelWidth, int pixelHeight, bool isOpaque)
        : base(pixelWidth, pixelHeight, isOpaque)
    {
      width = pixelWidth;
      height = pixelHeight;
    }

    public async Task Initialize(CaptureElement element)
    {
      await initVideo(element);
//      initGeo();
      initDX();
    }

//    #region Geo Position
//    Timer geoTimer;
//    private void initGeo()
//    {
//      geoTimer = new Timer(onGeoTimer, null, 0, Timeout.Infinite);
//    }
//
//    private void BeginGeo()
//    {
//      if (mIsActive) geoTimer.Change(1000, Timeout.Infinite);
//    }
//
//    private async void onGeoTimer(Object state)
//    {
//      try
//      {
//        Geoposition pos = await geo.GetGeopositionAsync();
//        playerPos = pos.Coordinate.Point;
//        PokemonGo.RocketAPI.Logger.Write($"player at {playerPos.Position.Latitude} {playerPos.Position.Longitude}");
//      }
//      catch { }
//      BeginGeo();
//    }
//    #endregion

    #region Camera Stream
    private async Task initVideo(CaptureElement element)
    {
      // already inited?
      if (mMediaCapture != null) return;
      // get hardware camera on the back side
      var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
      DeviceInformation cameraDevice = allVideoDevices.FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Back);
      if (cameraDevice == null) return;
      // get stream renderer
      mMediaCapture = new MediaCapture();
      if (mMediaCapture == null) return;
      try { await mMediaCapture.InitializeAsync(new MediaCaptureInitializationSettings { VideoDeviceId = cameraDevice.Id }); }
      catch (UnauthorizedAccessException)
      {
        await Deinitialize();
        return;
      }
      // assume portrait
      mMediaCapture.SetPreviewRotation(VideoRotation.Clockwise90Degrees);
      // init state
      mElement = element;
      mIsInitialized = true;
    }

    public async Task Deinitialize()
    {
      // deinit video
      await StopVideoStream();
      if (mMediaCapture != null)
      {
        mMediaCapture.Dispose();
        mMediaCapture = null;
      }
      dxClean();
      mIsInitialized = false;
    }

    public async Task BeginVideoStream()
    {
      if (mIsInitialized == false) return;
      mElement.Source = mMediaCapture;
      await mMediaCapture.StartPreviewAsync();
      mIsActive = true;
      compass.Reset = true;
      //BeginGeo();
    }

    public async Task StopVideoStream()
    {
      if (mIsInitialized == false) return;
      if (mIsActive) await mMediaCapture.StopPreviewAsync();
      mIsActive = false;
    }
    #endregion

    #region DirectX
    public void initDX()
    {
      dxCreateDevice();
      dxCreateShaders();
      dxCreateMesh();
      dxInitScene();
    }

    public void dxClean()
    {
      renderMan.Release();
      plane = null;
      floor = null;
      // reset to initial state
      pokemons.Clear();
      camera = new arCamera();
    }

    private void dxCreateDevice()
    {
      renderMan.Init(width, height, this);
      renderMan.clearColor = new Color(0, 0, 0, 0);
    }

    private void dxCreateShaders()
    {
      var vertexDesc = new[]
      {
        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0, InputClassification.PerVertexData, 0),
      };

      renderMan.shaderMan.Register("Assets\\Shaders\\VertexShader.vs.hlsl", "vs", vertexDesc);
      mxShader shader = renderMan.shaderMan.GetShader("vs");
      shader.DefineData("common", 0);
      shader.DefineData("model", 1);

      renderMan.shaderMan.Register("Assets\\Shaders\\PixelShader.ps.hlsl", "ps");
      shader = renderMan.shaderMan.GetShader("ps");
      shader.DefineTexture("mainTexture", 0);

      renderMan.shaderMan.Register("plainObj", "vs", "ps");
    }

    private void dxCreateMesh()
    {
      var cubeVertices = new[]
      {
        new VertexPositionTex(new Vector3(-5.0f, -0.01f, -5.0f), new Vector2(1.0f, 1.0f)),
        new VertexPositionTex(new Vector3(-5.0f, -0.01f,  5.0f), new Vector2(1.0f, 0.0f)),
        new VertexPositionTex(new Vector3(-5.0f,  0.01f, -5.0f), new Vector2(1.0f, 1.0f)),
        new VertexPositionTex(new Vector3(-5.0f,  0.01f,  5.0f), new Vector2(1.0f, 0.0f)),
        new VertexPositionTex(new Vector3( 5.0f, -0.01f, -5.0f), new Vector2(0.0f, 1.0f)),
        new VertexPositionTex(new Vector3( 5.0f, -0.01f,  5.0f), new Vector2(0.0f, 0.0f)),
        new VertexPositionTex(new Vector3( 5.0f,  0.01f, -5.0f), new Vector2(0.0f, 1.0f)),
        new VertexPositionTex(new Vector3( 5.0f,  0.01f,  5.0f), new Vector2(0.0f, 0.0f)),
      };

      var cubeVertices2 = new[]
      {
        new VertexPositionTex(new Vector3(-0.5f,  1.0f, -0.01f), new Vector2(0.0f, 0.0f)),
        new VertexPositionTex(new Vector3(-0.5f,  1.0f,  0.01f), new Vector2(0.0f, 0.0f)),
        new VertexPositionTex(new Vector3(-0.5f,  0.0f, -0.01f), new Vector2(0.0f, 1.0f)),
        new VertexPositionTex(new Vector3(-0.5f,  0.0f,  0.01f), new Vector2(0.0f, 1.0f)),
        new VertexPositionTex(new Vector3( 0.5f,  1.0f, -0.01f), new Vector2(1.0f, 0.0f)),
        new VertexPositionTex(new Vector3( 0.5f,  1.0f,  0.01f), new Vector2(1.0f, 0.0f)),
        new VertexPositionTex(new Vector3( 0.5f,  0.0f, -0.01f), new Vector2(1.0f, 1.0f)),
        new VertexPositionTex(new Vector3( 0.5f,  0.0f,  0.01f), new Vector2(1.0f, 1.0f)),
      };

      var cubeIndices = new ushort[]
      {
        0, 2, 1, // -x
        1, 2, 3,

        4, 5, 6, // +x
        5, 7, 6,

        0, 1, 5, // -y
        0, 5, 4,

        2, 6, 7, // +y
        2, 7, 3,

        0, 4, 6, // -z
        0, 6, 2,

        1, 3, 7, // +z
        1, 7, 5,
      };

      plane = renderMan.CreateModel<VertexPositionTex>("plane", cubeVertices, cubeIndices);
      billboard = renderMan.CreateModel<VertexPositionTex>("billboard", cubeVertices2, cubeIndices);

      renderMan.CreateTexture("texPokemon", "mainTexture", "\\Assets\\Pokemons\\1.png");
      renderMan.CreateTexture("texPokemon2", "mainTexture", "\\Assets\\Pokemons\\19.png");
      renderMan.CreateTexture("texPokemon3", "mainTexture", "\\Assets\\Pokemons\\10.png");
      renderMan.CreateTexture("pokestop", "mainTexture", "\\Assets\\Pokemons\\pokestop.png");
      renderMan.CreateTexture("texFloor", "mainTexture", "\\Assets\\UI\\compass.png");
    }

    private void dxInitScene()
    {
      // Calculate the aspect ratio and field of view.
      float aspectRatio = (float)width / (float)height;
      float fovAngleY = 60.0f * (float)Math.PI / 180.0f;

      // Create the constant buffer.
      dataCommon = renderMan.CreateBuffer<CommonBuffer>("common");
      dataCommon.data.projection = Matrix.Transpose(Matrix.PerspectiveFovRH(fovAngleY, aspectRatio, 0.01f, 100.0f));

      // create the world objects
      mxTexture tex = null;
      floor = new mxInstance();
      floor.model = plane;
      floor.data = renderMan.CreateBuffer<ModelBuffer>("model");
      floor.access<ModelBuffer>().data.model = Matrix.Transpose(Matrix.Translation(0.0f, 0.0f, 0.0f));
      renderMan.GetAsset<mxTexture>("texFloor", ref tex);
      floor.AttachTexture(tex);

      CreatePokemon("texPokemon",  new Vector3( 0.0f, 0.0f, -7.0f), 1.0f);
      CreatePokemon("texPokemon2", new Vector3( 0.0f, 0.0f,  7.0f), 1.0f);
      CreatePokemon("texPokemon3", new Vector3(-7.0f, 0.0f,  0.0f), 1.0f);
      CreatePokemon("pokestop", new Vector3( 7.0f, 0.0f,  0.0f), 10.0f);
    }

    public void Render()
    {
      if (mIsActive == false) return;
      if (renderMan.dxDevice == null) return;

      Update();
      renderMan.StartFrame();
      RenderFrame();
      renderMan.EndFrame();
    }
    #endregion

    #region Frame Updating

    private ulong fixedid = 1;
    public void CreatePokemon(String pname, Vector3 pos, float scale)
    {
      mxTexture tex = null;
      mxInstance guy;
      arPokemon mon;
      guy = new mxInstance();
      guy.model = billboard;
      guy.data = renderMan.CreateBuffer<ModelBuffer>("model");
      renderMan.GetAsset<mxTexture>(pname, ref tex);
      guy.AttachTexture(tex);
      mon = new arPokemon();
      mon.id = fixedid++;
      mon.instance = guy;
      mon.position = pos;
      mon.scale = scale;
      mon.geo = null;
      mon.instance.access<ModelBuffer>().data.model = Matrix.Transpose(Matrix.Multiply(Matrix.Scaling(mon.scale), Matrix.Translation(mon.position)));
      testGuys.Add(mon);
    }

    public void CreatePokemon(FortDataWrapper p)
    {
      mxTexture tex = null;
      mxInstance guy;
      arPokemon mon;
      guy = new mxInstance();
      guy.model = billboard;
      guy.data = renderMan.CreateBuffer<ModelBuffer>("model");
      renderMan.GetAsset<mxTexture>("pokestop", ref tex);
      guy.AttachTexture(tex);

      mon = new arPokemon();
      mon.geo = p.Geoposition;
      mon.id = fixedid++;
      mon.instance = guy;
      float X = GetDistanceTo(p.Geoposition, GetDistanceType.Long);
      float Z = GetDistanceTo(p.Geoposition, GetDistanceType.Lat);
      mon.position = new Vector3(X, 0, Z);
      mon.scale = 10.0f;
      pokestops[p.Id] = mon;
    }

    public void CreatePokemon(MapPokemonWrapper pokemon)
    {
      mxTexture tex = null;
      arPokemon mon;

      string name = (int)(pokemon.PokemonId) + ".png";
      if (!renderMan.GetAsset<mxTexture>(name, ref tex))
      {
        string fname = "\\Assets\\Pokemons\\" + name;
        renderMan.CreateTexture(name, "mainTexture", fname);
        renderMan.GetAsset<mxTexture>(name, ref tex);
      }

      mxInstance guy;
      guy = new mxInstance();
      guy.model = billboard;
      guy.data = renderMan.CreateBuffer<ModelBuffer>("model");
      guy.AttachTexture(tex);

      float X = GetDistanceTo(pokemon.Geoposition, GetDistanceType.Long);
      float Z = GetDistanceTo(pokemon.Geoposition, GetDistanceType.Lat);
      PokemonGo.RocketAPI.Logger.Write($"creating {pokemon.PokemonId} id {pokemon.EncounterId} at {X}, {Z}");

      mon = new arPokemon();
      mon.geo = pokemon.Geoposition;
      mon.id = pokemon.EncounterId;
      mon.instance = guy;
      mon.position = new Vector3(X, 0, Z);
      mon.scale = 1.0f;
      pokemons[pokemon.EncounterId] = mon;
    }

    private enum GetDistanceType { Long = 1, Lat };

    private float GetDistanceTo(Geopoint point, GetDistanceType type)
    {
      double lat2 = GameClient.Geoposition.Coordinate.Point.Position.Latitude;
      double lon1 = GameClient.Geoposition.Coordinate.Point.Position.Longitude;
      double lat1 = (type == GetDistanceType.Long) ? lat2 : point.Position.Latitude;
      double lon2 = (type == GetDistanceType.Lat) ? lon1 : point.Position.Longitude;
      double R = 6378.137; // Radius of earth in KM
      double dLat = (lat2 - lat1) * Math.PI / 180;
      double dLon = (lon2 - lon1) * Math.PI / 180;
      double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
      double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
      double d = R * c;
      return (float)(d * 1000); // meters
    }

    List<ulong> okids = new List<ulong>();
    List<string> okids2 = new List<String>();
    Vector3 forward = new Vector3(0.0f, 0.0f, 1.0f);

    private void Update()
    {
      // update camera
      Matrix3x3 mat = compass.Matrix;
//      Quaternion q = compass.Quat;
      camera.forward = Vector3.Transform(new Vector3(0.0f, 0.0f, 1.0f), mat);
      camera.eye = new Vector3(0.0f, 2.0f, 0.0f); // Define camera position.
      camera.up = Vector3.Transform(new Vector3(0.0f, 1.0f, 0.0f), mat); // Define up direction.
      camera.target = Vector3.Add(camera.eye, camera.forward); // Define focus position.
      camera.lookat = Matrix.LookAtRH(camera.eye, camera.target, camera.up);
      dataCommon.data.view = Matrix.Transpose(camera.lookat);

      // create new pokemon
      okids.Clear();
      foreach (var p in GameClient.CatchablePokemons)
      {
        if (pokemons.Keys.Contains(p.EncounterId) == false) 
          CreatePokemon(p);
        okids.Add(p.EncounterId);
      }
      foreach (var p in GameClient.NearbyPokestops)
      {
        if (pokestops.Keys.Contains(p.Id) == false)
          CreatePokemon(p);
        okids2.Add(p.Id);
      }
      // remove old pokemon
      foreach (var p in GameClient.CatchablePokemons)
      {
        if (okids.Contains(p.EncounterId)) continue;
        pokemons.Remove(p.EncounterId);
      }
      foreach (var p in GameClient.NearbyPokestops)
      {
        if (okids2.Contains(p.Id)) continue;
        pokestops.Remove(p.Id);
      }

      // update each pokemon
      foreach (var p in pokemons.Values)
        FixPokething(p);
      foreach (var p in pokestops.Values)
        FixPokething(p);
//      foreach (var p in testGuys)
//        FixPokething(p);
    }


    private void FixPokething(arPokemon p)
    {
      // get new offset from geo
      if (p.geo != null)
      {
        p.position.X = GetDistanceTo(p.geo, GetDistanceType.Long);
        p.position.Z = GetDistanceTo(p.geo, GetDistanceType.Lat);
      }
      // get new orientation to face origin
      Vector3 loc = p.position;
      loc.Normalize();
      float angle = (float)Math.Acos(Vector3.Dot(forward, loc));
      Matrix rot = Matrix.RotationY(angle);
      Matrix trans = Matrix.Translation(p.position);
      Matrix scale = Matrix.Scaling(p.scale);
      p.instance.access<ModelBuffer>().data.model = Matrix.Transpose(Matrix.Multiply(Matrix.Multiply(scale, rot), trans));
    }
    #endregion

    private void RenderFrame()
    {
      renderMan.shaderMan.GetEffect("plainObj").Bind(); // set the shader
      dataCommon.Bind(); // set the common data
      floor.Draw();      // draw the floor
      foreach (var p in pokemons.Values)
        p.instance.Draw();
      foreach (var p in pokestops.Values)
        p.instance.Draw();
//      foreach (var p in testGuys)
//        p.instance.Draw();
    }
  }
}
