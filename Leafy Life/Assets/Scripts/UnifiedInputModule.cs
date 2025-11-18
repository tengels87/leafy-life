using UnityEngine;
using UnityEngine.InputSystem;

public class UnifiedInputModule : MonoBehaviour {
    // ====== SINGLETON ======
    private static UnifiedInputModule _instance;
    public static UnifiedInputModule Instance {
        get
        {
            if (_instance == null) {
                // Try to find an existing instance
                _instance = FindObjectOfType<UnifiedInputModule>();
                if (_instance == null) {
                    // Create a new GameObject if none exists
                    GameObject go = new GameObject("UnifiedInputModule");
                    _instance = go.AddComponent<UnifiedInputModule>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    // ====== CONFIGURATION ======
    [Header("Gesture Settings")]
    public float doubleTapMaxDelay = 0.25f;
    public float longPressDuration = 0.35f;
    public float swipeThreshold = 60f;
    public float dragDeadzone = 1.5f;
    public float smoothing = 0.08f;

    // ====== EVENTS ======
    public System.Action<Vector2> OnPress;
    public System.Action<Vector2> OnTap;
    public System.Action<Vector2> OnDoubleTap;
    public System.Action<Vector2> OnLongPress;
    public System.Action<Vector2, Vector2> OnSwipe; // dir, total distance
    public System.Action<Vector2> OnDragDelta;

    // ====== INTERNAL STATE ======
    private Vector2 rawPosition;
    private Vector2 startPosition;
    private Vector2 lastHeldPosition;      // for drag & release

    private float pressStartTime = -1;
    private float lastTapTime = -1;

    private bool isPressing = false;
    private bool longPressTriggered = false;

    // ====== PUBLIC POINTER POSITION ======
    public Vector2 PointerPosition => rawPosition;

    // ====== POINTER INPUT ======
    private static Vector2 ReadPointer() {
        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.position.ReadValue();
        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();
        return Vector2.zero;
    }

    public static bool DownThisFrame =>
        (Mouse.current?.leftButton.wasPressedThisFrame ?? false) ||
        (Touchscreen.current?.primaryTouch.press.wasPressedThisFrame ?? false);

    public static bool UpThisFrame =>
        (Mouse.current?.leftButton.wasReleasedThisFrame ?? false) ||
        (Touchscreen.current?.primaryTouch.press.wasReleasedThisFrame ?? false);

    void Awake() {
        if (_instance == null) {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        } else if (_instance != this) {
            Destroy(gameObject);
        }
    }

    void Update() {
        rawPosition = ReadPointer();

        // ====== PRESS START ======
        if (DownThisFrame) {
            isPressing = true;
            longPressTriggered = false;
            pressStartTime = Time.time;

            startPosition = rawPosition;
            lastHeldPosition = rawPosition;

            OnPress?.Invoke(rawPosition);
        }

        // ====== PRESS HOLD ======
        if (isPressing) {
            Vector2 delta = rawPosition - lastHeldPosition;

            if (delta.sqrMagnitude > dragDeadzone * dragDeadzone) {
                OnDragDelta?.Invoke(delta);
            }

            lastHeldPosition = rawPosition;

            if (!longPressTriggered && Time.time - pressStartTime >= longPressDuration) {
                longPressTriggered = true;
                OnLongPress?.Invoke(startPosition);
            }
        }

        // ====== RELEASE ======
        if (UpThisFrame && isPressing) {
            isPressing = false;

            Vector2 releasePos = lastHeldPosition;
            float travelDist = Vector2.Distance(releasePos, startPosition);

            if (travelDist >= swipeThreshold) {
                Vector2 swipeVector = releasePos - startPosition;
                Vector2 dir = swipeVector.normalized;
                OnSwipe?.Invoke(dir, swipeVector);
                return;
            }

            if (longPressTriggered)
                return;

            if (Time.time - lastTapTime <= doubleTapMaxDelay) {
                OnDoubleTap?.Invoke(releasePos);
                lastTapTime = -1;
            } else {
                OnTap?.Invoke(releasePos);
                lastTapTime = Time.time;
            }
        }
    }
}