using UnityEngine;
using UnityEngine.Pool;

namespace DanielTran.Utility
{
  public abstract class Pool<T> : IPool<T>
    where T : MonoBehaviour, IPoolObject<T>
  {
    // Private Fields
    protected readonly T Prefab;
    protected readonly IObjectPool<T> PoolInstance;

    // Public
    public Pool(T prefab, bool collectionCheck = true, int maxSize = 10, int defaultCapacity = 10)
    {
      Prefab = prefab;

      PoolInstance = new ObjectPool<T>(
        OnCreate,
        OnGet,
        OnRelease,
        OnDestroyObject,
        collectionCheck: collectionCheck,
        maxSize: maxSize,
        defaultCapacity: defaultCapacity
      );
    }

    // IPool Interface
    public IObjectPool<T> GetPool()
    {
      return PoolInstance;
    }

    // Private
    protected virtual T OnCreate()
    {
      var objectInstance = GameObject.Instantiate(Prefab);
      objectInstance.SetPool(PoolInstance);
      return objectInstance;
    }

    protected virtual void OnGet(T poolObject)
    {
      poolObject.ResetForReuse();
      poolObject.gameObject.SetActive(true);
    }

    protected virtual void OnRelease(T poolObject)
    {
      poolObject.gameObject.SetActive(false);
    }

    protected virtual void OnDestroyObject(T poolObject)
    {
      GameObject.Destroy(poolObject.gameObject);
    }
  }
}
