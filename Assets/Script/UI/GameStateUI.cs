using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using Utility.DependencyInjection;

namespace Game
{
    public class GameStateUI : MonoBehaviour
    {
        // The Inspector state list lets one script control tutorial, status, result, and CTA UI.
        [SerializeField, InspectorName("Visible In")]
        private List<GameState> visibleInStates = new() { GameState.Start };
        [Min(0f)] [SerializeField] private float showDelaySeconds;
        [SerializeField] private bool disableWhenHidden;
        [SerializeField] private bool blocksRaycasts;

        [Inject] private GameManager _game;
        [Inject] private IGameFlowSettings _settings;

        private CanvasGroup _group;
        private Component _alpha;
        private Coroutine _fade;
        private Coroutine _delayedShow;
        private bool _wantedVisible;

        private void Awake()
        {
            _group = gameObject.GetOrAdd<CanvasGroup>();
            _alpha = gameObject.FindAlphaComponent();
        }

        private void Start()
        {
            // Sync the UI to the current state immediately instead of waiting for another event.
            _game.StateChanged += Show;
            _game.StartTransitionBegan += FadeStart;
            Show(_game.State);
        }

        private void Show(GameState state)
        {
            // Cancel the old fade or delay so the newest state controls visibility.
            var visible = IsVisibleIn(state);
            if (visible && _wantedVisible && _fade == null) return;
            _wantedVisible = visible;

            if (_fade != null) StopCoroutine(_fade);
            _fade = null;
            if (_delayedShow != null) _game.StopCoroutine(_delayedShow);
            _delayedShow = null;

            if (!visible)
            {
                Hide();
                return;
            }

            if (showDelaySeconds <= 0f)
            {
                Reveal();
                return;
            }

            // Run the delay on GameManager so this UI object may stay inactive while waiting.
            Hide(true);
            _delayedShow = _game.StartCoroutine(ShowAfterDelay());
        }

        private bool IsVisibleIn(GameState state)
        {
            return visibleInStates != null && visibleInStates.Contains(state);
        }

        private void Reveal()
        {
            // Update alpha and raycasts together so hidden UI does not block input.
            gameObject.SetActive(true);
            _alpha.SetAlpha(1f);
            _group.blocksRaycasts = blocksRaycasts;
        }

        private void Hide(bool forceDisable = false)
        {
            _alpha.SetAlpha(0f);
            _group.blocksRaycasts = false;
            if (forceDisable || disableWhenHidden) gameObject.SetActive(false);
        }

        private IEnumerator ShowAfterDelay()
        {
            yield return new WaitForSecondsRealtime(showDelaySeconds);
            _delayedShow = null;
            Reveal();
        }

        private void FadeStart()
        {
            // Only UI shown in Start fades when the player begins a run.
            if (!IsVisibleIn(GameState.Start) || !gameObject.activeInHierarchy) return;
            _group.blocksRaycasts = false;
            if (_fade != null) StopCoroutine(_fade);
            _fade = StartCoroutine(FadeOut());
        }

        private IEnumerator FadeOut()
        {
            // Unscaled time keeps the transition duration stable if timeScale changes.
            var seconds = _settings.StartTransitionSeconds;
            var from = _alpha.GetAlpha();
            var elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                _alpha.SetAlpha(Mathf.Lerp(from, 0f, elapsed / seconds));
                yield return null;
            }

            _alpha.SetAlpha(0f);
            _fade = null;
            if (disableWhenHidden) gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_game == null) return;
            if (_delayedShow != null) _game.StopCoroutine(_delayedShow);
            _game.StateChanged -= Show;
            _game.StartTransitionBegan -= FadeStart;
        }
    }
}
