using System;

namespace Game
{
    // World-space coordinates shared by cat movement and note placement.
    public readonly struct LayoutData
    {
        public readonly float PlayLeft, PlayRight, LeftMinX, LeftMaxX, RightMinX, RightMaxX;
        public readonly float LeftCatX, RightCatX, CatY, HitY, SpawnY;
        public readonly float Lane0X, Lane1X, Lane2X, Lane3X;

        public float PlayWidth => PlayRight - PlayLeft;

        public LayoutData(float playLeft, float playRight,
            float leftMinX, float leftMaxX, float rightMinX, float rightMaxX,
            float leftCatX, float rightCatX, float catY, float hitY, float spawnY,
            float lane0X, float lane1X, float lane2X, float lane3X)
        {
            PlayLeft = playLeft;
            PlayRight = playRight;
            LeftMinX = leftMinX;
            LeftMaxX = leftMaxX;
            RightMinX = rightMinX;
            RightMaxX = rightMaxX;
            LeftCatX = leftCatX;
            RightCatX = rightCatX;
            CatY = catY;
            HitY = hitY;
            SpawnY = spawnY;
            Lane0X = lane0X;
            Lane1X = lane1X;
            Lane2X = lane2X;
            Lane3X = lane3X;
        }

        public float LaneX(int lane)
        {
            switch (lane)
            {
                case 0: return Lane0X;
                case 1: return Lane1X;
                case 2: return Lane2X;
                case 3: return Lane3X;
                default: throw new ArgumentOutOfRangeException(nameof(lane));
            }
        }

        public float ClampCatX(bool left, float x)
        {
            var min = left ? LeftMinX : RightMinX;
            var max = left ? LeftMaxX : RightMaxX;
            return Math.Max(min, Math.Min(max, x));
        }

        public bool SameAs(LayoutData other)
        {
            return PlayLeft == other.PlayLeft && PlayRight == other.PlayRight &&
                   LeftMinX == other.LeftMinX && LeftMaxX == other.LeftMaxX &&
                   RightMinX == other.RightMinX && RightMaxX == other.RightMaxX &&
                   LeftCatX == other.LeftCatX && RightCatX == other.RightCatX &&
                   CatY == other.CatY && HitY == other.HitY && SpawnY == other.SpawnY &&
                   Lane0X == other.Lane0X && Lane1X == other.Lane1X &&
                   Lane2X == other.Lane2X && Lane3X == other.Lane3X;
        }
    }
}
