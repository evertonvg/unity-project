using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RetwineMake.Player
{
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] InputActionAsset inputActions;
        [SerializeField] Transform weaponModel;
        [SerializeField] Transform muzzlePoint;

        [Header("Ballistics")]
        [SerializeField] float range = 100f;
        [SerializeField] float damage = 25f;
        [SerializeField] float fireCooldown = 0.25f;
        [SerializeField] LayerMask hitMask = ~0;

        [Header("Ammo")]
        [SerializeField] int magazineSize = 8;
        [SerializeField] float reloadDuration = 1.2f;

        [Header("Fire kick")]
        [SerializeField] float kickBackDistance = 0.04f;
        [SerializeField] float kickUpAngle = 8f;
        [SerializeField] float kickOutTime = 0.03f;
        [SerializeField] float kickRecoverTime = 0.12f;

        [Header("Reload motion")]
        [SerializeField] float reloadDropDistance = 0.18f;
        [SerializeField] float reloadTiltAngle = 25f;

        InputAction attackAction;
        InputAction reloadAction;

        int currentAmmo;
        bool isReloading;
        float nextFireTime;

        Vector3 restLocalPosition;
        Quaternion restLocalRotation;
        Coroutine weaponMotion;

        public int CurrentAmmo => currentAmmo;
        public int MagazineSize => magazineSize;
        public bool IsReloading => isReloading;

        void Awake()
        {
            var map = inputActions.FindActionMap("Player", throwIfNotFound: true);
            attackAction = map.FindAction("Attack", throwIfNotFound: true);
            reloadAction = map.FindAction("Reload", throwIfNotFound: true);

            currentAmmo = magazineSize;

            if (weaponModel != null)
            {
                restLocalPosition = weaponModel.localPosition;
                restLocalRotation = weaponModel.localRotation;
            }
        }

        void OnEnable()
        {
            attackAction.Enable();
            reloadAction.Enable();
        }

        void OnDisable()
        {
            attackAction.Disable();
            reloadAction.Disable();
        }

        void Update()
        {
            if (reloadAction.WasPressedThisFrame() && !isReloading && currentAmmo < magazineSize)
            {
                StartReload();
                return;
            }

            if (attackAction.IsPressed() && !isReloading && Time.time >= nextFireTime)
                Fire();
        }

        void Fire()
        {
            nextFireTime = Time.time + fireCooldown;

            if (currentAmmo <= 0)
            {
                Debug.Log("[Weapon] Dry fire - press R to reload.");
                return;
            }

            currentAmmo--;

            Vector3 origin = muzzlePoint != null ? muzzlePoint.position : transform.position;
            Vector3 direction = muzzlePoint != null ? muzzlePoint.forward : transform.forward;

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

            Debug.Log($"[Weapon] Fired - {currentAmmo}/{magazineSize}");
            PlayMotion(FireKickRoutine());
        }

        void StartReload()
        {
            isReloading = true;
            Debug.Log("[Weapon] Reloading...");
            PlayMotion(ReloadRoutine());
        }

        void PlayMotion(IEnumerator routine)
        {
            if (weaponModel == null)
                return;

            if (weaponMotion != null)
                StopCoroutine(weaponMotion);
            weaponMotion = StartCoroutine(routine);
        }

        IEnumerator FireKickRoutine()
        {
            Vector3 kickPos = restLocalPosition + new Vector3(0f, 0f, -kickBackDistance);
            Quaternion kickRot = restLocalRotation * Quaternion.Euler(-kickUpAngle, 0f, 0f);

            yield return Tween(weaponModel.localPosition, kickPos, weaponModel.localRotation, kickRot, kickOutTime);
            yield return Tween(weaponModel.localPosition, restLocalPosition, weaponModel.localRotation, restLocalRotation, kickRecoverTime);

            weaponMotion = null;
        }

        IEnumerator ReloadRoutine()
        {
            Vector3 downPos = restLocalPosition + new Vector3(0f, -reloadDropDistance, 0f);
            Quaternion downRot = restLocalRotation * Quaternion.Euler(reloadTiltAngle, 0f, 0f);

            float half = reloadDuration * 0.5f;
            yield return Tween(weaponModel.localPosition, downPos, weaponModel.localRotation, downRot, half);

            currentAmmo = magazineSize;
            Debug.Log($"[Weapon] Reloaded - {currentAmmo}/{magazineSize}");

            yield return Tween(weaponModel.localPosition, restLocalPosition, weaponModel.localRotation, restLocalRotation, half);

            isReloading = false;
            weaponMotion = null;
        }

        IEnumerator Tween(Vector3 fromPos, Vector3 toPos, Quaternion fromRot, Quaternion toRot, float duration)
        {
            if (duration <= 0f)
            {
                weaponModel.localPosition = toPos;
                weaponModel.localRotation = toRot;
                yield break;
            }

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / duration);
                weaponModel.localPosition = Vector3.Lerp(fromPos, toPos, u);
                weaponModel.localRotation = Quaternion.Slerp(fromRot, toRot, u);
                yield return null;
            }

            weaponModel.localPosition = toPos;
            weaponModel.localRotation = toRot;
        }
    }

    public interface IDamageable
    {
        void ApplyDamage(float amount, Vector3 hitPoint, Vector3 hitNormal);
    }
}
