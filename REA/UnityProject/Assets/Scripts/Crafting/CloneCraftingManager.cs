using System.Collections.Generic;
using FishNet.Object;
using Clone.Items;
using Clone.Player;
using UnityEngine;

namespace Clone.Crafting
{
    public enum CloneRecipeCategory
    {
        Tools,
        Weapons,
        Gear,
        Consumables,
        Misc
    }

    [System.Serializable]
    public struct CloneRecipeIngredient
    {
        public int ItemId;
        public int Amount;

        public CloneRecipeIngredient(int itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;
        }
    }

    [System.Serializable]
    public struct CloneRecipe
    {
        public int Id;
        public string DisplayName;
        public CloneRecipeCategory Category;
        public int OutputItemId;
        public int OutputAmount;
        public CloneRecipeIngredient[] Inputs;

        public bool AllIngredientsPresent(ClonePlayerInventory inventory)
        {
            for (int i = 0; i < Inputs.Length; i++)
            {
                if (!inventory.ServerHasItems(Inputs[i].ItemId, Inputs[i].Amount))
                    return false;
            }
            return true;
        }
    }

    // Server-authoritative crafting: clients only request; the server checks,
    // consumes, and stores. Recipes are tooled in code so ids stay stable
    // across rebuilds.
    public class CloneCraftingManager : NetworkBehaviour
    {
        [Header("Recipe Registry")]
        [SerializeField] private List<CloneRecipe> recipes = new List<CloneRecipe>();

        public IReadOnlyList<CloneRecipe> Recipes => recipes;

        private int nextRecipeId = 1;

        private void Awake()
        {
            BuildDefaultRecipes();
        }

        private void BuildDefaultRecipes()
        {
            AddRecipe(Out(CloneItemCatalog.WoodAxe, 1),
                In(CloneItemCatalog.WoodPlanks, 4),
                In(CloneItemCatalog.Fiber, 2));

            AddRecipe(Out(CloneItemCatalog.WoodPick, 1),
                In(CloneItemCatalog.WoodPlanks, 4),
                In(CloneItemCatalog.Fiber, 2));

            AddRecipe(Out(CloneItemCatalog.StoneSword, 1),
                In(CloneItemCatalog.Stone, 6),
                In(CloneItemCatalog.Fiber, 2));

            AddRecipe(Out(CloneItemCatalog.ArmorWoodenHelm, 1),
                In(CloneItemCatalog.WoodPlanks, 6),
                In(CloneItemCatalog.Fiber, 3));

            AddRecipe(Out(CloneItemCatalog.ArmorWoodenChest, 1),
                In(CloneItemCatalog.WoodPlanks, 10),
                In(CloneItemCatalog.Fiber, 4));

            AddRecipe(Out(CloneItemCatalog.ArmorWoodenLegs, 1),
                In(CloneItemCatalog.WoodPlanks, 8),
                In(CloneItemCatalog.Fiber, 3));

            AddRecipe(Out(CloneItemCatalog.ArmorWoodenBoots, 1),
                In(CloneItemCatalog.WoodPlanks, 6),
                In(CloneItemCatalog.Fiber, 3));

            AddRecipe(Out(CloneItemCatalog.WoodPlanks, 4),
                In(CloneItemCatalog.WoodLog, 2));
        }

        private static int Out(int itemId, int amount)
        {
            return (itemId << 16) | (amount & 0xFFFF);
        }

        private static int OutId(int packed)
        {
            return packed >> 16;
        }

        private static int OutAmount(int packed)
        {
            return packed & 0xFFFF;
        }

        private static CloneRecipeIngredient In(int itemId, int amount)
        {
            return new CloneRecipeIngredient(itemId, amount);
        }

        private static CloneRecipeCategory CategoryForItemId(int itemId)
        {
            if (itemId >= CloneItemCatalog.ArmorWoodenHelm &&
                itemId <= CloneItemCatalog.ArmorWoodenBoots)
                return CloneRecipeCategory.Gear;

            if (itemId == CloneItemCatalog.WoodAxe ||
                itemId == CloneItemCatalog.WoodPick ||
                itemId == CloneItemCatalog.StoneSword)
                return CloneRecipeCategory.Weapons;

            return CloneRecipeCategory.Tools;
        }

        private static string BuildDisplayName(int itemId)
        {
            CloneItemDefinition definition = CloneItemCatalog.GetById(itemId);
            return definition != null ? definition.Name : "Crafted Item";
        }

        private void AddRecipe(int packedOutput, params CloneRecipeIngredient[] inputs)
        {
            int outputId = OutId(packedOutput);
            recipes.Add(new CloneRecipe
            {
                Id = nextRecipeId++,
                DisplayName = BuildDisplayName(outputId),
                Category = CategoryForItemId(outputId),
                OutputItemId = outputId,
                OutputAmount = OutAmount(packedOutput),
                Inputs = inputs
            });
        }

        private CloneRecipe FindRecipe(int recipeId)
        {
            for (int i = 0; i < recipes.Count; i++)
            {
                if (recipes[i].Id == recipeId)
                    return recipes[i];
            }
            return default;
        }

        [ServerRpc(RequireOwnership = false)]
        public void ServerRequestCraft(int recipeId, ServerRpcParams rpcParams = default)
        {
            ExecuteCraft(recipeId);
        }

        [Server]
        private void ExecuteCraft(int recipeId)
        {
            CloneRecipe recipe = FindRecipe(recipeId);
            if (recipe.Id != recipeId)
                return;

            ClonePlayerInventory inventory = GetComponent<ClonePlayerInventory>();
            if (inventory == null)
                return;

            if (!recipe.AllIngredientsPresent(inventory))
                return;

            for (int i = 0; i < recipe.Inputs.Length; i++)
            {
                if (!inventory.ServerConsume(recipe.Inputs[i].ItemId, recipe.Inputs[i].Amount))
                    return;
            }

            CloneItemDefinition outputDefinition = CloneItemCatalog.GetById(recipe.OutputItemId);
            if (outputDefinition != null && outputDefinition.Category == CloneItemCategory.Gear)
            {
                var equipment = GetComponent<ClonePlayerEquipment>();
                if (equipment == null)
                    return;

                equipment.ServerEquip(recipe.OutputItemId, false);
                Debug.Log($"[CloneCrafting] Equipped {recipe.DisplayName}");
                return;
            }

            if (inventory.ServerTryStore(recipe.OutputItemId, recipe.OutputAmount, 0, 0f, out int remaining))
            {
                if (remaining > 0)
                    Debug.Log($"[CloneCrafting] Inventory full; {remaining}x {recipe.DisplayName} lost");
            }
            else
            {
                Debug.LogWarning($"[CloneCrafting] Could not store {recipe.DisplayName}");
            }

            Debug.Log($"[CloneCrafting] Crafted {recipe.OutputAmount}x {recipe.DisplayName}");
        }
    }
}
