using System;
using UnityEngine;

namespace Game
{
    public interface IGameFlowSettings
    {
        float StartTransitionSeconds { get; }
        float ResultSeconds { get; }
        float CtaReloadSeconds { get; }
        bool WaitForCatActionsOnWin { get; }
    }

    [Serializable]
    public class GameFlowSettings : IGameFlowSettings
    {
        [Min(0f)] [SerializeField] private float startTransitionSeconds = 2f;
        [Min(0f)] [SerializeField] private float resultSeconds = 3f;
        [Min(0f)] [SerializeField] private float ctaReloadSeconds = 2f;
        [Tooltip("Wait for both cats to finish their current action before showing a winning Result.")]
        [SerializeField] private bool waitForCatActionsOnWin = true;

        public float StartTransitionSeconds => startTransitionSeconds;
        public float ResultSeconds => resultSeconds;
        public float CtaReloadSeconds => ctaReloadSeconds;
        public bool WaitForCatActionsOnWin => waitForCatActionsOnWin;
    }
}
