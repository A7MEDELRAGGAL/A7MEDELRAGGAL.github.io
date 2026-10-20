using UnityEngine;

namespace Clone.World
{
    public class CloneWaterManager : MonoBehaviour
    {
        public static CloneWaterManager Instance { get; private set; }

        [Header("Surface")]
        public float WaterHeight = 0f;
        public bool WavesEnabled = true;
        public float WaveHeight = 0.25f;
        public float WaveLength = 20f;
        public float WaveSpeed = 1.1f;
        public float CrossWaveHeightFactor = 0.5f;
        public float CrossWaveLength = 7.5f;
        public float CrossWaveSpeed = 1.7f;

        [Header("Depth")]
        public float ShallowDepth = 1.4f;
        public float DeepDepth = 8f;

        [Header("Environment")]
        public LayerMask WaterMask = ~0;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public float GetSurfaceHeight(Vector3 worldPosition)
        {
            if (!WavesEnabled || WaveHeight <= 0f)
                return WaterHeight;

            float time = Time.time;
            float primary = Mathf.Sin(worldPosition.x / Mathf.Max(0.01f, WaveLength) * TwoPi - time * WaveSpeed);
            float cross = Mathf.Sin(worldPosition.z / Mathf.Max(0.01f, CrossWaveLength) * TwoPi + time * CrossWaveSpeed);
            return WaterHeight + primary * WaveHeight + cross * WaveHeight * CrossWaveHeightFactor;
        }

        public float GetSurfaceHeight(Vector3 worldPosition, float time)
        {
            if (!WavesEnabled || WaveHeight <= 0f)
                return WaterHeight;

            float primary = Mathf.Sin(worldPosition.x / Mathf.Max(0.01f, WaveLength) * TwoPi - time * WaveSpeed);
            float cross = Mathf.Sin(worldPosition.z / Mathf.Max(0.01f, CrossWaveLength) * TwoPi + time * CrossWaveSpeed);
            return WaterHeight + primary * WaveHeight + cross * WaveHeight * CrossWaveHeightFactor;
        }

        public float GetDepth(Vector3 worldPosition)
        {
            return Mathf.Max(0f, GetSurfaceHeight(worldPosition) - worldPosition.y);
        }

        public float GetNormalizedDepth(Vector3 worldPosition)
        {
            return Mathf.Clamp01(GetDepth(worldPosition) / Mathf.Max(0.01f, DeepDepth));
        }

        public bool IsUnderWater(Vector3 worldPosition)
        {
            return GetDepth(worldPosition) > 0f;
        }

        public bool IsInShallowWater(Vector3 worldPosition)
        {
            float depth = GetDepth(worldPosition);
            return depth > 0f && depth < ShallowDepth;
        }

        public bool IsDeepEnough(Vector3 worldPosition, float requiredDepth)
        {
            return GetDepth(worldPosition) >= requiredDepth;
        }

        public bool IsInWaterLayer(GameObject target)
        {
            return target != null && WaterMask.value != 0
                && (WaterMask.value & (1 << target.layer)) != 0;
        }

        private const float TwoPi = Mathf.PI * 2f;
    }
}
