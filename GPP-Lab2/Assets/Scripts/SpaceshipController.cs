using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpaceshipController : MonoBehaviour
{
    [Header("Настройки вращения")]
    [Tooltip("Сила, с которой корабль меняет направление взгляда")]
    [SerializeField] private float rotationSpeed = 50f;

    [Header("Настройки тяги")]
    [Tooltip("Сила двигателей корабля")]
    [SerializeField] private float thrustForce = 10f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Отключаем гравитацию для космоса
        rb.useGravity = false;
    }

    private void FixedUpdate()
    {
        HandleRotation();
        HandleThrust();
    }

    private void HandleRotation()
    {
        float pitchInput = Input.GetAxis("Vertical");   // W = 1, S = -1
        float yawInput = -Input.GetAxis("Horizontal");   // D = 1, A = -1

        // Тангаж (W/S): вращаем вокруг локальной X (ось крыльев)
        // Рыскание (A/D): вращаем вокруг локальной Z (сейчас она смотрит в мировое "небо")
        Vector3 torque = new Vector3(-pitchInput, 0f, yawInput) * rotationSpeed;

        rb.AddRelativeTorque(torque, ForceMode.Force);
    }

    private void HandleThrust()
    {
        Vector3 force = Vector3.zero;

        // 1. Вперед / Назад (I / K)
        // "Нос" капсулы теперь смотрит по локальной оси Y (Vector3.up)
        if (Input.GetKey(KeyCode.I)) force += Vector3.up;
        if (Input.GetKey(KeyCode.K)) force += Vector3.down;

        // 2. Влево / Вправо (J / L)
        // "Крылья" капсулы остались на локальной оси X
        if (Input.GetKey(KeyCode.J)) force += Vector3.left;
        if (Input.GetKey(KeyCode.L)) force += Vector3.right;

        // 3. Вверх / Вниз (U / O)
        // "Спина" капсулы теперь смотрит в мировое небо, это локальная ось Z (Vector3.forward)
        if (Input.GetKey(KeyCode.U)) force += Vector3.forward;
        if (Input.GetKey(KeyCode.O)) force += Vector3.back;

        // Применяем итоговую силу
        rb.AddRelativeForce(force * thrustForce, ForceMode.Force);
    }
}