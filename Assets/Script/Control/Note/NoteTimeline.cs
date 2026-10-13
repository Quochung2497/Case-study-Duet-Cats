namespace Control.Note
{
    public class NoteTimeline : INoteTimeline
    {
        // _next points to the first untaken note in the sorted chart.
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

        public void Reset() => _next = 0;

        public bool TryTakeDue(double songTime, out NoteEvent note)
        {
            // Take one note per call; NoteManager loops to take every note due this tick.
            if (_next < _notes.Length &&
                songTime >= _notes[_next].SpawnTime)
            {
                note = _notes[_next++];
                return true;
            }

            note = default;
            return false;
        }

        public float GetY(float spawnY, float hitY, double songTime, float spawnTime)
        {
            // Progress is 0 at spawn and 1 after this timeline's travel time.
            var progress = (songTime - spawnTime) / _travelSeconds;
            return spawnY + (hitY - spawnY) * (float)progress;
        }
    }
}
