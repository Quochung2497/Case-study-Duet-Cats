using TMPro;
using UnityEngine;
using Utility.DependencyInjection;

namespace Game
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private string loseParameterName = "Lose";
        [SerializeField] private bool loseParameterValue = true;
        [Inject] private IGameState _game;
        [Inject] private IScoreManager _score;

        private Animator _statusAnimator;

        private void Awake()
        {
            _statusAnimator = GetComponentInParent<Animator>();
            if (scoreText == null)
                Debug.LogError("ScoreUI needs the numeric TMP label.", this);
        }

        private void OnEnable()
        {
            // On enable, display the current score before waiting for later changes.
            _score.ScoreChanged += Refresh;
            _game.StateChanged += OnGameStateChanged;
            Refresh();
            OnGameStateChanged(_game.State);
        }

        private void OnDisable()
        {
            if (_score != null) _score.ScoreChanged -= Refresh;
            if (_game != null) _game.StateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState state)
        {
            // Set the configured Animator parameter only for a losing Result.
            if (state == GameState.Result && !_game.Won && _statusAnimator != null)
                _statusAnimator.SetBool(loseParameterName, loseParameterValue);
        }

        private void Refresh()
        {
            if (scoreText != null) scoreText.text = _score.Score.ToString();
        }
    }
}
