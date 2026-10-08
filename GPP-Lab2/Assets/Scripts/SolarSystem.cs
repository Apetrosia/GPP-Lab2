using UnityEngine;
using System.Collections.Generic;

public class SolarSystem : MonoBehaviour
{
    [Header("Все гравитирующие тела")]
    public List<GravityBody> bodies = new List<GravityBody>();

    [Header("Настройка эффекта ОТО")]
    [Tooltip("Искусственно заниженная скорость света для наглядности эффекта.")]
    public float speedOfLight = 150f;

    [Header("Визуализация")]
    [Tooltip("Показывать ли ярко-зеленый след орбиты Меркурия")]
    public bool showMercuryTrajectory = true;

    private TrailRenderer mercuryTrail;

    void Awake()
    {
        var all = FindObjectsByType<GravityBody>(FindObjectsSortMode.None);
        bodies.AddRange(all);
    }

    void Start()
    {
        AssignOrbits();

        // Находим Меркурий и его компонент TrailRenderer при старте
        GameObject mercuryObj = GameObject.Find("Mercury");
        if (mercuryObj != null)
        {
            mercuryTrail = mercuryObj.GetComponent<TrailRenderer>();
            if (mercuryTrail != null)
            {
                // Применяем начальное состояние галочки
                mercuryTrail.enabled = showMercuryTrajectory;
            }
            else
            {
                Debug.LogWarning("На Меркурии не найден компонент TrailRenderer!");
            }
        }
    }

    void Update()
    {
        // Если галочка в инспекторе изменилась, обновляем видимость следа
        if (mercuryTrail != null && mercuryTrail.enabled != showMercuryTrajectory)
        {
            mercuryTrail.enabled = showMercuryTrajectory;

            // Опционально: очищаем след при выключении, чтобы он не висел в воздухе
            if (!showMercuryTrajectory)
            {
                mercuryTrail.Clear();
            }
        }
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

            float v = Mathf.Sqrt(GravityBody.G * sun.mass / r);
            float eccentricityFactor = 1.0f;

            if (planet.gameObject.name == "Mercury")
            {
                eccentricityFactor = 1.15f;
            }
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

        if (sqrDist < 0.1f) return;

        Vector3 unitDir = direction.normalized;
        float forceMag = GravityBody.G * a.mass * b.mass / sqrDist;

        float dominantMass = Mathf.Max(a.mass, b.mass);
        float r = Mathf.Sqrt(sqrDist);

        float correction = 1f + (3f * GravityBody.G * dominantMass) / (speedOfLight * speedOfLight * r);
        forceMag *= correction;

        Vector3 force = unitDir * forceMag;

        a.accumulatedForce += force;
        b.accumulatedForce -= force;
    }
}