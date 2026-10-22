using System;

namespace Clone.Items
{
    public enum CloneItemCategory
    {
        Tool,
        Weapon,
        Melee,
        FishingRod,
        Bait,
        Consumable,
        Resource,
        Gear
    }

    [Serializable]
    public class CloneItemDefinition
    {
        public int Id = -1;
        public string Name = string.Empty;
        public CloneItemCategory Category = CloneItemCategory.Resource;

        [Header("Stacking")]
        public bool Stackable = true;
        public int MaxStack = 10;

        [Header("Physical")]
        public float Weight = 0.4f;

        [Header("State")]
        public int SkinId;
        public float Cookness;

        [Header("Combat")]
        public float Damage = 1f;
        public float Knockback = 3f;
        public float AttackRange = 2.2f;

        [Header("Fishing")]
        public float CastRange = 14f;
        public float ReelSpeed = 5f;
        public float BiteDelayMin = 1.5f;
        public float BiteDelayMax = 6f;
        public float HookWindow = 1.25f;
        public float FishHealth = 100f;

        [Header("Consumable")]
        public bool IsEdible;
        public float FullnessRestore = 10f;
        public float HealthRestore;
        public float PoisonCure;
        public float FireCure;

        [Header("Armor")]
        public int ArmorTier;
        public float Protection;
        public float Agility;

        public bool IsRod => Category == CloneItemCategory.FishingRod;
        public bool IsBait => Category == CloneItemCategory.Bait;
        public bool IsEdibleOrMedical => IsEdible || HealthRestore > 0f || PoisonCure > 0f || FireCure > 0f;

        public float TotalWeight(int amount)
        {
            return Weight * (amount < 1 ? 1 : amount);
        }
    }
}
