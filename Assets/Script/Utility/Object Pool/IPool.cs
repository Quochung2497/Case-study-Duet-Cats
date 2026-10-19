using UnityEngine.Pool;

namespace Utility
{
  // A minimal contract for accessing the Unity pool without its concrete wrapper.
  public interface IPool<T> where T : class
  {
    IObjectPool<T> GetPool();
  }
}
