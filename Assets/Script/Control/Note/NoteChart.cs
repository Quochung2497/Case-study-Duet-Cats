using System;
using System.Collections.Generic;
using UnityEngine;

namespace Control.Note
{
    public static class NoteChart
    {
        // These classes mirror the JSON shape so JsonUtility can deserialize the chart.
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
            // Reject an empty chart because it contains no playable notes.
            var chart = JsonUtility.FromJson<ChartFile>(json);
            if (chart?.notes == null || chart.notes.Length == 0)
                throw new FormatException("The note chart has no notes.");

            var notes = new NoteEvent[chart.notes.Length];
            // HashSet.Add returns false for an existing ID, detecting duplicate notes.
            var ids = new HashSet<int>();

            for (var i = 0; i < notes.Length; i++)
            {
                var entry = chart.notes[i];
                // Reject missing/duplicate IDs, lanes outside 0-3, invalid or negative times,
                // and unknown visual types. TryParse ignores case; IsDefined rejects numeric enum values.
                if (entry == null || entry.id <= 0 || !ids.Add(entry.id) ||
                    entry.lane < 0 || entry.lane > 3 ||
                    float.IsNaN(entry.spawnTime) || float.IsInfinity(entry.spawnTime) ||
                    entry.spawnTime < 0f ||
                    !Enum.TryParse<NoteVisualType>(entry.visualType, true, out var visualType) ||
                    !Enum.IsDefined(typeof(NoteVisualType), visualType))
                    throw new FormatException($"Invalid note at chart index {i}.");

                notes[i] = new NoteEvent(entry.id, entry.spawnTime, entry.lane, visualType);
            }

            // Sort by spawn time; break ties by placing the smaller ID first.
            // CompareTo returns negative, zero, or positive as Array.Sort expects.
            Array.Sort(notes, (a, b) =>
            {
                var time = a.SpawnTime.CompareTo(b.SpawnTime);
                return time != 0 ? time : a.Id.CompareTo(b.Id);
            });
            return notes;
        }
    }
}
