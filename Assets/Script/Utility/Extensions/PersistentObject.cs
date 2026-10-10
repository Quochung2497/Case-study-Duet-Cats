using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.TestTools;

namespace Utility
{
  [ExcludeFromCoverage, ExcludeFromCodeCoverage]
  public class PersistentObject : MonoBehaviour
  {
    private void Awake()
    {
      // Keep this GameObject across scene loads; the current playable does not use it.
      DontDestroyOnLoad(gameObject);
    }
  }
}
