namespace Control.Note
{
    public enum NoteVisualType
    {
        Normal,
        Strong,
        Long,
        LolipopLong
    }

    // A parsed note is immutable data shared by the timeline, visuals, and scoring.
    public readonly struct NoteEvent
    {
        public int Id { get; }
        public float SpawnTime { get; }
        public int Lane { get; }
        public NoteVisualType VisualType { get; }

        public NoteEvent(int id, float spawnTime, int lane, NoteVisualType visualType)
        {
            Id = id;
            SpawnTime = spawnTime;
            Lane = lane;
            VisualType = visualType;
        }
    }
}
