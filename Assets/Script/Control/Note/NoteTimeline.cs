namespace Control.Note
{
    public class NoteTimeline
    {
        private readonly NoteEvent[] _notes;
        private readonly float _travelSeconds;
        private int _next;

        public float TravelSeconds => _travelSeconds;
        public bool IsDone => _next >= _notes.Length;

        public NoteTimeline(NoteEvent[] notes, float travelSeconds)
        {
            _notes = notes;
            _travelSeconds = travelSeconds;
        }

        public bool TryTakeDue(double songTime, out NoteEvent note)
        {
            if (_next < _notes.Length &&
                songTime >= _notes[_next].SpawnTime)
            {
                note = _notes[_next++];
                return true;
            }

            note = default;
            return false;
        }

        public static float GetY(float spawnY, float hitY, double songTime,
            float spawnTime, float travelSeconds)
        {
            var progress = (songTime - spawnTime) / travelSeconds;
            return spawnY + (hitY - spawnY) * (float)progress;
        }
    }
}
