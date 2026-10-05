using System;
using System.Collections;
using Input;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public enum GameState
    {
        Start,
        Playing,
        Result,
        Cta
    }

    public class GameManager : MonoBehaviour
    {
#if UNITY_EDITOR
        [Header("Debug")]
        [Tooltip("Ignore all misses so the song can finish with a win in the Editor.")]
        [SerializeField] private bool ignoreMisses;
#endif

        private NoteManager _notes;
        private CatBehaviour _leftCat;
        private CatBehaviour _rightCat;
        private IInputReader _input;
        private ScoreManager _score;
        private PlayableSettings _settings;
        private float _ctaAt;
        private float _startAt;
        private bool _starting;
        private bool _reloading;

        public GameState State { get; private set; } = GameState.Start;
        public bool Won { get; private set; }
        public event Action<GameState> StateChanged;
        public event Action StartTransitionBegan;

        public void Initialize(NoteManager notes, CatBehaviour leftCat, CatBehaviour rightCat,
            IInputReader input, PlayableSettings settings)
        {
            _notes = notes;
            _leftCat = leftCat;
            _rightCat = rightCat;
            _input = input;
            _settings = settings;
            _score = ScoreManager.Instance;

            _input.PointerDown += OnPointerDown;
            _notes.NoteMissed += OnNoteMissed;
            _notes.SongFinished += OnSongFinished;
        }

        private void Start() => StateChanged?.Invoke(State);
        private void Update()
        {
            UpdateStart();
            UpdateResult();
        }

        private void UpdateStart()
        {
            if (!_starting || Time.unscaledTime < _startAt) return;
            BeginPlaying();
        }

        private void UpdateResult()
        {
            if (State == GameState.Result && Time.unscaledTime >= _ctaAt)
                ShowCta();
        }

        private void OnDestroy()
        {
            if (_input != null) _input.PointerDown -= OnPointerDown;
            if (_notes == null) return;
            _notes.NoteMissed -= OnNoteMissed;
            _notes.SongFinished -= OnSongFinished;
        }

        private void OnPointerDown(PointerSample _)
        {
            if (State == GameState.Start)
                StartGame();
            else if (State == GameState.Cta && !_reloading)
            {
                _reloading = true;
                StartCoroutine(ReloadSceneAfterDelay());
            }
        }

        private IEnumerator ReloadSceneAfterDelay()
        {
            yield return new WaitForSecondsRealtime(_settings.CtaReloadSeconds);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnNoteMissed()
        {
#if UNITY_EDITOR
            if (ignoreMisses) return;
#endif
            EndGame(false);
        }
        private void OnSongFinished() => EndGame(true);

        public void StartGame()
        {
            if (State != GameState.Start || _starting) return;
            _starting = true;
            _startAt = Time.unscaledTime + _settings.StartTransitionSeconds;
            StartTransitionBegan?.Invoke();
        }

        private void BeginPlaying()
        {
            _starting = false;
            if (!_notes.StartSong())
            {
                StateChanged?.Invoke(State);
                return;
            }

            _score.ResetScore();
            State = GameState.Playing;
            _leftCat.StartPlaying();
            _rightCat.StartPlaying();
            StateChanged?.Invoke(State);
        }

        private void EndGame(bool won)
        {
            if (State != GameState.Playing) return;

            Won = won;
            State = GameState.Result;
            _ctaAt = Time.unscaledTime + _settings.ResultSeconds;
            _notes.StopSong();
            _leftCat.ShowResult(won);
            _rightCat.ShowResult(won);
            StateChanged?.Invoke(State);
        }

        public void ShowCta()
        {
            if (State != GameState.Result) return;

            State = GameState.Cta;
            _leftCat.gameObject.SetActive(false);
            _rightCat.gameObject.SetActive(false);
            StateChanged?.Invoke(State);
        }
    }
}
