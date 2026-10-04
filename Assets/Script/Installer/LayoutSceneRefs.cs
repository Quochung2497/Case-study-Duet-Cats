using UnityEngine;

namespace Game
{
    public class LayoutSceneRefs
    {
        public Camera Camera { get; }
        public Transform LeftCat { get; }
        public Transform RightCat { get; }

        public LayoutSceneRefs(Camera camera, Transform leftCat, Transform rightCat)
        {
            Camera = camera;
            LeftCat = leftCat;
            RightCat = rightCat;
        }
    }
}
