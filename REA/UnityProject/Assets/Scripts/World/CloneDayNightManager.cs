using UnityEngine;

namespace Clone.World
{
    public class CloneDayNightManager : MonoBehaviour
    {
        public static CloneDayNightManager Instance { get; private set; }

        [SerializeField, Min(1f)]
        private float dayLengthSeconds = 600f;

        [SerializeField]
        private Light sun;

        private int dayNumber = 1;
        private float timeOfDay;

        public int DayNumber => dayNumber;

        public float TimeOfDay => timeOfDay;

        public bool IsNight => timeOfDay >= 0.5f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            timeOfDay += Time.deltaTime / dayLengthSeconds;
            if (timeOfDay >= 1f)
            {
                timeOfDay -= 1f;
                dayNumber++;
            }

            if (sun != null)
                sun.transform.rotation = Quaternion.Euler(360f * timeOfDay - 90f, 170f, 0f);
        }

        public void SetTimeOfDay(int day, float time01)
        {
            dayNumber = Mathf.Max(1, day);
            timeOfDay = Mathf.Clamp01(time01);
        }

        public void ResetToMorning()
        {
            SetTimeOfDay(1, 0.15f);
        }
    }
}
