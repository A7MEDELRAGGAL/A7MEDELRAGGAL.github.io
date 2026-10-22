using System;
using UnityEngine;

namespace Clone.SaveLoad
{
    [Serializable]
    public class CloneSaveData
    {
        public const int CurrentVersion = 1;

        public int Version = CurrentVersion;
        public int DayNumber = 1;
        public float TimeOfDay;
        public CloneSavedPlayer[] Players = Array.Empty<CloneSavedPlayer>();
    }

    [Serializable]
    public class CloneSavedPlayer
    {
        public string Name;
        public Vector3 Position;
        public float Health = 100f;
        public float Fullness = 100f;
        public float Poison;
        public float Fire;
        public int Money;
        public int[] Equipment;
        public CloneSavedSlot[] Inventory = Array.Empty<CloneSavedSlot>();
    }

    [Serializable]
    public class CloneSavedSlot
    {
        public int ItemId;
        public int Amount = 1;
        public int SkinId;
        public float Cookness;
    }
}
