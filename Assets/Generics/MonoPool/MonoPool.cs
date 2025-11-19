using System.Collections.Generic;
using UnityEngine;

namespace Generics
{
    /// <summary>
    /// Generic object pool for SPCBaseMono, enabling efficient reuse of pooled objects.
    /// </summary>
    public class MonoPool<T> : MonoBehaviour where T : MonoBehaviour, IPoolable
    {
        [SerializeField] private int initialSize = 10;
        [SerializeField] private T prefab;
        [SerializeField] private Transform parent;
        private Stack<T> _pool;

        /// <summary>
        /// Initializes the pool and creates the initial set of objects.
        /// </summary>
        private void Awake()
        {
            _pool = new Stack<T>();
            CreateObjects();
        }

        /// <summary>
        /// Gets an object from the pool, creating more if necessary.
        /// </summary>
        /// <returns>A pooled object instance.</returns>
        public T Get()
        {
            if (_pool.Count == 0)
            {
                CreateObjects();
            }
            T obj = _pool.Pop();
            obj.Reset();
            obj.gameObject.SetActive(true);
            return obj;
        }

        /// <summary>
        /// Returns an object to the pool and deactivates it.
        /// </summary>
        /// <param name="obj">The object to return to the pool.</param>
        public void Return(T obj)
        {
            obj.gameObject.SetActive(false);
            _pool.Push(obj);
        }

        /// <summary>
        /// Creates the initial set of pooled objects.
        /// </summary>
        private void CreateObjects()
        {
            for (int i = 0; i < initialSize; i++)
            {
                var obj = Instantiate(prefab, parent);
                obj.gameObject.SetActive(false);
                _pool.Push(obj);
            }
        }
    }
}
