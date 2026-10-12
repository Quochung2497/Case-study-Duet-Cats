using System;

namespace Game
{
    public interface IViewport
    {
        event Action<bool> Changed;
        bool HasViewport { get; }
        bool IsPortrait { get; }
    }
}
