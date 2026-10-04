using UnityEngine.Pool;

namespace DanielTran.Utility
{
  public interface IPool<T> where T : class
  {
    IObjectPool<T> GetPool();
  }
}
