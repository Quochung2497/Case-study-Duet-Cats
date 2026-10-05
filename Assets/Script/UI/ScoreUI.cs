using TMPro;
using UnityEngine;

namespace Game
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        private ScoreManager _score;

        private void Awake()
        {
            if (scoreText == null)
                Debug.LogError("ScoreUI needs the numeric TMP label.", this);
        }

        private void OnEnable()
        {
            _score = ScoreManager.Instance;
            _score.ScoreChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (_score != null) _score.ScoreChanged -= Refresh;
        }

        private void Refresh()
        {
            if (scoreText != null) scoreText.text = _score.Score.ToString();
        }
    }
}
