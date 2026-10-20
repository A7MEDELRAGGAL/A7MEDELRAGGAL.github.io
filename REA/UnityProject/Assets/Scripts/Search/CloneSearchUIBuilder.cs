using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Clone.Items;
using Clone.Player;

namespace Clone.Search
{
    public class CloneSearchUIBuilder : MonoBehaviour
    {
        [SerializeField]
        private CloneItemSearch searchService;

        [SerializeField]
        private InputField searchInput;

        [SerializeField]
        private GameObject slotPrefab;

        [SerializeField]
        private Transform slotContainer;

        private void Awake()
        {
            if (searchService == null)
                searchService = GetComponent<CloneItemSearch>();

            if (searchInput != null)
                searchInput.onValueChanged.AddListener(OnSearchTextChanged);
        }

        private void OnDestroy()
        {
            if (searchInput != null)
                searchInput.onValueChanged.RemoveListener(OnSearchTextChanged);
        }

        private void OnSearchTextChanged(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                ClearResults();
                return;
            }

            var matches = searchService.SearchInventory(query);
            DisplayResults(matches);
        }

        private void DisplayResults(List<int> slotIndices)
        {
            ClearResults();

            if (slotContainer == null || slotPrefab == null)
                return;

            foreach (int index in slotIndices)
            {
                var slotGO = Instantiate(slotPrefab, slotContainer);
                var slotUI = slotGO.GetComponent<CloneInventorySlotUI>();
                if (slotUI != null)
                    slotUI.BindSlot(index);
            }
        }

        private void ClearResults()
        {
            if (slotContainer == null)
                return;

            foreach (Transform child in slotContainer)
                Destroy(child.gameObject);
        }
    }
}
