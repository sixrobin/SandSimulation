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
        private const string ITERATIONS_ID = "_Iterations";
        private const string RESULT_ID = "_Result";
        private const string GRID_BUFFER_ID = "_GridBuffer";
        
        [SerializeField]
        private ComputeShader _computeShader;
        [SerializeField, Min(8)]
        private int _resolution = 256;
        [SerializeField, Min(0f)]
        private float _iterationDelay = 0.1f;
        [SerializeField, Min(1)]
        private int _iterationsPerTick = 1;
        [SerializeField]
        private Renderer _renderer;
        [SerializeField, Min(1f)]
        private int _spawnRadius = 10;

        private int _initKernelIndex;
        private int _nextKernelIndex;
        private int _clearGreenChannelKernelIndex;
        private int _applyBufferKernelIndex;
        
        private float _iterationTimer;
        private int _threadGroups;
        private int _iterations;
        
        protected RenderTexture _result;
        protected RenderTexture _gridBuffer;

        private SpawnType _nextSpawnType;
        private Vector2 _nextSpawnUV;

        public void ResetSimulation() => this.Init();
        // TODO: ResetSand method.

        private RenderTexture CreateTexture()
        {
            RenderTexture texture = new(this._resolution, this._resolution, 0, RenderTextureFormat.ARGBFloat)
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
            Texture2D texture = new(this._resolution, this._resolution, TextureFormat.RGBAFloat, false)
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
            this._renderer.material.SetTexture("_MainTex", this._result);
            
            this._computeShader.SetTexture(this._initKernelIndex, RESULT_ID, this._gridBuffer);
            
            this._computeShader.Dispatch(this._initKernelIndex, this._threadGroups, this._threadGroups, 1);

            this._computeShader.SetTexture(this._applyBufferKernelIndex, RESULT_ID, this._result);
            this._computeShader.SetTexture(this._applyBufferKernelIndex, GRID_BUFFER_ID, this._gridBuffer);
            this._computeShader.Dispatch(this._applyBufferKernelIndex, this._threadGroups, this._threadGroups, 1);
        }

        private void Next()
        {
            this._computeShader.SetInt("_Iterations", this._iterations);
            this._computeShader.SetInt("_SpawnRadius", this._spawnRadius);
            
            if (this._nextSpawnType != null)
                this._computeShader.SetVector("_SpawnData", new Vector4(this._nextSpawnUV.x, this._nextSpawnUV.y, this._nextSpawnType.Weight, this._nextSpawnType.ID));
            
            this._computeShader.SetTexture(this._nextKernelIndex, RESULT_ID, this._result);
            this._computeShader.SetTexture(this._nextKernelIndex, GRID_BUFFER_ID, this._gridBuffer);
            this._computeShader.Dispatch(this._nextKernelIndex, this._threadGroups, this._threadGroups, 1);

            this.ApplyTextureBuffer();
            
            this._computeShader.SetTexture(this._clearGreenChannelKernelIndex, RESULT_ID, this._result);
            this._computeShader.SetTexture(this._clearGreenChannelKernelIndex, GRID_BUFFER_ID, this._gridBuffer);
            this._computeShader.Dispatch(this._clearGreenChannelKernelIndex, this._threadGroups, this._threadGroups, 1);
            
            this.ApplyTextureBuffer();
            
            this._computeShader.SetVector("_SpawnData", new Vector4(-1f, -1f, -1f, -1f));
            this._nextSpawnType = null;
        }
        
        private void ApplyTextureBuffer()
        {
            this._computeShader.SetTexture(this._applyBufferKernelIndex, RESULT_ID, this._gridBuffer);
            this._computeShader.SetTexture(this._applyBufferKernelIndex, GRID_BUFFER_ID, this._result);
            this._computeShader.Dispatch(this._applyBufferKernelIndex, this._threadGroups, this._threadGroups, 1);
        }

        public void SpawnSand(SpawnType type, Vector2 uv)
        {
            this._nextSpawnType = type;
            this._nextSpawnUV = uv;
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
                for (int i = 0; i < this._iterationsPerTick; ++i)
                {
                    this.Next();
                    this._iterations++;
                }
    
                this._iterationTimer = 0f;
            }
        }
        #endregion // UNITY METHODS
    }
}