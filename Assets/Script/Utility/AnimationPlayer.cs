using System;
using Spine;
using Spine.Unity;

namespace Utility
{
    public enum CatClip
    {
        IdleStart,
        IdlePlaying,
        IdleHungry,
        IdleLiemchan,
        IdleYawn,
        Listening,
        Eating,
        EatingSingle1,
        EatingSingle2,
        EatShot,
        EatLongBegin,
        EatLongLoop,
        EatLongEnd,
        MissAppease,
        MissObject,
        MissObjectLose,
        MissObjectLose2,
        Victory,
        Victory2,
        Tail
    }

    public static class AnimationPlayer
    {
        public static TrackEntry Play(SkeletonAnimation cat, CatClip clip, bool loop = false)
        {
            if (cat == null) throw new ArgumentNullException(nameof(cat));
            return cat.AnimationState.SetAnimation(0, Name(clip), loop);
        }

        public static string Name(CatClip clip)
        {
            switch (clip)
            {
                case CatClip.IdleStart: return "Idle_Start";
                case CatClip.IdlePlaying: return "Idle_Playing";
                case CatClip.IdleHungry: return "Idle_Hungry";
                case CatClip.IdleLiemchan: return "Idle_Liemchan";
                case CatClip.IdleYawn: return "Idle_Yawn";
                case CatClip.Listening: return "Listening";
                case CatClip.Eating: return "Eating";
                case CatClip.EatingSingle1: return "Eating_Single_Object";
                case CatClip.EatingSingle2: return "Eating_Single_Object_2";
                case CatClip.EatShot: return "Eat_Shot";
                case CatClip.EatLongBegin: return "Eat_Long_Begin";
                case CatClip.EatLongLoop: return "Eat_Long_Loop";
                case CatClip.EatLongEnd: return "Eat_Long_End";
                case CatClip.MissAppease: return "Miss_Appease";
                case CatClip.MissObject: return "Miss_Object";
                case CatClip.MissObjectLose: return "Miss_Object_Lose";
                case CatClip.MissObjectLose2: return "Miss_Object_Lose_2";
                case CatClip.Victory: return "Cheering_Happy _Victory";
                case CatClip.Victory2: return "Cheering_Happy _Victory_2";
                case CatClip.Tail: return "Tail";
                default: throw new ArgumentOutOfRangeException(nameof(clip), clip, null);
            }
        }
    }
}
