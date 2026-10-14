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
        [Header("Portrait baseline")]
        [SerializeField] private UIRectLayout portrait;
        [SerializeField] private bool portraitConfigured;
        [SerializeField] private UIRectLayoutRules rules = new();

        [Inject] private IViewport _viewport;

        private RectTransform _rect;
        private RectTransform _parent;
        private RectTransform _canvasRect;
        private readonly Vector3[] _canvasCorners = new Vector3[4];

        private void OnEnable()
        {
            Initialize();
            if (_viewport == null) return;
            _viewport.Changed += OnViewportChanged;
            // A newly enabled group can miss the previous event, so apply the current geometry once.
            if (_viewport.HasViewport) ApplyLayout();
        }

        private void Initialize()
        {
            // Cache scene references once. Runtime receives IViewport through injection.
            _rect ??= GetComponent<RectTransform>();
            _parent ??= _rect.parent as RectTransform;
            if (_canvasRect == null)
            {
                var canvas = GetComponentInParent<Canvas>();
                if (canvas != null) _canvasRect = canvas.rootCanvas.transform as RectTransform;
            }
#if UNITY_EDITOR
            // Injection does not run during Inspector preview; use the Canvas controller in edit mode.
            if (!Application.isPlaying) _viewport = GetComponentInParent<ViewportController>();
#endif
        }

        private void OnDisable()
        {
            if (_viewport != null) _viewport.Changed -= OnViewportChanged;
#if UNITY_EDITOR
            EditorApplication.delayCall -= ApplyCurrentLayout;
#endif
        }

        private void OnViewportChanged(bool _) => ApplyLayout();

        private void ApplyLayout()
        {
            if (!TryGetGeometry(out var parentRect, out var viewportRect)) return;
            // ViewportController filters changes; this component only reads, calculates and applies.
            var layout = LayoutMath.CalculateUI(portrait, rules, parentRect, viewportRect, _viewport.IsPortrait);
            Apply(layout);
        }

        private bool TryGetGeometry(out Rect parentRect, out Rect viewportRect)
        {
            parentRect = default;
            viewportRect = default;
            if (!portraitConfigured || _parent == null || _canvasRect == null ||
                _viewport == null || !_viewport.HasViewport ||
                rules.referenceParentSize.x <= 0f || rules.referenceParentSize.y <= 0f)
                return false;

            parentRect = _parent.rect;
            if (parentRect.width <= 0f || parentRect.height <= 0f) return false;
            // Read Unity geometry here; the helper converts it into the parent's local UI units.
            _canvasRect.GetWorldCorners(_canvasCorners);
            viewportRect = LayoutMath.GetRectInLocalSpace(_canvasCorners, _parent.worldToLocalMatrix);
            return viewportRect.width > 0f && viewportRect.height > 0f;
        }

        private void Apply(UIRectLayout layout)
        {
            // Write only this group. Children retain their authored layout and animation bindings.
            _rect.anchorMin = layout.anchorMin;
            _rect.anchorMax = layout.anchorMax;
            _rect.pivot = layout.pivot;
            _rect.sizeDelta = layout.sizeDelta;
            _rect.anchoredPosition3D = layout.anchoredPosition;
            _rect.localEulerAngles = layout.rotation;
            _rect.localScale = layout.scale;
        }

#if UNITY_EDITOR
        // Authoring tools save the portrait baseline; the runtime path above only reads it.
        private UIRectLayout Capture()
        {
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
            Initialize();
            if (_parent == null || Application.isPlaying || _viewport == null || !_viewport.IsPortrait)
                return;
            Undo.RecordObject(this, "Capture Portrait Layout");
            portrait = Capture();
            portraitConfigured = true;
            rules.referenceParentSize = _parent.rect.size;
            EditorUtility.SetDirty(this);
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
        }

        [ContextMenu("Apply Current Layout")]
        private void ApplyCurrentLayout()
        {
            if (this == null || !isActiveAndEnabled) return;
            Initialize();
            ApplyLayout();
        }

        private void OnValidate()
        {
            // Inspector edits do not change the viewport; schedule one Editor preview instead.
            if (Application.isPlaying) return;
            EditorApplication.delayCall -= ApplyCurrentLayout;
            EditorApplication.delayCall += ApplyCurrentLayout;
        }
#endif
    }
}
