using System.Collections;
using Animancer;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(AnimancerComponent))]
public class PlayerCombatAnimator : MonoBehaviour
{
    [SerializeField] private AnimancerComponent animancer;
    [SerializeField] private AnimationClip swingAnimation;
    [SerializeField] private MeleeAttack meleeAttack;
    [SerializeField] private PlayerWeaponHitbox weaponHitbox;
    [SerializeField] private float fadeDuration = 0.05f;
    [SerializeField] private float playbackSpeed = 1f;
    [SerializeField, Range(0f, 1f)] private float hitStartNormalizedTime = 0.25f;
    [SerializeField, Range(0f, 1f)] private float hitEndNormalizedTime = 0.55f;
    [SerializeField] private bool allowRestartSwing;

    [Header("Events")]
    [SerializeField] private UnityEngine.Events.UnityEvent swingStarted;
    [SerializeField] private UnityEngine.Events.UnityEvent hitFrameReached;
    [SerializeField] private UnityEngine.Events.UnityEvent swingEnded;

    private AnimancerState swingState;
    private Coroutine swingRoutine;
    private bool isSwinging;

    private void Awake()
    {
        if (animancer == null)
        {
            animancer = GetComponent<AnimancerComponent>();
        }

        if (meleeAttack == null)
        {
            meleeAttack = GetComponent<MeleeAttack>();
        }

        if (weaponHitbox == null)
        {
            weaponHitbox = GetComponentInChildren<PlayerWeaponHitbox>();
        }
    }

    private void OnValidate()
    {
        fadeDuration = Mathf.Max(0f, fadeDuration);
        playbackSpeed = Mathf.Max(0.01f, playbackSpeed);
        hitEndNormalizedTime = Mathf.Max(hitStartNormalizedTime, hitEndNormalizedTime);
    }

#if ENABLE_INPUT_SYSTEM
    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            TryPlaySwing();
        }
    }
#endif

    public bool TryPlaySwing()
    {
        if (swingAnimation == null || animancer == null)
        {
            Debug.LogWarning("PlayerCombatAnimator is missing an AnimancerComponent or swing animation.", this);
            return false;
        }

        if (isSwinging && !allowRestartSwing)
        {
            return false;
        }

        if (swingRoutine != null)
        {
            StopCoroutine(swingRoutine);
        }

        if (weaponHitbox != null)
        {
            weaponHitbox.EndHitWindow();
        }

        swingRoutine = StartCoroutine(SwingRoutine());
        return true;
    }

    public void TriggerHitFrame()
    {
        hitFrameReached?.Invoke();

        if (weaponHitbox != null)
        {
            weaponHitbox.BeginHitWindow();
            return;
        }

        if (meleeAttack != null)
        {
            meleeAttack.TryAttack();
        }
    }

    private IEnumerator SwingRoutine()
    {
        isSwinging = true;
        swingStarted?.Invoke();

        swingState = animancer.Play(swingAnimation, fadeDuration, FadeMode.FromStart);
        swingState.Speed = playbackSpeed;
        swingState.Time = 0f;

        float hitStartDelay = swingAnimation.length * hitStartNormalizedTime / playbackSpeed;
        yield return new WaitForSeconds(hitStartDelay);

        TriggerHitFrame();

        float hitDuration = swingAnimation.length * (hitEndNormalizedTime - hitStartNormalizedTime) / playbackSpeed;
        yield return new WaitForSeconds(hitDuration);

        if (weaponHitbox != null)
        {
            weaponHitbox.EndHitWindow();
        }

        float remainingDuration = swingAnimation.length * (1f - hitEndNormalizedTime) / playbackSpeed;
        yield return new WaitForSeconds(remainingDuration);

        isSwinging = false;
        swingRoutine = null;
        swingEnded?.Invoke();
    }
}
