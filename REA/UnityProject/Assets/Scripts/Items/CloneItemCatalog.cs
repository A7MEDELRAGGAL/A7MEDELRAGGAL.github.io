using System.Collections.Generic;

namespace Clone.Items
{
    public static class CloneItemCatalog
    {
        public const int CedarRod = 1;
        public const int ReedRod = 2;
        public const int StoneSpear = 3;
        public const int IronKnife = 4;
        public const int StonePick = 5;
        public const int Torch = 6;
        public const int Rope = 7;
        public const int WormBait = 8;
        public const int DoughBait = 9;
        public const int Seaweed = 10;
        public const int Mackerel = 11;
        public const int Pike = 12;
        public const int Eel = 13;
        public const int Clam = 14;
        public const int Crab = 15;
        public const int Bread = 16;
        public const int Berry = 17;
        public const int Bandage = 18;
        public const int Antidote = 19;
	public const int KelpWrack = 20;
	public const int BossKey = 21;

        private static readonly Dictionary<int, CloneItemDefinition> Definitions =
            new Dictionary<int, CloneItemDefinition>();

        private static readonly Dictionary<string, int> ByName = new Dictionary<string, int>();

        private static bool _built;

        public static IEnumerable<CloneItemDefinition> All
        {
            get
            {
                EnsureBuilt();
                return Definitions.Values;
            }
        }

        public static int Count
        {
            get
            {
                EnsureBuilt();
                return Definitions.Count;
            }
        }

        public static CloneItemDefinition Get(int id)
        {
            EnsureBuilt();
            return Definitions.TryGetValue(id, out CloneItemDefinition definition) ? definition : null;
        }

        public static bool TryGet(int id, out CloneItemDefinition definition)
        {
            EnsureBuilt();
            return Definitions.TryGetValue(id, out definition);
        }

        public static bool TryFindByName(string name, out int id)
        {
            EnsureBuilt();
            if (string.IsNullOrEmpty(name))
            {
                id = -1;
                return false;
            }

            return ByName.TryGetValue(name.ToLowerInvariant(), out id);
        }

        public static int Register(CloneItemDefinition definition)
        {
            EnsureBuilt();
            if (definition == null)
                return -1;

            Definitions[definition.Id] = definition;
            if (!string.IsNullOrEmpty(definition.Name))
                ByName[definition.Name.ToLowerInvariant()] = definition.Id;
            return definition.Id;
        }

        public static CloneItemDefinition Create(int id, string name, CloneItemCategory category)
        {
            return new CloneItemDefinition
            {
                Id = id,
                Name = name,
                Category = category
            };
        }

        private static void EnsureBuilt()
        {
            if (_built)
                return;

            _built = true;
            BuildDefaultCatalog();
        }

        private static void BuildDefaultCatalog()
        {
            CloneItemDefinition rod = Create(CedarRod, "Cedar Rod", CloneItemCategory.FishingRod);
            rod.Weight = 1.6f;
            rod.Stackable = false;
            rod.CastRange = 18f;
            rod.ReelSpeed = 6f;
            rod.BiteDelayMin = 1.2f;
            rod.BiteDelayMax = 5f;
            rod.HookWindow = 1.35f;
            rod.FishHealth = 100f;
            Register(rod);

            CloneItemDefinition reed = Create(ReedRod, "Reed Rod", CloneItemCategory.FishingRod);
            reed.Weight = 1.1f;
            reed.Stackable = false;
            reed.CastRange = 11f;
            reed.ReelSpeed = 4.2f;
            reed.BiteDelayMin = 2f;
            reed.BiteDelayMax = 7.5f;
            reed.HookWindow = 1.1f;
            reed.FishHealth = 80f;
            Register(reed);

            CloneItemDefinition spear = Create(StoneSpear, "Stone Spear", CloneItemCategory.Melee);
            spear.Weight = 2.4f;
            spear.Stackable = false;
            spear.Damage = 26f;
            spear.Knockback = 7f;
            spear.AttackRange = 2.8f;
            Register(spear);

            CloneItemDefinition knife = Create(IronKnife, "Iron Knife", CloneItemCategory.Melee);
            knife.Weight = 0.7f;
            knife.Stackable = false;
            knife.Damage = 17f;
            knife.Knockback = 3.5f;
            knife.AttackRange = 2f;
            Register(knife);

            CloneItemDefinition pick = Create(StonePick, "Stone Pick", CloneItemCategory.Tool);
            pick.Weight = 2f;
            pick.Stackable = false;
            Register(pick);

            CloneItemDefinition torch = Create(Torch, "Torch", CloneItemCategory.Tool);
            torch.Weight = 0.9f;
            torch.Stackable = false;
            Register(torch);

            CloneItemDefinition rope = Create(Rope, "Rope Coil", CloneItemCategory.Tool);
            rope.Weight = 0.8f;
            rope.MaxStack = 3;
            Register(rope);

            CloneItemDefinition worm = Create(WormBait, "Worm Bait", CloneItemCategory.Bait);
            worm.Weight = 0.05f;
            worm.MaxStack = 40;
            Register(worm);

            CloneItemDefinition dough = Create(DoughBait, "Dough Bait", CloneItemCategory.Bait);
            dough.Weight = 0.06f;
            dough.MaxStack = 40;
            Register(dough);

            CloneItemDefinition kelp = Create(KelpWrack, "Kelp Wrack", CloneItemCategory.Resource);
            kelp.Weight = 0.3f;
            kelp.MaxStack = 12;
            Register(kelp);

            CloneItemDefinition mackerel = Create(Mackerel, "Mackerel", CloneItemCategory.Resource);
            mackerel.Weight = 0.9f;
            mackerel.MaxStack = 6;
            mackerel.IsEdible = true;
            mackerel.FullnessRestore = 14f;
            mackerel.HealthRestore = 2f;
            Register(mackerel);

            CloneItemDefinition pike = Create(Pike, "Pike", CloneItemCategory.Resource);
            pike.Weight = 2.6f;
            pike.MaxStack = 4;
            pike.IsEdible = true;
            pike.FullnessRestore = 26f;
            pike.HealthRestore = 5f;
            Register(pike);

            CloneItemDefinition eel = Create(Eel, "Eel", CloneItemCategory.Resource);
            eel.Weight = 2.1f;
            eel.MaxStack = 4;
            eel.IsEdible = true;
            eel.FullnessRestore = 20f;
            eel.HealthRestore = 4f;
            eel.PoisonCure = 0f;
            Register(eel);

            CloneItemDefinition clam = Create(Clam, "Clam", CloneItemCategory.Resource);
            clam.Weight = 0.4f;
            clam.MaxStack = 12;
            clam.IsEdible = true;
            clam.FullnessRestore = 8f;
            Register(clam);

            CloneItemDefinition crab = Create(Crab, "Crab", CloneItemCategory.Resource);
            crab.Weight = 0.6f;
            crab.MaxStack = 8;
            crab.IsEdible = true;
            crab.FullnessRestore = 10f;
            crab.HealthRestore = 3f;
            Register(crab);

            CloneItemDefinition bread = Create(Bread, "Bread Loaf", CloneItemCategory.Consumable);
            bread.Weight = 0.5f;
            bread.MaxStack = 5;
            bread.IsEdible = true;
            bread.FullnessRestore = 28f;
            Register(bread);

            CloneItemDefinition berry = Create(Berry, "Wild Berry", CloneItemCategory.Consumable);
            berry.Weight = 0.1f;
            berry.MaxStack = 20;
            berry.IsEdible = true;
            berry.FullnessRestore = 5f;
            Register(berry);

            CloneItemDefinition bandage = Create(Bandage, "Linen Bandage", CloneItemCategory.Consumable);
            bandage.Weight = 0.15f;
            bandage.MaxStack = 6;
            bandage.HealthRestore = 25f;
            Register(bandage);

            CloneItemDefinition antidote = Create(Antidote, "Charcoal Draught", CloneItemCategory.Consumable);
            antidote.Weight = 0.4f;
            antidote.MaxStack = 4;
            antidote.HealthRestore = 4f;
            antidote.PoisonCure = 100f;
			antidote.FireCure = 60f;
			Register(antidote);

			CloneItemDefinition bossKey = Create(BossKey, "Boss Key", CloneItemCategory.Resource);
			bossKey.Weight = 0.5f;
			bossKey.MaxStack = 1;
			bossKey.Stackable = false;
			Register(bossKey);
		}
    }
}
