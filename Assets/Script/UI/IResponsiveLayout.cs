using System;

namespace Game
{
    public interface IResponsiveLayout
    {
        event Action Changed;
        LayoutData Current { get; }
        void RecalculateLayout();
        float ScreenToWorldX(float screenX, float screenY);
    }
}
