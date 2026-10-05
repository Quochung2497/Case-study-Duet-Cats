using System;
using System.Collections.Generic;
using UnityEngine;

namespace Control.Note
{
    public static class NoteChart
    {
        [Serializable]
        private class ChartFile
        {
            public ChartEntry[] notes;
        }

        [Serializable]
        private class ChartEntry
        {
            public int id;
            public float spawnTime;
            public int lane;
            public string visualType;
        }

        public static NoteEvent[] Parse(string json)
        {
            var chart = JsonUtility.FromJson<ChartFile>(json);
            if (chart?.notes == null || chart.notes.Length == 0)
                throw new FormatException("The note chart has no notes.");

            var notes = new NoteEvent[chart.notes.Length];
            var ids = new HashSet<int>();

            for (var i = 0; i < notes.Length; i++)
            {
                var entry = chart.notes[i];
                if (entry == null || entry.id <= 0 || !ids.Add(entry.id) ||
                    entry.lane < 0 || entry.lane > 3 ||
                    float.IsNaN(entry.spawnTime) || float.IsInfinity(entry.spawnTime) ||
                    entry.spawnTime < 0f ||
                    !Enum.TryParse(entry.visualType, true, out NoteVisualType visualType) ||
                    !Enum.IsDefined(typeof(NoteVisualType), visualType))
                    throw new FormatException($"Invalid note at chart index {i}.");

                notes[i] = new NoteEvent(entry.id, entry.spawnTime, entry.lane, visualType);
            }

            Array.Sort(notes, (a, b) =>
            {
                var time = a.SpawnTime.CompareTo(b.SpawnTime);
                return time != 0 ? time : a.Id.CompareTo(b.Id);
            });
            return notes;
        }
    }
}
