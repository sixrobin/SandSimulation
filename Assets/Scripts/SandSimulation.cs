namespace SandSimulation
{
    using UnityEngine;

    [DisallowMultipleComponent]
    public class SandSimulation : MonoBehaviour
    {
        private const string KERNEL_NAME_INIT = "Init";
        private const string KERNEL_NAME_NEXT = "Next";
        private const string KERNEL_NAME_APPLY_BUFFER = "ApplyBuffer";
        
        private const string RESOLUTION_ID = "_Resolution";
        private const string RESULT_ID = "_Result";
        private const string GRID_BUFFER_ID = "_GridBuffer";
        
        [SerializeField]
        private ComputeShader _computeShader;
        [SerializeField]
        private int _resolution = 256;
        [SerializeField]
        private float _iterationDelay = 0.1f;
        [SerializeField]
        private Renderer _renderer;
        [SerializeField]
        private int _spawnRadius = 10;

        private int _initKernelIndex;
        private int _nextKernelIndex;
        private int _clearGreenChannelKernelIndex;
        private int _applyBufferKernelIndex;
        
        private float _iterationTimer;
        private int _threadGroups;
        
        protected RenderTexture _result;
        protected RenderTexture _gridBuffer;
        protected Texture2D _sandToSpawn;

        private RenderTexture CreateTexture()
        {
            RenderTexture texture = new(this._resolution, this._resolution, 0, RenderTextureFormat.ARGB32)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };

            texture.Create();
            return texture;
        }
        
        private Texture2D CreateTexture2D()
        {
            Texture2D texture = new Texture2D(this._resolution, this._resolution, TextureFormat.RFloat, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };

            texture.Apply();
            return texture;
        }
        
        private void Init()
        {
            this._computeShader = Instantiate(this._computeShader); // Create a compute shader copy so that every instance can have its own parameters.
            
            this._initKernelIndex = this._computeShader.FindKernel(KERNEL_NAME_INIT);
            this._nextKernelIndex = this._computeShader.FindKernel(KERNEL_NAME_NEXT);
            this._clearGreenChannelKernelIndex = this._computeShader.FindKernel("ClearGreenChannel");
            this._applyBufferKernelIndex = this._computeShader.FindKernel(KERNEL_NAME_APPLY_BUFFER);
            
            this._threadGroups = this._resolution / 8;
            this._computeShader.SetFloat(RESOLUTION_ID, this._resolution);
            
            this._result = this.CreateTexture();
            this._gridBuffer = this.CreateTexture();
            this._sandToSpawn = this.CreateTexture2D();
            this._renderer.material.SetTexture("_MainTex", this._result);
            
            this._computeShader.SetTexture(this._initKernelIndex, RESULT_ID, this._gridBuffer);
            
            this._computeShader.Dispatch(this._initKernelIndex, this._threadGroups, this._threadGroups, 1);

            this._computeShader.SetTexture(this._applyBufferKernelIndex, RESULT_ID, this._result);
            this._computeShader.SetTexture(this._applyBufferKernelIndex, GRID_BUFFER_ID, this._gridBuffer);
            this._computeShader.Dispatch(this._applyBufferKernelIndex, this._threadGroups, this._threadGroups, 1);
        }

        private void Next()
        {
            this._computeShader.SetTexture(this._nextKernelIndex, RESULT_ID, this._result);
            this._computeShader.SetTexture(this._nextKernelIndex, GRID_BUFFER_ID, this._gridBuffer);
            this._computeShader.SetTexture(this._nextKernelIndex, "_SandToSpawn", this._sandToSpawn);
            this._computeShader.Dispatch(this._nextKernelIndex, this._threadGroups, this._threadGroups, 1);

            this.ApplyTextureBuffer();
            
            this._computeShader.SetTexture(this._clearGreenChannelKernelIndex, RESULT_ID, this._result);
            this._computeShader.SetTexture(this._clearGreenChannelKernelIndex, GRID_BUFFER_ID, this._gridBuffer);
            this._computeShader.SetTexture(this._clearGreenChannelKernelIndex, "_SandToSpawn", this._sandToSpawn);
            this._computeShader.Dispatch(this._clearGreenChannelKernelIndex, this._threadGroups, this._threadGroups, 1);
            
            this.ApplyTextureBuffer();

            // TODO: Do not recreate it each time, clear instead.
            this._sandToSpawn = this.CreateTexture2D();
        }
        
        private void ApplyTextureBuffer()
        {
            this._computeShader.SetTexture(this._applyBufferKernelIndex, RESULT_ID, this._gridBuffer);
            this._computeShader.SetTexture(this._applyBufferKernelIndex, GRID_BUFFER_ID, this._result);
            this._computeShader.Dispatch(this._applyBufferKernelIndex, this._threadGroups, this._threadGroups, 1);
        }

        public void SpawnSand(Vector2 uv)
        {
            // TODO: Move this to a compute shader kernel (with the ability to use a brush/radius).
            
            for (int x = -this._spawnRadius / 2; x <= this._spawnRadius / 2; ++x)
            {
                for (int y = -this._spawnRadius / 2; y <= this._spawnRadius / 2; ++y)
                {
                    if (new Vector2Int(x, y).magnitude > (this._spawnRadius - 1f) / 2)
                        continue;
                    
                    int px = Mathf.FloorToInt((uv.x + x * (1f / this._resolution)) * this._resolution);
                    int py = Mathf.FloorToInt((uv.y + y * (1f / this._resolution)) * this._resolution);
                    this._sandToSpawn.SetPixel(px, py, Color.white);
                }   
            }
            
            this._sandToSpawn.Apply();
        }
        
        #region UNITY METHODS
        private void Start()
        {
            this.Init();
        }
        
        private void Update()
        {
            this._iterationTimer += Time.deltaTime;
            if (this._iterationTimer > this._iterationDelay)
            {
                this.Next();
                this._iterationTimer = 0f;
            }
        }
        #endregion // UNITY METHODS
    }
}