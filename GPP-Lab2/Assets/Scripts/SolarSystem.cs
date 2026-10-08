using UnityEngine;
using System.Collections.Generic;

public class SolarSystem : MonoBehaviour
{
    [Header("Все гравитирующие тела")]
    public List<GravityBody> bodies = new List<GravityBody>();

    [Header("Включить релятивистскую поправку (бонус)")]
    public bool enableRelativity = false;

    // "Скорость света" в игровых единицах. 
    // В реальности c огромна, поэтому поправка почти нулевая.
    // Мы её искусственно уменьшим, чтобы увидеть эффект.
    public float speedOfLight = 50f;

    void Awake()
    {
        // Автоматически собираем все GravityBody в сцене
        var all = FindObjectsByType<GravityBody>(FindObjectsSortMode.None);
        bodies.AddRange(all);
    }

    void Start()
    {
        AssignCircularOrbits();
    }

    void AssignCircularOrbits()
    {
        // Находим Солнце (самое массивное тело)
        GravityBody sun = null;
        foreach (var b in bodies)
        {
            if (sun == null || b.mass > sun.mass) sun = b;
        }
        if (sun == null) return;

        foreach (var planet in bodies)
        {
            if (planet == sun) continue;

            Vector3 toSun = sun.transform.position - planet.transform.position;
            float r = toSun.magnitude;

            // Скорость для круговой орбиты
            float v = Mathf.Sqrt(GravityBody.G * sun.mass / r);

            // Направление скорости - перпендикулярно радиусу (в плоскости XZ)
            Vector3 tangent = Vector3.Cross(toSun.normalized, Vector3.up).normalized;

            // Применяем скорость через Rigidbody
            planet.rb.linearVelocity = tangent * v;
        }
    }

    void FixedUpdate()
    {
        // Для каждой пары тел считаем силу тяготения
        for (int i = 0; i < bodies.Count; i++)
        {
            for (int j = i + 1; j < bodies.Count; j++)
            {
                ApplyGravity(bodies[i], bodies[j]);
            }
        }
    }

    void ApplyGravity(GravityBody a, GravityBody b)
    {
        Vector3 direction = b.transform.position - a.transform.position;
        float sqrDist = direction.sqrMagnitude;

        // Защита от деления на ноль (если тела столкнутся)
        if (sqrDist < 0.01f) return;

        Vector3 unitDir = direction.normalized;

        // Базовая ньютоновская сила: F = G * m1 * m2 / r^2
        float forceMag = GravityBody.G * a.mass * b.mass / sqrDist;

        // === БОНУС: релятивистская поправка ОТО ===
        if (enableRelativity)
        {
            // Поправка: (1 + 3*GM/(c^2 * r))
            // M - масса более тяжелого тела (для простоты - сумма)
            float M = a.mass + b.mass;
            float r = Mathf.Sqrt(sqrDist);
            float correction = 1f + (3f * GravityBody.G * M)
                                 / (speedOfLight * speedOfLight * r);
            forceMag *= correction;
        }

        Vector3 force = unitDir * forceMag;

        // По третьему закону Ньютона: силы равны и противоположны
        a.accumulatedForce += force;
        b.accumulatedForce -= force;
    }
}