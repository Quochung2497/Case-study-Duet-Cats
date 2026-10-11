using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility.DependencyInjection;

namespace Game
{
    public class GameUIController : MonoBehaviour
    {
        [Serializable]
        private class PanelBinding
        {
            public GameStateUI view;
            public List<GameState> visibleInStates = new();
            [Min(0f)] public float showDelaySeconds;
            public bool fadeOnStartTransition;

            [NonSerialized] public Coroutine delayedShow;
            [NonSerialized] public Coroutine fade;
            [NonSerialized] public bool wantedVisible;
            [NonSerialized] public bool fading;

            public bool IsVisibleIn(GameState state) => visibleInStates != null && visibleInStates.Contains(state);
        }

        [SerializeField] private List<PanelBinding> panels = new();

        [Inject] private IGameState _game;
        [Inject] private IGameFlowSettings _settings;

        private void Start()
        {
            _game.StateChanged += ApplyState;
            _game.StartTransitionBegan += FadeStartPanels;
            ApplyState(_game.State);
        }

        private void ApplyState(GameState state)
        {
            foreach (var panel in panels)
            {
                if (panel?.view == null) continue;

                var visible = panel.IsVisibleIn(state);
                // A panel shared by Playing and Result keeps its current delay or visibility.
                if (visible && panel.wantedVisible && !panel.fading) continue;

                Cancel(panel);
                panel.wantedVisible = visible;
                panel.fading = false;

                if (!visible)
                {
                    panel.view.Hide();
                }
                else if (panel.showDelaySeconds <= 0f)
                {
                    panel.view.Show();
                }
                else
                {
                    // Run on this always-active controller while the panel stays inactive.
                    panel.view.Hide(true);
                    panel.delayedShow = StartCoroutine(ShowAfterDelay(panel));
                }
            }
        }

        private IEnumerator ShowAfterDelay(PanelBinding panel)
        {
            yield return new WaitForSecondsRealtime(panel.showDelaySeconds);
            panel.delayedShow = null;
            if (panel.wantedVisible && panel.IsVisibleIn(_game.State)) panel.view.Show();
        }

        private void FadeStartPanels()
        {
            foreach (var panel in panels)
            {
                if (panel?.view == null || !panel.fadeOnStartTransition || !panel.wantedVisible ||
                    !panel.view.gameObject.activeInHierarchy) continue;

                panel.view.SetBlocksRaycasts(false);
                if (panel.fade != null) StopCoroutine(panel.fade);
                panel.fading = true;
                panel.fade = StartCoroutine(FadeOut(panel));
            }
        }

        private IEnumerator FadeOut(PanelBinding panel)
        {
            var seconds = _settings.StartTransitionSeconds;
            var from = panel.view.GetAlpha();
            var elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                panel.view.SetAlpha(Mathf.Lerp(from, 0f, elapsed / seconds));
                yield return null;
            }

            panel.view.Hide();
            panel.fade = null;
        }

        private void Cancel(PanelBinding panel)
        {
            if (panel.delayedShow != null) StopCoroutine(panel.delayedShow);
            if (panel.fade != null) StopCoroutine(panel.fade);
            panel.delayedShow = null;
            panel.fade = null;
        }

        private void OnDestroy()
        {
            if (_game == null) return;
            _game.StateChanged -= ApplyState;
            _game.StartTransitionBegan -= FadeStartPanels;
        }
    }
}
