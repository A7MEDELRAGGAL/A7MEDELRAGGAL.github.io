using System.Collections.Generic;
using Clone.Items;
using Clone.Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Clone.CraftingUI
{
    // Runtime-built crafting panel: no scene objects or prefabs required.
    // Attached to the owning player object; [C] toggles the window.
    public class CloneCraftingUI : MonoBehaviour
    {
        private CloneCraftingManager crafting;
        private GameObject panel;
        private Text statusText;
        private Transform rows;
        private float refreshTimer;

        public static void EnsureFor(CloneCraftingManager manager)
        {
            if (manager == null || manager.GetComponent<CloneCraftingUI>() != null)
                return;

            var host = manager.gameObject.AddComponent<CloneCraftingUI>();
            host.crafting = manager;
        }

        private void Start()
        {
            BuildCanvas();
        }

        private void OnDestroy()
        {
            if (panel != null)
                Destroy(panel);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
                Toggle();

            if (panel != null && panel.activeSelf)
            {
                refreshTimer += Time.unscaledDeltaTime;
                if (refreshTimer > 1f)
                {
                    refreshTimer = 0f;
                    RefreshRows();
                }
            }
        }

        public void Toggle()
        {
            if (ClonePlayer.LocalPlayer == null || crafting == null || panel == null)
                return;

            if (panel.activeSelf)
            {
                panel.SetActive(false);
                return;
            }

            RefreshRows();
            panel.SetActive(true);
        }

        private void RefreshRows()
        {
            if (rows == null)
                return;

            foreach (Transform child in rows)
                Destroy(child.gameObject);

            if (crafting != null)
            {
                foreach (var recipe in crafting.Recipes)
                {
                    string label = recipe.DisplayName + "\n" + DescribeIngredients(recipe);
                    int recipeId = recipe.Id;
                    CreateRow(label, enabled: true, onClick: () => crafting.ServerRequestCraft(recipeId));
                }
            }

            var equipment = ClonePlayer.LocalPlayer != null ? ClonePlayer.LocalPlayer.Equipment : null;
            if (equipment != null)
            {
                for (int slot = 0; slot < ClonePlayerEquipment.SlotCount; slot++)
                {
                    int itemId = equipment.GetSlotItemId(slot);
                    CloneItemDefinition definition = itemId != CloneInventorySlot.EmptyId ? CloneItemCatalog.Get(itemId) : null;
                    string slotName = SlotName(slot);
                    int capturedSlot = slot;

                    if (definition != null)
                        CreateRow(slotName + ": " + definition.Name + "  [unequip]", true,
                            () => equipment.ServerRequestUnequip(capturedSlot));
                    else
                        CreateRow(slotName + ": (nothing equipped)", false, null);
                }
            }

            if (statusText != null)
                statusText.text = "[C] close - click a row to craft";
        }

        private void CreateRow(string label, bool enabled, UnityAction onClick)
        {
            GameObject row = new GameObject("CraftRow", typeof(RectTransform), typeof(Image), typeof(Button));
            row.transform.SetParent(rows, false);

            Image background = row.GetComponent<Image>();
            background.color = enabled ? new Color(0.15f, 0.15f, 0.18f, 0.9f) : new Color(0.1f, 0.1f, 0.1f, 0.6f);

            var rect = row.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 44f);

            var layoutElement = row.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 44f;
            layoutElement.flexibleWidth = 1f;

            Button button = row.GetComponent<Button>();
            button.interactable = enabled;
            if (enabled && onClick != null)
                button.onClick.AddListener(onClick);

            GameObject textObject = new GameObject("RowLabel", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(row.transform, false);
            var text = textObject.GetComponent<Text>();
            text.text = label;
            text.fontSize = 15;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10f, 2f);
            textRect.offsetMax = new Vector2(-10f, -2f);
        }

        private static string DescribeIngredients(CloneRecipe recipe)
        {
            var parts = new List<string>();
            for (int i = 0; i < recipe.Inputs.Length; i++)
            {
                CloneItemDefinition definition = CloneItemCatalog.Get(recipe.Inputs[i].ItemId);
                string name = definition != null ? definition.Name : "Item";
                parts.Add(name + " x" + recipe.Inputs[i].Amount);
            }
            return string.Join(", ", parts);
        }

        private static string SlotName(int slot)
        {
            switch (slot)
            {
                case ClonePlayerEquipment.Helm:
                    return "Helm";
                case ClonePlayerEquipment.Chest:
                    return "Chest";
                case ClonePlayerEquipment.Legs:
                    return "Legs";
                case ClonePlayerEquipment.Boots:
                    return "Boots";
                default:
                    return "Slot " + slot;
            }
        }

        private void BuildCanvas()
        {
            GameObject canvasObject = new GameObject("CloneCraftingCanvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);

            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("CloneCraftingEventSystem",
                    typeof(EventSystem), typeof(StandaloneInputModule));
                eventSystem.transform.SetParent(canvasObject.transform, false);
            }

            panel = new GameObject("CraftingPanel", typeof(RectTransform), typeof(Image),
                typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            panel.transform.SetParent(canvasObject.transform, false);
            panel.SetActive(false);

            Image panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0.06f, 0.06f, 0.08f, 0.95f);

            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(380f, 0f);

            var fitter = panel.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var group = panel.GetComponent<VerticalLayoutGroup>();
            group.spacing = 4f;
            group.padding = new RectOffset(10, 10, 10, 10);
            group.childControlWidth = true;
            group.childControlHeight = false;
            group.childForceExpandHeight = false;
            group.childForceExpandWidth = true;

            GameObject titleObject = new GameObject("CraftTitle", typeof(RectTransform), typeof(Text));
            titleObject.transform.SetParent(panel.transform, false);
            Text title = titleObject.GetComponent<Text>();
            title.text = "Crafting";
            title.fontSize = 20;
            title.color = Color.white;
            title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            title.alignment = TextAnchor.MiddleCenter;

            var titleLayout = titleObject.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 28f;

            GameObject statusObject = new GameObject("CraftStatus", typeof(RectTransform), typeof(Text));
            statusObject.transform.SetParent(panel.transform, false);
            statusText = statusObject.GetComponent<Text>();
            statusText.fontSize = 13;
            statusText.color = new Color(0.8f, 0.8f, 0.8f);
            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusText.alignment = TextAnchor.MiddleCenter;

            var statusLayout = statusObject.AddComponent<LayoutElement>();
            statusLayout.preferredHeight = 22f;

            GameObject rowsObject = new GameObject("CraftRows", typeof(RectTransform),
                typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            rowsObject.transform.SetParent(panel.transform, false);
            rows = rowsObject.transform;

            var rowsGroup = rowsObject.GetComponent<VerticalLayoutGroup>();
            rowsGroup.spacing = 2f;
            rowsGroup.childControlWidth = true;
            rowsGroup.childControlHeight = false;
            rowsGroup.childForceExpandHeight = false;
            rowsGroup.childForceExpandWidth = true;

            var rowsFitter = rowsObject.GetComponent<ContentSizeFitter>();
            rowsFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }
}
