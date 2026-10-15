using Control.Note;
using UnityEngine;
using Utility;
using Utility.DependencyInjection;

namespace Game
{
    public class CatHitVfx : MonoBehaviour
    {
        [Inject] private HitVfxSettings _settings;

        private ICatBehaviour _cat;
        private ParticleSystem _particles;

        private void Awake()
        {
            // The effect sits under MouthAnchor, so find the cat contract in a parent.
            _cat = GetComponentInParent<ICatBehaviour>();
            _particles = gameObject.GetOrAdd<ParticleSystem>();
            var main = _particles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            if (_cat == null)
                Debug.LogError("CatHitVfx needs an ICatBehaviour in its parents.", this);
        }

        private void OnEnable()
        {
            if (_cat != null) _cat.NoteHit += Play;
        }

        private void OnDisable()
        {
            if (_cat != null) _cat.NoteHit -= Play;
        }

        private void Play(NoteVisualType type)
        {
            if (_settings == null || _particles == null || _cat == null) return;

            var vfx = _settings.GetHitVfx(type);

            // Clear the previous hit before restarting the prefab's configured burst.
            _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = _particles.main;
            main.startLifetime = vfx.lifetime;
            main.startSize = vfx.size;
            main.startSpeed = vfx.speed;
            main.startColor = _settings.GetHitVfxStartColor(type, _cat.IsLeft);

            // Shape and direction are authored in the prefab; only the note's count changes here.
            var emission = _particles.emission;
            var burst = emission.GetBurst(0);
            burst.count = new ParticleSystem.MinMaxCurve(vfx.count);
            emission.SetBurst(0, burst);

            _particles.Play(true);
        }
    }
}
