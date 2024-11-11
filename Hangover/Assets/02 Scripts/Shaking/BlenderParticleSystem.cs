using UnityEngine;

public class BlenderParticleSystem : MonoBehaviour
{
    private ParticleSystem particleSystem;
    
    [Header("Blender Settings")]
    [SerializeField] private float blenderRadius = 0.5f;
    [SerializeField] private float blenderLength = 1f;
    [SerializeField] private Color liquidColor = Color.red;
    [SerializeField] private float rotationSpeed = 2f;
    
    [Header("Particle Settings")]
    [SerializeField] private float particleSize = 0.1f;
    [SerializeField] private int maxParticles = 1000;
    [SerializeField] private float particleLifetime = 2f;
    [SerializeField] private float startSpeed = 1f;

    void Start()
    {
        // 초기 셋업
        if (particleSystem == null)
            particleSystem = GetComponent<ParticleSystem>();

        // duration은 한 번만 설정
        var main = particleSystem.main;
        main.duration = 5f;
        main.loop = true;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        // 초기 collision 설정
        var collision = particleSystem.collision;
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.bounce = 0.2f;
        collision.dampen = 0.6f;

        // 초기 모듈 활성화
        var rotationOverLifetime = particleSystem.rotationOverLifetime;
        rotationOverLifetime.enabled = true;

        var velocityOverLifetime = particleSystem.velocityOverLifetime;
        velocityOverLifetime.enabled = true;

        var colorOverLifetime = particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
    }

    void Update()
    {
        UpdateParticleSystem();
    }

    private void UpdateParticleSystem()
    {
        // Main module update (duration 제외)
        var main = particleSystem.main;
        main.maxParticles = maxParticles;
        main.startSize = particleSize;
        main.startSpeed = startSpeed;
        main.startLifetime = particleLifetime;
        main.startColor = liquidColor;

        // Shape update
        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 0f;
        shape.radius = blenderRadius;
        shape.length = blenderLength;
        shape.radiusThickness = 1f;

        // Rotation update
        var rotationOverLifetime = particleSystem.rotationOverLifetime;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-rotationSpeed, rotationSpeed);

        // Velocity update
        var velocityOverLifetime = particleSystem.velocityOverLifetime;
        velocityOverLifetime.orbitalY = rotationSpeed;

        // Color gradient update
        var colorOverLifetime = particleSystem.colorOverLifetime;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(liquidColor, 0.0f), new GradientColorKey(liquidColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.5f, 1.0f) }
        );
        colorOverLifetime.color = grad;
    }

    public void StartBlending()
    {
        if (!particleSystem.isPlaying)
        {
            particleSystem.Play();
        }
    }

    public void StopBlending()
    {
        if (particleSystem.isPlaying)
        {
            // StopEmittingAndClear를 사용하여 완전히 정지
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }

    public void SetLiquidColor(Color newColor)
    {
        liquidColor = newColor;
    }
}