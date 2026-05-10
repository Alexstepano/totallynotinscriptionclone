using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }
    [Header("Setup")]
    public Transform pivot;       // table center point
    public float distance = 7f;
    public float downAngle = 35f;

    [Header("Limits")]
    public float minYaw = -45f;
    public float maxYaw = 45f;
    public float sensitivity = 0.25f;

    private float currentYaw;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    void Awake() => Instance = this;

    void Start()
    {
        currentYaw = 0f;
        UpdateTarget();
        transform.position = targetPosition;
        transform.rotation = targetRotation;
    }


    public void RotateCamera(float deltaX)
    {
        currentYaw += deltaX * sensitivity;
        currentYaw = Mathf.Clamp(currentYaw, minYaw, maxYaw);
        UpdateTarget();
    }

    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 8f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
    }

    void UpdateTarget()
    {
        targetRotation = Quaternion.Euler(downAngle, currentYaw, 0);
        targetPosition = pivot.position - targetRotation * Vector3.forward * distance;
    }

}
