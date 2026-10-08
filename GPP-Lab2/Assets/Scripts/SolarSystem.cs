using UnityEngine;
using System.Collections.Generic;

public class SolarSystem : MonoBehaviour
{
    [Header("Все гравитирующие тела")]
    public List<GravityBody> bodies = new List<GravityBody>();

    [Header("Настройка эффекта ОТО")]
    [Tooltip("Искусственно заниженная скорость света для наглядности эффекта. В реальности эффект виден за столетия, здесь - за минуты.")]
    public float speedOfLight = 150f;

    void Awake()
    {
        var all = FindObjectsByType<GravityBody>(FindObjectsSortMode.None);
        bodies.AddRange(all);
    }

    void Start()
    {
        AssignOrbits();
    }

    void AssignOrbits()
    {
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

            // Базовая скорость для круговой орбиты
            float v = Mathf.Sqrt(GravityBody.G * sun.mass / r);

            // Задаем эллиптичность орбиты
            float eccentricityFactor = 1.0f;

            // Меркурий: делаем орбиту вытянутой, чтобы было ЧЕМУ смещаться
            if (planet.gameObject.name == "Mercury")
            {
                eccentricityFactor = 1.15f; // 1.15 = старт из перигелия, полет к апогею
            }
            // Марс: небольшой эллипс для реалистичности
            else if (planet.gameObject.name == "Mars")
            {
                eccentricityFactor = 1.05f;
            }

            Vector3 tangent = Vector3.Cross(toSun.normalized, Vector3.up).normalized;
            planet.rb.linearVelocity = tangent * v * eccentricityFactor;
        }
    }

    void FixedUpdate()
    {
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

        if (sqrDist < 0.1f) return; // Защита от сингулярности

        Vector3 unitDir = direction.normalized;
        float forceMag = GravityBody.G * a.mass * b.mass / sqrDist;

        // === ПОСТОЯННАЯ РЕЛЯТИВИСТСКАЯ ПОПРАВКА (ОТО) ===
        // Используем массу доминирующего тела (Солнца)
        float dominantMass = Mathf.Max(a.mass, b.mass);
        float r = Mathf.Sqrt(sqrDist);

        // Формула 1PN поправки: сила увеличивается тем сильнее, чем ближе планета к Солнцу (чем меньше r)
        float correction = 1f + (3f * GravityBody.G * dominantMass) / (speedOfLight * speedOfLight * r);

        forceMag *= correction;

        Vector3 force = unitDir * forceMag;

        a.accumulatedForce += force;
        b.accumulatedForce -= force;
    }
}