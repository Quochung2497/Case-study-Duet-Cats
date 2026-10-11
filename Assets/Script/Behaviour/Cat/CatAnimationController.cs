using System;
using Control.Cat;
using Control.Note;
using Spine.Unity;

namespace Game
{
    public class CatAnimationController : ICatAnimation
    {
        private readonly SkeletonAnimation _spine;
        private readonly CatAnimationSet _animations;
        private int _animVersion;
        private int _lastIdle = -1;

        public CatAnimationController(SkeletonAnimation spine, CatAnimationSet animations)
        {
            _spine = spine ?? throw new ArgumentNullException(nameof(spine));
            _animations = animations ?? throw new ArgumentNullException(nameof(animations));
        }

        public void PlayIdle(Action onComplete) => Play(CatAnimationCue.Idle, onComplete);

        public void PlayPlaying() => Play(CatAnimationCue.Playing);

        public void PlayHit(NoteVisualType type, Action onComplete) =>
            Play(type == NoteVisualType.Normal ? CatAnimationCue.HitNormal : CatAnimationCue.HitOther,
                onComplete);

        public void PlayResult(bool won) => Play(won ? CatAnimationCue.Win : CatAnimationCue.Lose);

        private void Play(CatAnimationCue cue, Action onComplete = null)
        {
            // Each cat owns its own selection history, even when both share one asset.
            var selection = _animations.Choose(cue, cue == CatAnimationCue.Idle ? _lastIdle : -1);
            if (cue == CatAnimationCue.Idle) _lastIdle = selection.Index;

            // Ignore a completion callback after another animation replaces this one.
            var version = ++_animVersion;
            var entry = _spine.AnimationState.SetAnimation(0, selection.Name, selection.Loop);
            if (onComplete != null)
                entry.Complete += _ =>
                {
                    if (version == _animVersion) onComplete();
                };
        }
    }
}
