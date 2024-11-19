using DG.Tweening;
using UnityEngine;

public class BlenderParticleSystem : MonoBehaviour
{
    private ParticleSystem particleSystem;
    
    [Header("블렌더 설정")]
    [SerializeField] private float blenderRadius = 0.3f;
    [SerializeField] private float blenderLength = 0f;
    [SerializeField] private Color liquidColor = Color.red;
    [SerializeField] private float rotationSpeed = 2f;
    
    [Header("파티클 설정")]
    [SerializeField] private float particleSize = 0.3f;
    [SerializeField] private int maxParticles = 1000;
    [SerializeField] private float particleLifetime = 5f;
    [SerializeField] private float startSpeed = 15f;
    
    private bool isVisible = false;
    private Sequence fadeSequence;
    
    void Awake()
    {
        // 파티클 시스템 컴포넌트 가져오기
        particleSystem = GetComponent<ParticleSystem>();
        
        // 파티클 시스템이 실행 중이라면 중지
        if (particleSystem.isPlaying)
        {
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        InitializeParticleSystem();
    }
    
    private void InitializeParticleSystem()
    {
        // 메인 모듈 초기 설정
        var main = particleSystem.main;
        main.duration = 5f;
        main.loop = true;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.startColor = new Color(liquidColor.r, liquidColor.g, liquidColor.b, 0f);

        // 충돌 설정
        var collision = particleSystem.collision;
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.bounce = 0.2f;
        collision.dampen = 0.6f;

        // 회전/속도/색상 모듈 활성화
        var rotationOverLifetime = particleSystem.rotationOverLifetime;
        rotationOverLifetime.enabled = true;

        var velocityOverLifetime = particleSystem.velocityOverLifetime;
        velocityOverLifetime.enabled = true;

        var colorOverLifetime = particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        
        // 초기 투명 파티클 생성
        // EmitAllParticlesInvisible(maxParticles);
    }

    void Update()
    {
        UpdateParticleSystem();
    }

    private void UpdateParticleSystem()
    {
        var main = particleSystem.main;
        main.maxParticles = maxParticles;
        main.startSize = particleSize;
        main.startSpeed = startSpeed;
        main.startLifetime = particleLifetime;
        main.startColor = liquidColor;

        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 0f;
        shape.radius = blenderRadius;
        shape.length = blenderLength;
        shape.radiusThickness = 1f;

        var rotationOverLifetime = particleSystem.rotationOverLifetime;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-rotationSpeed, rotationSpeed);

        var velocityOverLifetime = particleSystem.velocityOverLifetime;
        velocityOverLifetime.orbitalY = rotationSpeed;

        var colorOverLifetime = particleSystem.colorOverLifetime;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(liquidColor, 0.0f), new GradientColorKey(liquidColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.5f, 1.0f) }
        );
        colorOverLifetime.color = grad;
    }
    
    private void EmitAllParticlesInvisible(int count)
    {
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        emitParams.position = transform.position;
        emitParams.startColor = new Color(liquidColor.r, liquidColor.g, liquidColor.b, 0f);
        emitParams.startSize = particleSize;
        emitParams.startLifetime = particleLifetime;

        particleSystem.Emit(emitParams, count);
    }

    public void ToggleBlending()
    {
        if (!particleSystem.isPlaying)
        {
            particleSystem.Play();
        }
        else
        {
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