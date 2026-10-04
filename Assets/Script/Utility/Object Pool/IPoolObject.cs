using UnityEngine.Pool;

namespace Utility
{
  public interface IPoolObject<T> where T : class
  {
    void SetPool(IObjectPool<T> pool);
    void ResetForReuse();
    void Release();
  }
}
