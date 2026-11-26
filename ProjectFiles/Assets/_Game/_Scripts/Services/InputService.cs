using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(-100)]
public sealed class InputService : MonoBehaviour
{
    public static InputService Instance { get; private set; }

    [Header("Tap")]
    [Tooltip("Max. displacement in pixels to count this as a tap rather than a drag")]
    [SerializeField] float tapMaxDistancePx = 15f;
    [SerializeField] float tapMaxDurationSec = 0.4f;

    public event Action<Vector2> OnTap;
    public event Action<Vector2> OnDragStart;
    public event Action<Vector2> OnDrag;
    public event Action<Vector2> OnDragEnd;

    public Vector2 PointerPosition { get; private set; }
    public bool IsPointerOverUI { get; private set; }
    public bool IsDragging { get; private set; }

    Vector2 pressPosition;
    float pressTime;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Debug.LogError("InputService > 1 on scene"); Destroy(gameObject); }
    }

    void Update()
    {
        var pointer = Pointer.current;
        if (pointer == null) return;

        PointerPosition = pointer.position.ReadValue();

        IsPointerOverUI = EventSystem.current != null
                          && EventSystem.current.IsPointerOverGameObject();

        bool pressed = pointer.press.wasPressedThisFrame;
        bool held = pointer.press.isPressed;
        bool released = pointer.press.wasReleasedThisFrame;

        if (pressed)
        {
            pressPosition = PointerPosition;
            pressTime = Time.unscaledTime;
            IsDragging = false;
        }

        if (held && !IsDragging)
        {
            float dist = Vector2.Distance(PointerPosition, pressPosition);
            float dur = Time.unscaledTime - pressTime;

            if (dist > tapMaxDistancePx || dur > tapMaxDurationSec)
            {
                IsDragging = true;
                OnDragStart?.Invoke(pressPosition);
            }
        }

        if (held && IsDragging)
            OnDrag?.Invoke(PointerPosition);

        if (released)
        {
            if (IsDragging)
            {
                OnDragEnd?.Invoke(PointerPosition);
            }
            else
            {
                float dist = Vector2.Distance(PointerPosition, pressPosition);
                float dur = Time.unscaledTime - pressTime;
                if (dist <= tapMaxDistancePx && dur <= tapMaxDurationSec)
                    OnTap?.Invoke(PointerPosition);
            }
            IsDragging = false;
        }
    }
}
