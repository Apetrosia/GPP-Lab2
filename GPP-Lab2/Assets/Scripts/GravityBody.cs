using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityBody : MonoBehaviour
{
    [Header("Физика")]
    // Масса теперь берётся из Rigidbody в Awake()
    // Но переменную оставляем — вдруг понадобится в коде
    public float mass = 1f;

    // Гравитационная постоянная
    public static float G = 10f;

    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Vector3 accumulatedForce = Vector3.zero;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // ВАЖНО: берём массу из Rigidbody
        mass = rb.mass;

        // Отключаем стандартную гравитацию Unity
        rb.useGravity = false;

        // Чтобы тело не засыпало
        rb.sleepThreshold = 0f;

        // Убираем сопротивление
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;

        // Интерполяция для плавности
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        // Применяем силу только если тело НЕ кинематическое
        // (кинематические тела игнорируют AddForce, но проверим на всякий случай)
        if (!rb.isKinematic)
        {
            rb.AddForce(accumulatedForce, ForceMode.Force);
        }

        // Обнуляем силу для следующего кадра
        accumulatedForce = Vector3.zero;

        // Вращение вокруг своей оси
        transform.Rotate(Vector3.up * 10f * Time.fixedDeltaTime);
    }
}