using UnityEngine;
using Utility;

namespace Game
{
    public class GameStateUI : MonoBehaviour
    {
        [SerializeField] private bool disableWhenHidden;
        [SerializeField] private bool blocksRaycasts;

        private CanvasGroup _group;
        private Component _alpha;

        private void Awake() => EnsureComponents();

        public void Show()
        {
            EnsureComponents();
            gameObject.SetActive(true);
            _alpha.SetAlpha(1f);
            _group.blocksRaycasts = blocksRaycasts;
        }

        public void Hide(bool forceDisable = false)
        {
            EnsureComponents();
            _alpha.SetAlpha(0f);
            _group.blocksRaycasts = false;
            if (forceDisable || disableWhenHidden) gameObject.SetActive(false);
        }

        public float GetAlpha()
        {
            EnsureComponents();
            return _alpha.GetAlpha();
        }

        public void SetAlpha(float alpha)
        {
            EnsureComponents();
            _alpha.SetAlpha(alpha);
        }

        public void SetBlocksRaycasts(bool enabled)
        {
            EnsureComponents();
            _group.blocksRaycasts = enabled;
        }

        private void EnsureComponents()
        {
            if (_group != null) return;
            _group = gameObject.GetOrAdd<CanvasGroup>();
            _alpha = gameObject.FindAlphaComponent();
        }
    }
}
