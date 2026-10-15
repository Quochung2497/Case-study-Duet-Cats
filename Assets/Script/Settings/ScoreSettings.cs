using System;
using Control.Note;
using UnityEngine;

namespace Game
{
    [Serializable]
    public class ScoreSettings : IScoreRules
    {
        [Min(0)] [SerializeField] private int normalPoints = 2;
        [Min(0)] [SerializeField] private int strongPoints = 5;
        [Min(0)] [SerializeField] private int longPoints = 10;
        [Min(0)] [SerializeField] private int lolipopLongPoints = 10;

        public int GetPoints(NoteVisualType type)
        {
            // Keep note-type point selection with the values it reads.
            switch (type)
            {
                case NoteVisualType.Strong: return strongPoints;
                case NoteVisualType.Long: return longPoints;
                case NoteVisualType.LolipopLong: return lolipopLongPoints;
                default: return normalPoints;
            }
        }
    }
}
