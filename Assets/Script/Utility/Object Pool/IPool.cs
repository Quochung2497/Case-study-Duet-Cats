using UnityEngine.Pool;

namespace Utility
{
  public interface IPool<T> where T : class
  {
    IObjectPool<T> GetPool();
  }
}
