using UnityEngine;
using UnityEngine.UI;
using Clone.Items;
using Clone.Player;

namespace Clone.Search
{
    [RequireComponent(typeof(Image))]
    public class CloneInventorySlotUI : MonoBehaviour
    {
        [SerializeField]
        private Image iconImage;
        [SerializeField]
        private Text amountText;
        [SerializeField]
        private Text itemNameText;

        private int slotIndex;

        public void BindSlot(int index)
        {
            slotIndex = index;
            Refresh();
        }

        private void Refresh()
        {
            var inventory = ClonePlayer.LocalPlayer?.Inventory;
            if (inventory == null)
            {
                gameObject.SetActive(false);
                return;
            }

            var slot = inventory.GetSlot(slotIndex);
            if (slot == null || slot.IsEmpty)
            {
                gameObject.SetActive(false);
                return;
            }

            var def = CloneItemCatalog.Get(slot.ItemId);
            if (def == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (itemNameText != null)
                itemNameText.text = def.Name;

            if (amountText != null)
                amountText.text = slot.Amount > 1 ? slot.Amount.ToString() : string.Empty;
        }
    }
}
