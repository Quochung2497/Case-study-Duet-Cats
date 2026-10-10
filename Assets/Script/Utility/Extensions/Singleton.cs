using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.TestTools;

namespace Utility
{
  [ExcludeFromCoverage]
  [ExcludeFromCodeCoverage]
  public class Singleton<T> : MonoBehaviour
    where T : Component
  {
    protected static T instance;
    public static bool HasInstance => instance is not null;

    public static T TryGetInstance() => HasInstance ? instance : null;

    public static T Current => instance;

    public static T Instance
    {
      get
      {
        // Prefer an instance in the scene; create a GameObject only if none exists.
        if (instance)
          return instance;
        instance = FindFirstObjectByType<T>();
        if (instance)
          return instance;
        var obj = new GameObject { name = typeof(T).Name + "AutoCreated" };
        instance = obj.AddComponent<T>();

        return instance;
      }
    }

    protected virtual void Awake() => InitializeSingleton();

    protected virtual void InitializeSingleton()
    {
      // Do not register a static instance in Edit mode.
      if (!Application.isPlaying)
      {
        return;
      }

      instance = this as T;
    }
  }
}
