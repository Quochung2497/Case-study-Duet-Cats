using System;
using Input;
using UnityEngine;

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
        private NoteManager _notes;
        private CatBehaviour _leftCat;
        private CatBehaviour _rightCat;
        private IInputReader _input;
        private ScoreManager _score;
        private PlayableSettings _settings;
        private float _ctaAt;

        public GameState State { get; private set; } = GameState.Start;
        public bool Won { get; private set; }
        public event Action<GameState> StateChanged;

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
        private void Update() => UpdateResult();

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

        private void OnPointerDown(PointerSample _) => StartGame();
        private void OnNoteMissed() => EndGame(false);
        private void OnSongFinished() => EndGame(true);

        public void StartGame()
        {
            if (State != GameState.Start || !_notes.StartSong()) return;

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
            _leftCat.StopPlaying();
            _rightCat.StopPlaying();
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
