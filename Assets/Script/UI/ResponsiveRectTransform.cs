using UnityEngine;
using Utility.DependencyInjection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class ResponsiveRectTransform : MonoBehaviour
    {
        [SerializeField] private UIRectLayout portrait;
        [SerializeField] private UIRectLayout landscape;
        [SerializeField] private bool portraitConfigured;
        [SerializeField] private bool landscapeConfigured;
        [Inject] private IViewport _viewport;

        private RectTransform _rect;
        private bool? _appliedPortrait;

        private void OnEnable()
        {
            _rect ??= GetComponent<RectTransform>();
            _appliedPortrait = null;
#if UNITY_EDITOR
            if (!Application.isPlaying) _viewport = GetComponentInParent<ViewportController>();
#endif
            if (_viewport == null) return;
            _viewport.Changed += ApplyOrientation;
            if (_viewport.HasViewport) ApplyOrientation(_viewport.IsPortrait);
        }

        private void OnDisable()
        {
            if (_viewport != null) _viewport.Changed -= ApplyOrientation;
        }

        private void ApplyOrientation(bool isPortrait)
        {
            // Preserve manual Inspector edits until the orientation actually changes.
            if (_appliedPortrait == isPortrait) return;
            _appliedPortrait = isPortrait;
            if (isPortrait && portraitConfigured)
                Apply(portrait);
            else if (!isPortrait && landscapeConfigured)
                Apply(landscape);
        }

        private void Apply(UIRectLayout layout)
        {
            _rect.anchorMin = layout.anchorMin;
            _rect.anchorMax = layout.anchorMax;
            _rect.pivot = layout.pivot;
            _rect.sizeDelta = layout.sizeDelta;
            _rect.anchoredPosition3D = layout.anchoredPosition;
            _rect.localEulerAngles = layout.rotation;
            _rect.localScale = layout.scale;
        }

        private UIRectLayout Capture()
        {
            _rect ??= GetComponent<RectTransform>();
            return new UIRectLayout
            {
                anchorMin = _rect.anchorMin,
                anchorMax = _rect.anchorMax,
                pivot = _rect.pivot,
                anchoredPosition = _rect.anchoredPosition3D,
                sizeDelta = _rect.sizeDelta,
                rotation = _rect.localEulerAngles,
                scale = _rect.localScale
            };
        }

        [ContextMenu("Capture Portrait Layout")]
        private void CapturePortrait()
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, "Capture Portrait Layout");
#endif
            portrait = Capture();
            portraitConfigured = true;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif
        }

        [ContextMenu("Capture Landscape Layout")]
        private void CaptureLandscape()
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, "Capture Landscape Layout");
#endif
            landscape = Capture();
            landscapeConfigured = true;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif
        }

        [ContextMenu("Apply Current Layout")]
        private void ApplyCurrentLayout()
        {
            _appliedPortrait = null;
#if UNITY_EDITOR
            if (!Application.isPlaying) _viewport = GetComponentInParent<ViewportController>();
#endif
            if (_viewport != null && _viewport.HasViewport)
                ApplyOrientation(_viewport.IsPortrait);
        }
    }
}
