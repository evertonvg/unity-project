using UnityEngine;
using UnityEngine.InputSystem;

namespace RetwineMake.Player
{
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] InputActionAsset inputActions;
        [SerializeField] Transform muzzle;
        [SerializeField] float range = 100f;
        [SerializeField] float damage = 25f;
        [SerializeField] float fireCooldown = 0.15f;
        [SerializeField] LayerMask hitMask = ~0;

        InputAction attackAction;
        float nextFireTime;

        void Awake()
        {
            var map = inputActions.FindActionMap("Player", throwIfNotFound: true);
            attackAction = map.FindAction("Attack", throwIfNotFound: true);
        }

        void OnEnable() => attackAction.Enable();
        void OnDisable() => attackAction.Disable();

        void Update()
        {
            if (attackAction.IsPressed() && Time.time >= nextFireTime)
                Fire();
        }

        void Fire()
        {
            nextFireTime = Time.time + fireCooldown;

            Vector3 origin = muzzle != null ? muzzle.position : transform.position;
            Vector3 direction = muzzle != null ? muzzle.forward : transform.forward;

            if (Physics.Raycast(origin, direction, out var hit, range, hitMask, QueryTriggerInteraction.Ignore))
            {
                var damageable = hit.collider.GetComponentInParent<IDamageable>();
                damageable?.ApplyDamage(damage, hit.point, hit.normal);
                Debug.DrawLine(origin, hit.point, Color.red, 0.5f);
            }
            else
            {
                Debug.DrawRay(origin, direction * range, Color.yellow, 0.5f);
            }
        }
    }

    public interface IDamageable
    {
        void ApplyDamage(float amount, Vector3 hitPoint, Vector3 hitNormal);
    }
}
