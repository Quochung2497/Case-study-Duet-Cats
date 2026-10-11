using System;
using System.Collections.Generic;
using Spine;
using UnityEngine;

namespace Game
{
    public enum CatAnimationCue
    {
        Idle,
        Playing,
        HitNormal,
        HitOther,
        Win,
        Lose
    }

    [Serializable]
    public class CatAnimationGroup
    {
        [SerializeField] private CatAnimationCue cue;
        [SerializeField] private bool loop;
        [SerializeField] private string[] clips;

        public CatAnimationCue Cue => cue;
        public bool Loop => loop;
        public string[] Clips => clips;
    }

    public readonly struct CatAnimationSelection
    {
        public readonly string Name;
        public readonly bool Loop;
        public readonly int Index;

        public CatAnimationSelection(string name, bool loop, int index)
        {
            Name = name;
            Loop = loop;
            Index = index;
        }
    }

    [CreateAssetMenu(fileName = "CatAnimationSet", menuName = "Playable/Cat Animation Set")]
    public class CatAnimationSet : ScriptableObject
    {
        [SerializeField] private CatAnimationGroup[] groups;

        public CatAnimationSelection Choose(CatAnimationCue cue, int previousIndex = -1)
        {
            // The asset stores clip choices, while each cat keeps its own previous idle index.
            if (groups != null)
                foreach (var group in groups)
                {
                    if (group == null || group.Cue != cue) continue;
                    var clips = group.Clips;
                    if (clips == null || clips.Length == 0) break;

                    var index = UnityEngine.Random.Range(0, clips.Length);
                    if (clips.Length > 1 && index == previousIndex)
                        index = (index + 1) % clips.Length;
                    return new CatAnimationSelection(clips[index], group.Loop, index);
                }

            throw new InvalidOperationException($"{name} has no clips for {cue}.");
        }

        public bool TryValidate(SkeletonData skeletonData, out string error)
        {
            if (skeletonData == null)
            {
                error = "Skeleton data is missing.";
                return false;
            }

            var seen = new HashSet<CatAnimationCue>();
            if (groups != null)
                foreach (var group in groups)
                {
                    if (group == null || !Enum.IsDefined(typeof(CatAnimationCue), group.Cue) ||
                        !seen.Add(group.Cue))
                    {
                        error = "Animation groups contain a null, unknown, or duplicate cue.";
                        return false;
                    }

                    if (group.Clips == null || group.Clips.Length == 0)
                    {
                        error = $"{group.Cue} has no clips.";
                        return false;
                    }

                    foreach (var clip in group.Clips)
                    {
                        if (string.IsNullOrWhiteSpace(clip) || skeletonData.FindAnimation(clip) == null)
                        {
                            error = $"{group.Cue} references missing Spine animation '{clip}'.";
                            return false;
                        }
                    }
                }

            foreach (var cue in (CatAnimationCue[])Enum.GetValues(typeof(CatAnimationCue)))
                if (!seen.Contains(cue))
                {
                    error = $"Animation group {cue} is missing.";
                    return false;
                }

            error = null;
            return true;
        }
    }
}
