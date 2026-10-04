using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.TestTools;

namespace DanielTran.Utility
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
