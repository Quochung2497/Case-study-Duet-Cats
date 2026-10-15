namespace Control.Cat
{
    // Keep drag rules outside MonoBehaviour, transforms, and the Input System.
    public interface ICatAction
    {
        bool IsDragging { get; }
        bool TryBegin(int pointerId, float pointerX, float centerX);
        bool TryMove(int pointerId, float pointerX, float currentX,
            float minX, float maxX, out float nextX);
        bool TryEnd(int pointerId);
        void Cancel();
    }
}
