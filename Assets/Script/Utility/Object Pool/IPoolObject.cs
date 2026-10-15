using UnityEngine.Pool;

namespace Utility
{
  // A pooled object must reset itself and return to its owning pool.
  public interface IPoolObject<T> where T : class
  {
    void SetPool(IObjectPool<T> pool);
    void ResetForReuse();
    void Release();
  }
}
