using UnityEngine;

namespace GameBase
{
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (T)FindObjectOfType(typeof(T));

                    if (instance == null)
                    {
                        Debug.Log(typeof(T) + " does not exist, create.");

                        GameObject obj = new GameObject();
                        #if UNITY_EDITOR
                        obj.name = typeof(T).Name;
                        #endif
                        instance = obj.AddComponent<T>();
                        #if UNITY_EDITOR
                        instance.runInEditMode = true;
                        #endif
                    }
                }
                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = (T)this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected bool IsDestroyed()
        {
            return Instance != this;
        }
    }
}
