using Control.Note;

namespace Game
{
    // The scoring system depends on this rule, not on the entire playable configuration.
    public interface IScoreRules
    {
        int GetPoints(NoteVisualType type);
    }
}
