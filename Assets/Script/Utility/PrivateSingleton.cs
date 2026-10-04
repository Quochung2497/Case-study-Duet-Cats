using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.TestTools;

namespace Utility
{
  [ExcludeFromCoverage]
  [ExcludeFromCodeCoverage]
  public class PrivateSingleton<T> : MonoBehaviour
    where T : Component
  {
    protected static T instance;
    public static bool HasInstance => instance is not null;

    public static T TryGetInstance() => HasInstance ? instance : null;

    public static T Current => instance;

    private static T Instance
    {
      get
      {
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
      if (!Application.isPlaying)
      {
        return;
      }

      instance = this as T;
    }
  }
}
