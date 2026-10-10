using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityBody : MonoBehaviour
{
    [Header("Физика")]
    public float mass = 1f;

    public static float G = 10f;

    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Vector3 accumulatedForce = Vector3.zero;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        mass = rb.mass;

        rb.useGravity = false;

        rb.sleepThreshold = 0f;

        rb.linearDamping = 0f;
        rb.angularDamping = 0f;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        if (!rb.isKinematic)
        {
            rb.AddForce(accumulatedForce, ForceMode.Force);
        }

        accumulatedForce = Vector3.zero;

        transform.Rotate(Vector3.up * 10f * Time.fixedDeltaTime);
    }
}