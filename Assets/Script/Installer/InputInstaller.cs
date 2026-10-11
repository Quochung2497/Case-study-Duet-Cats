using Input;
using UnityEngine;
using Utility.DependencyInjection;

namespace Game
{
    public sealed class InputInstaller : MonoBehaviour
    {
        [Inject] private IInputReader _input;

        private void Awake()
        {
            if (_input == null)
            {
                Debug.LogError("InputInstaller needs an IInputReader.", this);
                return;
            }

            // Own the input callbacks for this scene's lifetime.
            _input.Initialize();
        }

        private void OnDestroy()
        {
            if (_input != null)
                _input.Shutdown();
        }
    }
}
