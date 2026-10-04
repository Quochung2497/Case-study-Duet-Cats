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
      DontDestroyOnLoad(gameObject);
    }
  }
}
