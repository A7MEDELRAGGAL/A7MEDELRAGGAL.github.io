using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Clone.Economy;
using Clone.Player;
using Clone.World;

namespace Clone.SaveLoad
{
    public static class CloneSaveManager
    {
        private const string SaveFolderName = "savedgames";
        private const string FallbackSlotName = "clone-auto";

        public static string SlotsRoot => Path.Combine(Application.persistentDataPath, SaveFolderName);

        public static string SlotPath(string slotName)
        {
            return Path.Combine(SlotsRoot, SanitizeSlotName(slotName) + ".json");
        }

        public static string SanitizeSlotName(string slotName)
        {
            if (string.IsNullOrWhiteSpace(slotName))
                return FallbackSlotName;

            var invalid = Path.GetInvalidFileNameChars();
            var chars = slotName.Trim().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                for (int j = 0; j < invalid.Length; j++)
                {
                    if (chars[i] == invalid[j])
                    {
                        chars[i] = '_';
                        break;
                    }
                }
            }
            var name = new string(chars);
            return name.Length == 0 ? FallbackSlotName : name;
        }

        public static bool SaveSlot(string slotName)
        {
            try
            {
                var data = Collect();
                Directory.CreateDirectory(SlotsRoot);
                File.WriteAllText(SlotPath(slotName), JsonUtility.ToJson(data, true));
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public static CloneSaveData LoadSlot(string slotName)
        {
            try
            {
                var path = SlotPath(slotName);
                if (!File.Exists(path))
                    return null;
                return JsonUtility.FromJson<CloneSaveData>(File.ReadAllText(path));
            }
            catch (IOException)
            {
                return null;
            }
        }

        public static bool DeleteSlot(string slotName)
        {
            try
            {
                var path = SlotPath(slotName);
                if (!File.Exists(path))
                    return false;
                File.Delete(path);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public static bool SlotExists(string slotName)
        {
            return File.Exists(SlotPath(slotName));
        }

        public static List<string> ListSlots()
        {
            var slots = new List<string>();
            try
            {
                if (Directory.Exists(SlotsRoot))
                {
                    foreach (var file in Directory.GetFiles(SlotsRoot, "*.json"))
                        slots.Add(Path.GetFileNameWithoutExtension(file));
                }
            }
            catch (IOException)
            {
            }
            return slots;
        }

        public static CloneSaveData Collect()
        {
            var data = new CloneSaveData();

            var day = CloneDayNightManager.Instance;
            if (day != null)
            {
                data.DayNumber = day.DayNumber;
                data.TimeOfDay = day.TimeOfDay;
            }

            var players = Object.FindObjectsOfType<ClonePlayer>();
            var savedPlayers = new List<CloneSavedPlayer>();
            foreach (var player in players)
            {
                if (player == null)
                    continue;
                savedPlayers.Add(CollectPlayer(player));
            }
            data.Players = savedPlayers.ToArray();

            return data;
        }

        public static CloneSavedPlayer CollectPlayer(ClonePlayer player)
        {
            var saved = new CloneSavedPlayer();
            saved.Name = player.name;
            saved.Position = player.transform.position;

            var vitals = player.GetComponent<ClonePlayerVitals>();
            if (vitals != null)
            {
                saved.Health = vitals.Health;
                saved.Fullness = vitals.Fullness;
                saved.Poison = vitals.Poison;
                saved.Fire = vitals.Fire;
            }

            var currency = player.GetComponent<CloneCurrency>();
            if (currency != null)
                saved.Money = currency.Money;

            var equipment = player.GetComponent<ClonePlayerEquipment>();
            if (equipment != null)
            {
                var equipped = new int[ClonePlayerEquipment.SlotCount];
                for (int i = 0; i < ClonePlayerEquipment.SlotCount; i++)
                    equipped[i] = equipment.GetSlotItemId(i);
                saved.Equipment = equipped;
            }

            var inventory = player.Inventory;
            if (inventory != null)
            {
                var slots = new List<CloneSavedSlot>();
                for (int i = 0; i < ClonePlayerInventory.SlotCount; i++)
                {
                    var savedSlot = new CloneSavedSlot();
                    var slot = inventory.GetSlot(i);
                    if (slot != null && !slot.IsEmpty)
                    {
                        savedSlot.ItemId = slot.ItemId;
                        savedSlot.Amount = slot.Amount;
                        savedSlot.SkinId = slot.SkinId;
                        savedSlot.Cookness = slot.Cookness;
                    }
                    slots.Add(savedSlot);
                }
                saved.Inventory = slots.ToArray();
            }

            return saved;
        }

        public static void ApplySave(CloneSaveData data)
        {
            if (data == null)
                return;

            ApplyToDay(data);

            var players = Object.FindObjectsOfType<ClonePlayer>();
            for (int i = 0; i < players.Length; i++)
            {
                var saved = FindBestMatch(data.Players, players[i] != null ? players[i].name : null, i);
                if (saved != null)
                    ApplyToPlayer(players[i], saved);
            }
        }

        public static void ApplyToDay(CloneSaveData data)
        {
            var day = CloneDayNightManager.Instance;
            if (day != null)
                day.SetTimeOfDay(data.DayNumber, data.TimeOfDay);
        }

        public static void ApplyToPlayer(ClonePlayer player, CloneSavedPlayer saved)
        {
            if (player == null || saved == null)
                return;

            player.TeleportTarget(saved.Position);

            var vitals = player.GetComponent<ClonePlayerVitals>();
            if (vitals != null)
                vitals.ServerRestoreAll(saved.Health, saved.Fullness, saved.Poison, saved.Fire);

            var currency = player.GetComponent<CloneCurrency>();
            if (currency != null)
                currency.SetMoney(saved.Money);

            var equipment = player.GetComponent<ClonePlayerEquipment>();
            if (equipment != null && saved.Equipment != null)
            {
                for (int i = 0; i < ClonePlayerEquipment.SlotCount; i++)
                {
                    int itemId = i < saved.Equipment.Length ? saved.Equipment[i] : -1;
                    equipment.ServerApplySavedSlot(i, itemId);
                }
            }

            var inventory = player.Inventory;
            if (inventory == null)
                return;

            for (int i = 0; i < ClonePlayerInventory.SlotCount; i++)
            {
                var slot = inventory.GetSlot(i);
                if (slot == null)
                    continue;

                var savedSlot = (saved.Inventory != null && i < saved.Inventory.Length) ? saved.Inventory[i] : null;
                if (savedSlot != null && savedSlot.ItemId > 0 && savedSlot.Amount > 0)
                    slot.Set(savedSlot.ItemId, savedSlot.Amount, savedSlot.SkinId, savedSlot.Cookness);
                else
                    slot.Clear();
            }
        }

        private static CloneSavedPlayer FindBestMatch(CloneSavedPlayer[] savedPlayers, string playerName, int index)
        {
            if (savedPlayers == null || savedPlayers.Length == 0)
                return null;

            if (!string.IsNullOrEmpty(playerName))
            {
                for (int i = 0; i < savedPlayers.Length; i++)
                {
                    if (savedPlayers[i] != null && savedPlayers[i].Name == playerName)
                        return savedPlayers[i];
                }
            }

            return index < savedPlayers.Length ? savedPlayers[index] : null;
        }
    }
}
