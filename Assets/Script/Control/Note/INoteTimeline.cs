namespace Control.Note
{
    public interface INoteTimeline
    {
        float TravelSeconds { get; }
        bool IsDone { get; }
        void Reset();
        bool TryTakeDue(double songTime, out NoteEvent note);
        float GetY(float spawnY, float hitY, double songTime, float spawnTime);
    }
}
