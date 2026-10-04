namespace Control.Cat
{
    public class CatAction : ICatAction
    {
        private readonly bool _left;
        private int _pointerId;
        private float _lastX;

        public bool IsDragging { get; private set; }

        public CatAction(bool left)
        {
            _left = left;
        }

        public bool TryBegin(int pointerId, float pointerX, float centerX)
        {
            if (IsDragging || (_left ? pointerX >= centerX : pointerX < centerX))
                return false;

            _pointerId = pointerId;
            _lastX = pointerX;
            IsDragging = true;
            return true;
        }

        public bool TryMove(int pointerId, float pointerX, float currentX,
            float minX, float maxX, out float nextX)
        {
            nextX = currentX;
            if (!IsDragging || pointerId != _pointerId)
                return false;

            var delta = pointerX - _lastX;
            _lastX = pointerX;
            nextX = Clamp(currentX + delta, minX, maxX);
            return true;
        }

        public bool TryEnd(int pointerId)
        {
            if (!IsDragging || pointerId != _pointerId)
                return false;

            Cancel();
            return true;
        }

        public void Cancel()
        {
            IsDragging = false;
            _pointerId = 0;
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
