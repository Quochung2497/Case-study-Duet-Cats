namespace Game
{
    // Names two setup phases; each MonoBehaviour calls them from Awake and Start.
    public interface IInstaller
    {
        void AwakeInitialize();
        void StartInitialize();
    }
}
