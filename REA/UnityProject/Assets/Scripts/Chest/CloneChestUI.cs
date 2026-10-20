using System.Collections.Generic;
using Clone.Items;
using Clone.Player;
using Clone.World;
using UnityEngine;
using UnityEngine.UI;

namespace Clone.ChestUI
{
    public class CloneChestUI : MonoBehaviour
    {
        [SerializeField]
        private GameObject panel;

        [SerializeField]
        private Transform slotContainer;

        [SerializeField]
        private GameObject slotPrefab;

        [SerializeField]
        private Button storeSelectedButton;

        [SerializeField]
        private Text statusText;

        private readonly List<CloneChestSlotUI> slotUIs = new List<CloneChestSlotUI>();
        private CloneChest boundChest;

        private void Awake()
        {
            BuildSlots();

            if (panel != null)
                panel.SetActive(false);

            if (storeSelectedButton != null)
                storeSelectedButton.onClick.AddListener(OnStoreSelectedClicked);
        }

        private void OnDestroy()
        {
            UnbindChest();

            if (storeSelectedButton != null)
                storeSelectedButton.onClick.RemoveListener(OnStoreSelectedClicked);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
                Toggle();
            else if (panel != null && panel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
                Close();
        }

        public void Toggle()
        {
            if (panel != null && panel.activeSelf)
            {
                Close();
                return;
            }

            var player = ClonePlayer.LocalPlayer;
            if (player == null)
                return;

            if (!CloneChest.TryGetNearbyChest(player, CloneChest.DefaultReach, out CloneChest chest))
            {
                SetStatus("No chest nearby");
                return;
            }

            BindChest(chest);
            chest.ServerRequestOpenChest();

            if (panel != null)
                panel.SetActive(true);

            SetStatus("Shared chest");
            RefreshSlots();
        }

        public void Close()
        {
            UnbindChest();

            if (panel != null)
                panel.SetActive(false);

            SetStatus(string.Empty);
        }

        public void OnStoreSelectedClicked()
        {
            var inventory = ClonePlayer.LocalPlayer != null ? ClonePlayer.LocalPlayer.Inventory : null;
            if (inventory == null || boundChest == null)
                return;

            boundChest.ServerRequestStore(inventory.SelectedSlot, 999999);
        }

        public void RequestWithdraw(int slotIndex, int amount)
        {
            if (boundChest == null)
                return;

            boundChest.ServerRequestWithdraw(slotIndex, amount);
        }

        private void BindChest(CloneChest chest)
        {
            if (chest == null)
                return;

            if (boundChest == chest)
                return;

            UnbindChest();
            boundChest = chest;
            chest.ChestOpened += OnChestOpened;
            chest.ChestStored += OnChestStored;
            chest.ChestWithdrawn += OnChestWithdrawn;
            chest.ChestStateRefreshed += OnChestStateRefreshed;
        }

        private void UnbindChest()
        {
            if (boundChest == null)
                return;

            boundChest.ChestOpened -= OnChestOpened;
            boundChest.ChestStored -= OnChestStored;
            boundChest.ChestWithdrawn -= OnChestWithdrawn;
            boundChest.ChestStateRefreshed -= OnChestStateRefreshed;
            boundChest = null;
        }

        private void OnChestOpened()
        {
            SetStatus("Shared chest opened");
            RefreshSlots();
        }

        private void OnChestStored(int slot, int itemId, int amount)
        {
            SetStatus("Stored " + amount + " x " + ResolveItemName(itemId));
            RefreshSlots();
        }

        private void OnChestWithdrawn(int slot, int itemId, int amount)
        {
            SetStatus("Withdrew " + amount + " x " + ResolveItemName(itemId));
            RefreshSlots();
        }

        private void OnChestStateRefreshed()
        {
            RefreshSlots();
        }

        private void RefreshSlots()
        {
            for (int i = 0; i < slotUIs.Count; i++)
            {
                var slotUI = slotUIs[i];
                if (slotUI != null)
                    slotUI.Refresh(boundChest);
            }
        }

        private static string ResolveItemName(int itemId)
        {
            if (itemId == CloneInventorySlot.EmptyId)
                return string.Empty;

            var definition = CloneItemCatalog.Get(itemId);
            return definition != null ? definition.Name : string.Empty;
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }

        private void BuildSlots()
        {
            if (slotContainer == null || slotPrefab == null)
                return;

            for (int i = 0; i < CloneChest.ChestSlotCount; i++)
            {
                GameObject slotObject = Instantiate(slotPrefab, slotContainer);
                var slotUI = slotObject.GetComponent<CloneChestSlotUI>();
                if (slotUI != null)
                {
                    slotUI.Bind(this, i);
                    slotUIs.Add(slotUI);
                }
            }
        }
    }

    public class CloneChestSlotUI : MonoBehaviour
    {
        [SerializeField]
        private Text itemNameText;

        [SerializeField]
        private Text amountText;

        private CloneChestUI owner;
        private int slotIndex = -1;

        public int SlotIndex => slotIndex;

        public void Bind(CloneChestUI newOwner, int newSlotIndex)
        {
            owner = newOwner;
            slotIndex = newSlotIndex;
            Button button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    if (owner != null && slotIndex >= 0)
                    {
                        owner.ClickSlot();
                    }
                });
            }
        }

        public void Refresh(CloneChest chest)
        {
            if (chest == null)
            {
                Clear();
                return;
            }

            int itemId = chest.GetViewItemId(slotIndex);
            int amount = chest.GetViewAmount(slotIndex);

            if (itemId == CloneInventorySlot.EmptyId)
            {
                Clear();
                return;
            }

            var definition = CloneItemCatalog.Get(itemId);

            if (itemNameText != null)
                itemNameText.text = definition != null ? definition.Name : string.Empty;

            if (amountText != null)
                amountText.text = amount > 1 ? amount.ToString() : string.Empty;

            var button = GetComponent<Button>();
            if (button != null)
                button.interactable = true;
        }

        public void Clear()
        {
            if (itemNameText != null)
                itemNameText.text = string.Empty;

            if (amountText != null)
                amountText.text = string.Empty;
        }

        public void ClickSlot()
        {
            if (owner != null && slotIndex >= 0)
                owner.RequestWithdraw(slotIndex, 999999);
        }
    }
}
