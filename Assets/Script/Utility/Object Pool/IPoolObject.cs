using UnityEngine.Pool;

namespace DanielTran.Utility
{
  public interface IPoolObject<T> where T : class
  {
    void SetPool(IObjectPool<T> pool);
    void ResetForReuse();
    void Release();
  }
}
