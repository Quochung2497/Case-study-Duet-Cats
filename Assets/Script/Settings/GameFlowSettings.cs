using System;
using UnityEngine;

namespace Game
{
    public interface IGameFlowSettings
    {
        float StartTransitionSeconds { get; }
        float ResultSeconds { get; }
        float CtaReloadSeconds { get; }
    }

    [Serializable]
    public class GameFlowSettings : IGameFlowSettings
    {
        [Min(0f)] [SerializeField] private float startTransitionSeconds = 2f;
        [Min(0f)] [SerializeField] private float resultSeconds = 3f;
        [Min(0f)] [SerializeField] private float ctaReloadSeconds = 2f;

        public float StartTransitionSeconds => startTransitionSeconds;
        public float ResultSeconds => resultSeconds;
        public float CtaReloadSeconds => ctaReloadSeconds;
    }
}
