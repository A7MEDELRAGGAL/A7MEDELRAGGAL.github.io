using Clone.Items;
using Clone.World;
using UnityEngine;
using UnityEngine.UI;

namespace Clone.Hud
{
    // Runtime-built HUD: vitals bars, carry weight, and the hotbar row.
    // No scene objects or prefabs required.
    public class ClonePlayerHud : MonoBehaviour
    {
        private Text _healthText;
        private Image _healthFill;
        private Text _fullnessText;
        private Image _fullnessFill;
        private Text _weightText;
        private Text[] _slotTexts = System.Array.Empty<Text>();

        public static void EnsureFor(ClonePlayer player)
        {
            if (player == null || player.GetComponent<ClonePlayerHud>() != null)
                return;

            player.gameObject.AddComponent<ClonePlayerHud>();
        }

        private void Start()
        {
            BuildCanvas();
        }

        private void Update()
        {
            var player = ClonePlayer.LocalPlayer;
            if (player == null)
                return;

            RefreshVitals(player);
            RefreshWeight(player);
            RefreshHotbar(player);
        }

        private static Font BuiltinFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private GameObject CreateBar(Transform parent, Vector2 anchor, Vector2 size, out Image fill, out Text label)
        {
            GameObject bar = new GameObject("HudBar", typeof(RectTransform), typeof(Image));
            bar.transform.SetParent(parent, false);
            Image background = bar.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.55f);

            var rect = bar.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.anchorMin = new Vector2(0f, anchor.y);
            rect.anchorMax = new Vector2(0f, anchor.y);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = size;

            GameObject fillObject = new GameObject("HudFill", typeof(RectTransform), typeof(Image));
            fillObject.transform.SetParent(bar.transform, false);
            fill = fillObject.GetComponent<Image>();
            fill.color = new Color(0.7f, 0.2f, 0.2f);
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = Vector2.one;
            fill.rectTransform.offsetMin = Vector2.zero;
            fill.rectTransform.offsetMax = Vector2.zero;

            GameObject labelObject = new GameObject("HudLabel", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(bar.transform, false);
            label = labelObject.GetComponent<Text>();
            label.font = BuiltinFont();
            label.fontSize = 14;
            label.color = Color.white;
            label.alignment = TextAnchor.MiddleCenter;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;

            return bar;
        }

        private void RefreshVitals(ClonePlayer player)
        {
            var vitals = ClonePlayer.LocalPlayer.Vitals;
            if (vitals == null)
                return;

            if (_healthFill != null)
            {
                _healthFill.color = new Color(0.75f, 0.15f, 0.15f);
                _healthFill.rectTransform.localScale = new Vector3(
                    Mathf.Clamp01(vitals.Health / ClonePlayerVitals.MaxHealth), 1f, 1f);
            }
            if (_healthText != null)
                _healthText.text = "HP " + Mathf.RoundToInt(vitals.Health);

            if (_fullnessFill != null)
            {
                _fullnessFill.color = new Color(0.85f, 0.6f, 0.15f);
                _fullnessFill.rectTransform.localScale = new Vector3(
                    Mathf.Clamp01(vitals.Fullness / ClonePlayerVitals.MaxFullness), 1f, 1f);
            }
            if (_fullnessText != null)
                _fullnessText.text = "Food " + Mathf.RoundToInt(vitals.Fullness);
        }

        private void RefreshWeight(ClonePlayer player)
        {
            var inventory = player.Inventory;
            if (inventory == null || _weightText == null)
                return;

            _weightText.text = "Weight " + Mathf.Round(inventory.CarryWeight) + " / " +
                ClonePlayerInventory.MaxCarryWeight;
        }

        private void RefreshHotbar(ClonePlayer player)
        {
            if (_slotTexts.Length == 0)
                return;

            var inventory = player.Inventory;
            for (int i = 0; i < _slotTexts.Length; i++)
            {
                var text = _slotTexts[i];
                if (text == null)
                    continue;

                CloneInventorySlot slot = inventory != null ? inventory.GetSlot(i) : null;
                if (slot == null || slot.IsEmpty)
                {
                    text.text = i == player.Inventory.SelectedSlot ? "[" + (i + 1) + "]" : "";
                    text.color = new Color(1f, 1f, 1f, 0.25f);
                    continue;
                }

                var definition = CloneItemCatalog.Get(slot.ItemId);
                string name = definition != null ? definition.Name : "?";
                bool selected = player.Inventory.SelectedSlot == i;
                text.text = name + (slot.Amount > 1 ? " x" + slot.Amount : "");
                text.color = selected ? Color.yellow : Color.white;
            }
        }

        private void BuildCanvas()
        {
            GameObject canvasObject = new GameObject("CloneHudCanvas",
                typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);

            var root = new GameObject("HudRoot", typeof(RectTransform));
            root.transform.SetParent(canvasObject.transform, false);
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            CreateBar(root.transform, new Vector2(12f, -12f), new Vector2(220f, 20f),
                out _healthFill, out _healthText);

            var fullness = CreateBar(root.transform, new Vector2(12f, -40f), new Vector2(220f, 20f),
                out _fullnessFill, out _fullnessText);
            RectTransform fullnessRect = fullness.GetComponent<RectTransform>();
            RectTransform healthRect = _healthText.transform.parent.GetComponent<RectTransform>();
            healthRect.anchoredPosition = new Vector2(12f, -12f);
            fullnessRect.anchoredPosition = new Vector2(12f, -38f);

            GameObject weightObject = new GameObject("HudWeight", typeof(RectTransform), typeof(Text));
            weightObject.transform.SetParent(root.transform, false);
            _weightText = weightObject.GetComponent<Text>();
            _weightText.font = BuiltinFont();
            _weightText.fontSize = 13;
            _weightText.color = new Color(1f, 1f, 1f, 0.8f);
            _weightText.alignment = TextAnchor.UpperLeft;
            var weightRect = _weightText.rectTransform;
            weightRect.anchorMin = new Vector2(0f, 1f);
            weightRect.anchorMax = new Vector2(0f, 1f);
            weightRect.pivot = new Vector2(0f, 1f);
            weightRect.anchoredPosition = new Vector2(12f, -66f);
            weightRect.sizeDelta = new Vector2(260f, 18f);

            GameObject hintObject = new GameObject("HudHints", typeof(RectTransform), typeof(Text));
            hintObject.transform.SetParent(root.transform, false);
            Text hints = hintObject.GetComponent<Text>();
            hints.font = BuiltinFont();
            hints.fontSize = 13;
            hints.color = new Color(1f, 1f, 1f, 0.6f);
            hints.alignment = TextAnchor.UpperLeft;
            hints.text = "[E] harvest  [C] craft  [G] drop  [R] eat  [B] bait  [V] camera  [1-9] select";
            var hintRect = hints.rectTransform;
            hintRect.anchorMin = new Vector2(0f, 1f);
            hintRect.anchorMax = new Vector2(1f, 1f);
            hintRect.pivot = new Vector2(0f, 1f);
            hintRect.anchoredPosition = new Vector2(12f, -88f);
            hintRect.sizeDelta = new Vector2(-24f, 18f);

            GameObject hotbar = new GameObject("Hotbar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            hotbar.transform.SetParent(root.transform, false);
            var hotbarRect = hotbar.GetComponent<RectTransform>();
            hotbarRect.anchorMin = new Vector2(0.5f, 0f);
            hotbarRect.anchorMax = new Vector2(0.5f, 0f);
            hotbarRect.pivot = new Vector2(0.5f, 0f);
            hotbarRect.sizeDelta = new Vector2(9 * 74f, 40f);
            hotbarRect.anchoredPosition = new Vector2(0f, 10f);

            var hotbarGroup = hotbar.GetComponent<HorizontalLayoutGroup>();
            hotbarGroup.spacing = 4f;
            hotbarGroup.childControlWidth = true;
            hotbarGroup.childControlHeight = true;
            hotbarGroup.childForceExpandWidth = true;
            hotbarGroup.childForceExpandHeight = true;

            _slotTexts = new Text[ClonePlayerInventory.HotbarCount];
            for (int i = 0; i < ClonePlayerInventory.HotbarCount; i++)
            {
                GameObject slot = new GameObject("HotbarSlot" + i,
                    typeof(RectTransform), typeof(Image), typeof(Text));
                slot.transform.SetParent(hotbar.transform, false);
                Image background = slot.GetComponent<Image>();
                background.color = new Color(0f, 0f, 0f, 0.55f);
                Text text = slot.GetComponent<Text>();

                text.transform.SetParent(slot.transform, false);
                text.font = BuiltinFont();
                text.fontSize = 11;
                text.alignment = TextAnchor.MiddleCenter;
                text.rectTransform.anchorMin = Vector2.zero;
                text.rectTransform.anchorMax = Vector2.one;
                text.rectTransform.offsetMin = new Vector2(2f, 2f);
                text.rectTransform.offsetMax = new Vector2(-2f, -2f);

                _slotTexts[i] = text;
            }
        }
    }
}
