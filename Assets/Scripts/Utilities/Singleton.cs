using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    static T _instance;

    [SerializeField, Tooltip("Enable this to prevent the object from being destroyed")]
    protected bool isPersistent = false;

    public static T instance
    {
        get
        {
            _instance = _instance ?? FindObjectOfType<T>();

            if (_instance == null)
            {
                // Create it
                GameObject go = new GameObject(typeof(T).Name, typeof(T));
                _instance = go.GetComponent<T>();
            }

            return _instance;
        }
    }

    public virtual void Awake()
    {        
        if (_instance == null)
        {
            _instance = this as T;

            if (isPersistent)
            {
                // Persistent objects cannot have parents
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
        }

        if (_instance != this)
        {
            // Instance already exists
            DestroyImmediate(gameObject);
            return;
        }
    }
}