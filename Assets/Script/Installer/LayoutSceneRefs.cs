using UnityEngine;

namespace Game
{
    // Bundle scene references commonly used together into one dependency.
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
