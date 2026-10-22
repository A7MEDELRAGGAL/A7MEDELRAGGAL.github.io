using FishNet;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Clone.World
{
    public class CloneDayNightManager : NetworkBehaviour
    {
        public static CloneDayNightManager Instance { get; private set; }

        [SerializeField, Min(1f)]
        private float dayLengthSeconds = 600f;

        [SerializeField]
        public Light sun;

        [SyncVar]
        private int _day = 1;

        [SyncVar]
        private byte _phase = StartPhase;

        private const int PhasesPerDay = 100;
        private const byte StartPhase = 25;
        private const float DawnStart = 0.16f;
        private const float DawnEnd = 0.30f;
        private const float DuskStart = 0.68f;
        private const float DuskEnd = 0.82f;
        private const float GlowCenterMorning = 0.27f;
        private const float GlowCenterEvening = 0.73f;
        private const float GlowHalfWidth = 0.06f;
        private const float SunDayIntensity = 1.05f;
        private const float SunNightIntensity = 0.02f;
        private const float SunYaw = 170f;
        private const float UiReferenceWidth = 1920f;
        private const float UiReferenceHeight = 1080f;
        private const float DayLabelTopOffset = 40f;
        private const float DayLabelFontSize = 40f;
        private const float DayLabelWidth = 400f;
        private const float DayLabelHeight = 60f;
        private const byte InitialRenderedPhase = 255;

        private static readonly Color DayTop = new Color(0.35f, 0.55f, 0.85f);
        private static readonly Color NightTop = new Color(0.01f, 0.02f, 0.05f);
        private static readonly Color DayBottom = new Color(0.75f, 0.82f, 0.90f);
        private static readonly Color NightBottom = new Color(0.05f, 0.07f, 0.14f);
        private static readonly Color DaySun = new Color(1f, 0.95f, 0.85f);
        private static readonly Color NightSun = new Color(0.15f, 0.20f, 0.35f);
        private static readonly Color SunsetGlow = new Color(1f, 0.55f, 0.25f);
        private static readonly string[] SkyboxShaderNames =
        {
            "Render Pipeline/Skybox/Procedural",
            "Skybox/Procedural"
        };
        private static readonly int TopColorProperty = Shader.PropertyToID("_TopColor");
        private static readonly int MiddleColorProperty = Shader.PropertyToID("_MiddleColor");
        private static readonly int BottomColorProperty = Shader.PropertyToID("_BottomColor");

        private float _progress;
        private float _segStart;
        private byte _lastPolledPhase = StartPhase;
        private byte _lastRenderedPhase = InitialRenderedPhase;
        private int _lastRenderedDay;
        private Text _dayText;
        private Material _skyboxMaterial;

        public int DayNumber => _day;

        public float TimeOfDay
        {
            get
            {
                float segmentLength = SegmentLength;
                float fraction = Mathf.Clamp01((Time.time - _segStart) / segmentLength);
                float value = (_phase + fraction * PhasesPerDay) % PhasesPerDay;
                return value / PhasesPerDay;
            }
        }

        public bool IsNight => NightFactor(TimeOfDay) > 0.5f;

        private float SegmentLength => dayLengthSeconds / (float)PhasesPerDay;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            BuildUi();
            _lastRenderedDay = 0;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            _day = 1;
            _phase = StartPhase;
            _progress = 0f;
            _segStart = Time.time;
        }

        private void Update()
        {
            if (_phase != _lastPolledPhase)
            {
                _segStart = Time.time;
                _lastPolledPhase = _phase;
            }

            if (InstanceFinder.IsServerInitialized)
            {
                _progress += Time.deltaTime;
                if (_progress >= dayLengthSeconds)
                {
                    _progress -= dayLengthSeconds;
                    _day = _day + 1;
                }

                byte target = (byte)((StartPhase + Mathf.FloorToInt(_progress / SegmentLength)) % PhasesPerDay);
                if (target != _phase)
                {
                    _phase = target;
                    _segStart = Time.time;
                }
            }

            float t = TimeOfDay;
            if (_phase != _lastRenderedPhase)
            {
                _lastRenderedPhase = _phase;
                ApplyVisuals(t);
            }

            if (_day != _lastRenderedDay)
            {
                _lastRenderedDay = _day;
                UpdateUi();
            }
        }

        public void SetTimeOfDay(int day, float time01)
        {
            _day = Mathf.Max(1, day);
            time01 = Mathf.Clamp01(time01);
            if (InstanceFinder.IsServerInitialized)
            {
                _phase = (byte)((int)(time01 * PhasesPerDay) % PhasesPerDay);
                _progress = (time01 * dayLengthSeconds) % dayLengthSeconds;
            }

            _segStart = Time.time;
            _lastRenderedPhase = InitialRenderedPhase;
            _lastRenderedDay = 0;
            UpdateUi();
        }

        public void ResetToMorning()
        {
            SetTimeOfDay(1, 0.15f);
        }

        private static float NightFactor(float t)
        {
            return Mathf.Max(1f - Mathf.InverseLerp(DawnStart, DawnEnd, t), Mathf.InverseLerp(DuskStart, DuskEnd, t));
        }

        private static float Bell(float t, float center)
        {
            float distance = Mathf.Abs(t - center);
            return distance >= GlowHalfWidth ? 0f : 1f - distance / GlowHalfWidth;
        }

        private void ApplyVisuals(float t)
        {
            float night = NightFactor(t);
            float warmth = Mathf.Max(Bell(t, GlowCenterMorning), Bell(t, GlowCenterEvening));
            Color top = Color.Lerp(DayTop, NightTop, night);
            Color bottom = Color.Lerp(DayBottom, NightBottom, night);
            Color warmBottom = Color.Lerp(bottom, SunsetGlow, warmth);

            if (sun != null)
            {
                sun.transform.rotation = Quaternion.Euler(360f * t - 90f, SunYaw, 0f);
                sun.color = Color.Lerp(Color.Lerp(DaySun, NightSun, night), SunsetGlow, warmth);
                sun.intensity = Mathf.Lerp(SunDayIntensity, SunNightIntensity, night);
            }

            EnsureSkyboxMaterial();
            if (_skyboxMaterial != null)
            {
                _skyboxMaterial.SetColor(TopColorProperty, top);
                _skyboxMaterial.SetColor(MiddleColorProperty, Color.Lerp(top, bottom, 0.5f));
                _skyboxMaterial.SetColor(BottomColorProperty, warmBottom);
                RenderSettings.fog = true;
                RenderSettings.fogColor = warmBottom;
            }
            else
            {
                RenderSettings.ambientSkyColor = top;
                RenderSettings.ambientGroundColor = bottom;
                RenderSettings.fog = true;
                RenderSettings.fogColor = warmBottom;
            }
        }

        private void EnsureSkyboxMaterial()
        {
            if (_skyboxMaterial != null)
            {
                return;
            }

            Shader shader = null;
            foreach (string shaderName in SkyboxShaderNames)
            {
                shader = Shader.Find(shaderName);
                if (shader != null)
                {
                    break;
                }
            }

            if (shader == null)
            {
                return;
            }

            _skyboxMaterial = new Material(shader);
            RenderSettings.skybox = _skyboxMaterial;
        }

        private void BuildUi()
        {
            GameObject canvasGo = new GameObject("DayNightCanvas");
            canvasGo.transform.SetParent(transform, false);
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(UiReferenceWidth, UiReferenceHeight);

            _dayText = canvasGo.AddComponent<Text>();
            _dayText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _dayText.fontSize = (int)DayLabelFontSize;
            _dayText.color = Color.white;
            _dayText.alignment = TextAnchor.UpperCenter;
            _dayText.raycastTarget = false;
            _dayText.horizontalOverflow = HorizontalWrapMode.Overflow;
            _dayText.verticalOverflow = VerticalWrapMode.Overflow;

            RectTransform rect = _dayText.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -DayLabelTopOffset);
            rect.sizeDelta = new Vector2(DayLabelWidth, DayLabelHeight);
            _dayText.text = $"Day {_day}";
        }

        private void UpdateUi()
        {
            if (_dayText == null)
            {
                return;
            }

            _dayText.text = $"Day {_day}";
        }
    }
}
