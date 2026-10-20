using UnityEngine;

namespace Clone.SaveLoad
{
    public class CloneAutoSaver : MonoBehaviour
    {
        [SerializeField, Min(10f)]
        private float intervalSeconds = 120f;

        [SerializeField]
        private string slotName = "clone-auto";

        private float elapsed;

        private void Update()
        {
            elapsed += Time.deltaTime;
            if (elapsed < intervalSeconds)
                return;

            elapsed = 0f;
            CloneSaveManager.SaveSlot(slotName);
        }
    }
}
