using UnityEngine;

public class FoosballRod : MonoBehaviour
{
    public float minSlide = -0.35f;
    public float maxSlide = 0.35f;

    public float minAngle = -90f;
    public float maxAngle = 90f;

    private Rigidbody rb;
    private Vector3 startPos;
    private Quaternion startRot;
    private float slideOffset;
    private float currentAngle;

    void Start()
    {
        startPos = transform.localPosition;
        startRot = transform.localRotation;

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    public void MoveAndRotate(float lsY, float rsX, float slideSpeed, float maxAngleVal, float snapSpeed)
    {
        // 1. Slide Up/Down (Left Stick Y)
        slideOffset = Mathf.Clamp(slideOffset + lsY * slideSpeed * Time.deltaTime, minSlide, maxSlide);
        Vector3 targetLocalPos = startPos + new Vector3(slideOffset, 0f, 0f);

        // 2. Tilt & Hold Angle (Right Stick X), return to 0 when released
        float targetAngle = rsX * maxAngleVal;
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, snapSpeed * Time.deltaTime);

        // Stop microscopic lerping when nearly at rest so the foot never jitters against the ball
        if (Mathf.Abs(currentAngle - targetAngle) < 0.05f)
        {
            currentAngle = targetAngle;
        }

        Quaternion targetLocalRot = startRot * Quaternion.Euler(currentAngle, 0f, 0f);

        // Move via Physics Rigidbody instead of teleporting Transform
        if (transform.parent != null)
        {
            rb.MovePosition(transform.parent.TransformPoint(targetLocalPos));
            rb.MoveRotation(transform.parent.rotation * targetLocalRot);
        }
        else
        {
            rb.MovePosition(targetLocalPos);
            rb.MoveRotation(targetLocalRot);
        }
    }
}