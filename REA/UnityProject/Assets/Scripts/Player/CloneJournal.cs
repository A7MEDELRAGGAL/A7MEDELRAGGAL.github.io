using System;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

namespace Clone.Player
{
    [Serializable]
    public class CloneJournalEntry
    {
        public uint Id;
        public string Name = string.Empty;
        public int Count;
        public int Worth;
        public bool Discovered;
        public bool Defeated;
        public string Note = string.Empty;
    }

    public class CloneJournal : NetworkBehaviour
    {
        private readonly List<CloneJournalEntry> _entries = new List<CloneJournalEntry>();

        public IReadOnlyList<CloneJournalEntry> Entries => _entries;
        public int Count => _entries.Count;

        public bool TryFind(uint id, out CloneJournalEntry entry)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Id != id)
                    continue;

                entry = _entries[i];
                return true;
            }

            entry = null;
            return false;
        }

        public bool Has(uint id)
        {
            return TryFind(id, out _);
        }

        public List<CloneJournalEntry> Search(string query, int limit)
        {
            string needle = (query ?? string.Empty).Trim().ToLowerInvariant();
            int max = limit > 0 ? limit : 32;
            var results = new List<CloneJournalEntry>();

            for (int i = 0; i < _entries.Count && results.Count < max; i++)
            {
                if (Matches(_entries[i], needle))
                    results.Add(_entries[i]);
            }

            return results;
        }

        public static bool Matches(CloneJournalEntry entry, string needle)
        {
            if (entry == null)
                return false;

            if (string.IsNullOrEmpty(needle))
                return true;

            if (!string.IsNullOrEmpty(entry.Name) &&
                entry.Name.ToLowerInvariant().Contains(needle))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(entry.Note) &&
                entry.Note.ToLowerInvariant().Contains(needle))
            {
                return true;
            }

            if (entry.Id.ToString() == needle)
                return true;

            return entry.Count.ToString() == needle;
        }

        public string Describe(CloneJournalEntry entry)
        {
            if (entry == null)
                return string.Empty;

            var parts = new List<string>();
            if (entry.Count > 1)
                parts.Add("x" + entry.Count);
            if (entry.Worth > 0)
                parts.Add(entry.Worth + " coins");
            if (entry.Defeated)
                parts.Add("defeated");
            else if (entry.Discovered)
                parts.Add("spotted");
            if (!string.IsNullOrEmpty(entry.Note))
                parts.Add(entry.Note);

            return string.Join(" - ", parts);
        }

        public void Record(uint id, string name, int count, int worth, bool defeated, string note)
        {
            if (id == 0)
                return;

            if (!TryFind(id, out CloneJournalEntry entry))
            {
                entry = new CloneJournalEntry { Id = id, Name = name ?? string.Empty };
                _entries.Add(entry);
            }

            if (!string.IsNullOrEmpty(name))
                entry.Name = name;

            entry.Count += count;
            entry.Worth = worth;
            entry.Discovered = true;

            if (defeated)
                entry.Defeated = true;

            if (!string.IsNullOrEmpty(note))
                entry.Note = note;
        }

        [Server]
        public void ServerRecord(uint id, string name, int count, int worth, bool defeated, string note)
        {
            Record(id, name, count, worth, defeated, note);
            Publish();
        }

        [Server]
        public void ServerClear()
        {
            _entries.Clear();
            Publish();
        }

        [Server]
        private void Publish()
        {
            int count = _entries.Count;
            var ids = new uint[count];
            var names = new string[count];
            var counts = new int[count];
            var worths = new int[count];
            var flags = new byte[count];
            var notes = new string[count];

            for (int i = 0; i < count; i++)
            {
                CloneJournalEntry entry = _entries[i];
                ids[i] = entry.Id;
                names[i] = entry.Name;
                counts[i] = entry.Count;
                worths[i] = entry.Worth;
                flags[i] = (byte)((entry.Discovered ? 1 : 0) | (entry.Defeated ? 2 : 0));
                notes[i] = entry.Note;
            }

            JournalSnapshotTarget(ids, names, counts, worths, flags, notes);
        }

        [TargetRpc]
        private void JournalSnapshotTarget(uint[] ids, string[] names, int[] counts, int[] worths,
            byte[] flags, string[] notes)
        {
            if (ids == null)
                return;

            _entries.Clear();

            for (int i = 0; i < ids.Length; i++)
            {
                var entry = new CloneJournalEntry
                {
                    Id = ids[i],
                    Name = names != null && i < names.Length ? names[i] : string.Empty,
                    Count = counts != null && i < counts.Length ? counts[i] : 0,
                    Worth = worths != null && i < worths.Length ? worths[i] : 0,
                    Note = notes != null && i < notes.Length ? notes[i] : string.Empty
                };

                if (flags != null && i < flags.Length)
                {
                    entry.Discovered = (flags[i] & 1) != 0;
                    entry.Defeated = (flags[i] & 2) != 0;
                }

                _entries.Add(entry);
            }
        }
    }
}
