using UnityEngine;
using UnityEngine.Events;

public class MonoBehaviourEvents : MonoBehaviour
{
    [SerializeField] private UnityEvent onAwake = new();
    [SerializeField] private UnityEvent onStart = new();
    [SerializeField] private UnityEvent onEnable = new();
    [SerializeField] private UnityEvent onDisable = new();
    [SerializeField] private UnityEvent onDestroy = new();

    private void Awake()
    {
        onAwake?.Invoke();
    }

    private void Start()
    {
        onStart?.Invoke();
    }

    private void OnEnable()
    {
        onEnable?.Invoke();
    }

    private void OnDisable()
    {
        onDisable?.Invoke();
    }

    private void OnDestroy()
    {
        onDestroy?.Invoke();
    }
}