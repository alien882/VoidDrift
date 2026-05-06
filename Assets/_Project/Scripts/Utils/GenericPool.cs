using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Pool genérico reutilizable para cualquier MonoBehaviour.
/// Usa el sistema de pooling integrado de Unity 6.
/// </summary>
public class GenericPool<T> where T : MonoBehaviour
{
    private readonly ObjectPool<T> pool;
    private readonly T prefab;
    private readonly Transform parent;

    /// <param name="prefab">Prefab a instanciar</param>
    /// <param name="parent">Transform padre para mantener la jerarquía limpia</param>
    /// <param name="defaultCapacity">Cantidad inicial de objetos pre-creados</param>
    /// <param name="maxSize">Máximo de objetos en el pool</param>
    public GenericPool(T prefab, Transform parent, int defaultCapacity = 10, int maxSize = 50)
    {
        this.prefab = prefab;
        this.parent = parent;

        pool = new ObjectPool<T>(
            createFunc: CreateObject,
            actionOnGet: OnGetFromPool,
            actionOnRelease: OnReturnToPool,
            actionOnDestroy: OnDestroyObject,
            collectionCheck: false, // Desactivado en producción para mejor rendimiento
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }

    public T Get() => pool.Get();
    public void Release(T obj) => pool.Release(obj);
    public int CountActive => pool.CountActive;

    private T CreateObject()
    {
        T obj = Object.Instantiate(prefab, parent);
        obj.gameObject.SetActive(false);
        return obj;
    }

    private void OnGetFromPool(T obj) => obj.gameObject.SetActive(true);
    private void OnReturnToPool(T obj) => obj.gameObject.SetActive(false);
    private void OnDestroyObject(T obj) => Object.Destroy(obj.gameObject);
}
