using Control.Note;
using UnityEngine;
using Utility;
using Utility.DependencyInjection;

namespace Game
{
    public class CatHitVfx : MonoBehaviour
    {
        [Inject] private PlayableSettings _settings;

        private CatBehaviour _cat;
        private ParticleSystem _particles;

        private void Awake()
        {
            _cat = GetComponentInParent<CatBehaviour>();
            _particles = gameObject.GetOrAdd<ParticleSystem>();
            var main = _particles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            if (_cat == null)
                Debug.LogError("CatHitVfx needs a CatBehaviour in its parents.", this);
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
            var main = _particles.main;
            main.startLifetime = vfx.lifetime;
            main.startSize = vfx.size;
            main.startColor = Color.white;

            _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _particles.Play(true);

            for (var i = 0; i < vfx.count; i++)
            {
                var t = vfx.count == 1 ? 0.5f : i / (float)(vfx.count - 1);
                var tilt = _cat.IsLeft ? -_settings.HitVfxTiltDegrees : _settings.HitVfxTiltDegrees;
                var angle = tilt + (t - 0.5f) * _settings.HitVfxSpreadDegrees
                    + Random.Range(-_settings.HitVfxAngleJitterDegrees, _settings.HitVfxAngleJitterDegrees);
                var speed = vfx.speed * Random.Range(
                    1f - _settings.HitVfxSpeedVariance, 1f + _settings.HitVfxSpeedVariance);
                var particle = new ParticleSystem.EmitParams
                {
                    position = transform.position,
                    velocity = Quaternion.Euler(0f, 0f, angle) * Vector3.up * speed,
                    startColor = _settings.GetHitVfxColor(type, _cat.IsLeft, t)
                };
                _particles.Emit(particle, 1);
            }
        }
    }
}