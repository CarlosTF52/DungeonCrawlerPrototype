using UnityEngine;

[CreateAssetMenu(fileName = "EnemyLaunchAttackProfile", menuName = "Dungeon Crawler/Enemies/Launch Attack Profile")]
public class EnemyLaunchAttackProfile : ScriptableObject
{
    [Header("Targeting")]
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private float attackRange = 4.5f;
    [SerializeField] private float minAttackRange = 0.75f;
    [SerializeField] private float turnSpeed = 720f;
    [SerializeField] private bool requireLineOfSight = true;
    [SerializeField] private LayerMask lineOfSightBlockers = ~0;

    [Header("Timing")]
    [SerializeField] private float windupDuration = 0.45f;
    [SerializeField] private float launchDuration = 0.55f;
    [SerializeField] private float recoveryDuration = 0.45f;
    [SerializeField] private float cooldownDuration = 1.2f;

    [Header("Launch")]
    [SerializeField] private float horizontalLaunchSpeed = 8f;
    [SerializeField] private float upwardLaunchSpeed = 2.5f;
    [SerializeField] private bool clearVelocityBeforeLaunch = true;
    [SerializeField] private bool stopVelocityOnRecovery = true;

    [Header("Landing")]
    [SerializeField] private bool endLaunchWhenGrounded = true;
    [SerializeField] private float minimumAirTime = 0.18f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayers = ~0;

    public string TargetTag => targetTag;
    public float AttackRange => attackRange;
    public float MinAttackRange => minAttackRange;
    public float TurnSpeed => turnSpeed;
    public bool RequireLineOfSight => requireLineOfSight;
    public LayerMask LineOfSightBlockers => lineOfSightBlockers;
    public float WindupDuration => windupDuration;
    public float LaunchDuration => launchDuration;
    public float RecoveryDuration => recoveryDuration;
    public float CooldownDuration => cooldownDuration;
    public float HorizontalLaunchSpeed => horizontalLaunchSpeed;
    public float UpwardLaunchSpeed => upwardLaunchSpeed;
    public bool ClearVelocityBeforeLaunch => clearVelocityBeforeLaunch;
    public bool StopVelocityOnRecovery => stopVelocityOnRecovery;
    public bool EndLaunchWhenGrounded => endLaunchWhenGrounded;
    public float MinimumAirTime => minimumAirTime;
    public float GroundCheckDistance => groundCheckDistance;
    public LayerMask GroundLayers => groundLayers;

    private void OnValidate()
    {
        attackRange = Mathf.Max(0f, attackRange);
        minAttackRange = Mathf.Clamp(minAttackRange, 0f, attackRange);
        turnSpeed = Mathf.Max(0f, turnSpeed);
        windupDuration = Mathf.Max(0f, windupDuration);
        launchDuration = Mathf.Max(0.01f, launchDuration);
        recoveryDuration = Mathf.Max(0f, recoveryDuration);
        cooldownDuration = Mathf.Max(0f, cooldownDuration);
        horizontalLaunchSpeed = Mathf.Max(0f, horizontalLaunchSpeed);
        upwardLaunchSpeed = Mathf.Max(0f, upwardLaunchSpeed);
        minimumAirTime = Mathf.Clamp(minimumAirTime, 0f, launchDuration);
        groundCheckDistance = Mathf.Max(0.01f, groundCheckDistance);
    }
}
