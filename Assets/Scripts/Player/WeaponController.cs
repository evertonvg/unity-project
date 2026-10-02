using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RetwineMake.Player
{
    [Serializable]
    public class WeaponSlot
    {
        public string weaponName = "Weapon";
        public Transform weaponModel;
        public Transform muzzlePoint;

        [Header("Ballistics")]
        public float range = 100f;
        public float damage = 25f;
        public float fireCooldown = 0.25f;

        [Header("Ammo")]
        public int magazineSize = 8;
        public float reloadDuration = 1.2f;

        [Header("Fire kick")]
        public float kickBackDistance = 0.04f;
        public float kickUpAngle = 8f;
        public float kickOutTime = 0.03f;
        public float kickRecoverTime = 0.12f;

        [Header("Reload motion")]
        public float reloadDropDistance = 0.18f;
        public float reloadTiltAngle = 25f;

        [NonSerialized] public int currentAmmo;
        [NonSerialized] public Vector3 restLocalPosition;
        [NonSerialized] public Quaternion restLocalRotation;
    }

    public class WeaponController : MonoBehaviour
    {
        [SerializeField] InputActionAsset inputActions;
        [SerializeField] LayerMask hitMask = ~0;
        [SerializeField] WeaponSlot[] weapons;

        [Header("Weapon switch motion")]
        [SerializeField] float switchDownDistance = 0.22f;
        [SerializeField] float switchOutTime = 0.08f;
        [SerializeField] float switchInTime = 0.12f;

        InputAction attackAction;
        InputAction reloadAction;
        InputAction nextWeaponAction;
        InputAction prevWeaponAction;

        int currentIndex;
        bool isReloading;
        bool isSwitching;
        float nextFireTime;
        Coroutine weaponMotion;

        WeaponSlot Current => weapons[currentIndex];

        public int CurrentAmmo => Current.currentAmmo;
        public int MagazineSize => Current.magazineSize;
        public bool IsReloading => isReloading;
        public string CurrentWeaponName => Current.weaponName;

        void Awake()
        {
            var map = inputActions.FindActionMap("Player", throwIfNotFound: true);
            attackAction = map.FindAction("Attack", throwIfNotFound: true);
            reloadAction = map.FindAction("Reload", throwIfNotFound: true);
            nextWeaponAction = map.FindAction("Next", throwIfNotFound: true);
            prevWeaponAction = map.FindAction("Previous", throwIfNotFound: true);

            for (int i = 0; i < weapons.Length; i++)
            {
                var w = weapons[i];
                w.currentAmmo = w.magazineSize;
                if (w.weaponModel != null)
                {
                    w.restLocalPosition = w.weaponModel.localPosition;
                    w.restLocalRotation = w.weaponModel.localRotation;
                    w.weaponModel.gameObject.SetActive(i == currentIndex);
                }
            }
        }

        void OnEnable()
        {
            attackAction.Enable();
            reloadAction.Enable();
            nextWeaponAction.Enable();
            prevWeaponAction.Enable();
        }

        void OnDisable()
        {
            attackAction.Disable();
            reloadAction.Disable();
            nextWeaponAction.Disable();
            prevWeaponAction.Disable();
        }

        void Update()
        {
            if (weapons.Length > 1)
            {
                if (nextWeaponAction.WasPressedThisFrame())
                    SwitchWeapon(1);
                else if (prevWeaponAction.WasPressedThisFrame())
                    SwitchWeapon(-1);
            }

            if (isSwitching)
                return;

            if (reloadAction.WasPressedThisFrame() && !isReloading && Current.currentAmmo < Current.magazineSize)
            {
                StartReload();
                return;
            }

            if (attackAction.IsPressed() && !isReloading && Time.time >= nextFireTime)
                Fire();
        }

        void SwitchWeapon(int direction)
        {
            if (isReloading)
                return;

            int nextIndex = (currentIndex + direction + weapons.Length) % weapons.Length;
            if (nextIndex == currentIndex)
                return;

            if (weaponMotion != null)
                StopCoroutine(weaponMotion);
            weaponMotion = StartCoroutine(SwitchRoutine(nextIndex));
        }

        IEnumerator SwitchRoutine(int nextIndex)
        {
            isSwitching = true;
            var outgoing = Current;

            if (outgoing.weaponModel != null)
            {
                Vector3 downPos = outgoing.restLocalPosition + new Vector3(0f, -switchDownDistance, 0f);
                yield return Tween(outgoing.weaponModel, outgoing.weaponModel.localPosition, downPos, outgoing.weaponModel.localRotation, outgoing.restLocalRotation, switchOutTime);
                outgoing.weaponModel.gameObject.SetActive(false);
            }

            currentIndex = nextIndex;
            var incoming = Current;

            if (incoming.weaponModel != null)
            {
                Vector3 downPos = incoming.restLocalPosition + new Vector3(0f, -switchDownDistance, 0f);
                incoming.weaponModel.localPosition = downPos;
                incoming.weaponModel.localRotation = incoming.restLocalRotation;
                incoming.weaponModel.gameObject.SetActive(true);
                yield return Tween(incoming.weaponModel, downPos, incoming.restLocalPosition, incoming.restLocalRotation, incoming.restLocalRotation, switchInTime);
            }

            isSwitching = false;
            weaponMotion = null;
        }

        void Fire()
        {
            var w = Current;
            nextFireTime = Time.time + w.fireCooldown;

            if (w.currentAmmo <= 0)
            {
                Debug.Log($"[Weapon] {w.weaponName} dry fire - press R to reload.");
                return;
            }

            w.currentAmmo--;

            Vector3 origin = w.muzzlePoint != null ? w.muzzlePoint.position : transform.position;
            Vector3 direction = w.muzzlePoint != null ? w.muzzlePoint.forward : transform.forward;

            if (Physics.Raycast(origin, direction, out var hit, w.range, hitMask, QueryTriggerInteraction.Ignore))
            {
                var damageable = hit.collider.GetComponentInParent<IDamageable>();
                damageable?.ApplyDamage(w.damage, hit.point, hit.normal);
                Debug.DrawLine(origin, hit.point, Color.red, 0.5f);
            }
            else
            {
                Debug.DrawRay(origin, direction * w.range, Color.yellow, 0.5f);
            }

            Debug.Log($"[Weapon] {w.weaponName} fired - {w.currentAmmo}/{w.magazineSize}");
            PlayMotion(FireKickRoutine(w));
        }

        void StartReload()
        {
            isReloading = true;
            Debug.Log($"[Weapon] Reloading {Current.weaponName}...");
            PlayMotion(ReloadRoutine(Current));
        }

        void PlayMotion(IEnumerator routine)
        {
            if (Current.weaponModel == null)
                return;

            if (weaponMotion != null)
                StopCoroutine(weaponMotion);
            weaponMotion = StartCoroutine(routine);
        }

        IEnumerator FireKickRoutine(WeaponSlot w)
        {
            Vector3 kickPos = w.restLocalPosition + new Vector3(0f, 0f, -w.kickBackDistance);
            Quaternion kickRot = w.restLocalRotation * Quaternion.Euler(-w.kickUpAngle, 0f, 0f);

            yield return Tween(w.weaponModel, w.weaponModel.localPosition, kickPos, w.weaponModel.localRotation, kickRot, w.kickOutTime);
            yield return Tween(w.weaponModel, w.weaponModel.localPosition, w.restLocalPosition, w.weaponModel.localRotation, w.restLocalRotation, w.kickRecoverTime);

            weaponMotion = null;
        }

        IEnumerator ReloadRoutine(WeaponSlot w)
        {
            Vector3 downPos = w.restLocalPosition + new Vector3(0f, -w.reloadDropDistance, 0f);
            Quaternion downRot = w.restLocalRotation * Quaternion.Euler(w.reloadTiltAngle, 0f, 0f);

            float half = w.reloadDuration * 0.5f;
            yield return Tween(w.weaponModel, w.weaponModel.localPosition, downPos, w.weaponModel.localRotation, downRot, half);

            w.currentAmmo = w.magazineSize;
            Debug.Log($"[Weapon] Reloaded {w.weaponName} - {w.currentAmmo}/{w.magazineSize}");

            yield return Tween(w.weaponModel, w.weaponModel.localPosition, w.restLocalPosition, w.weaponModel.localRotation, w.restLocalRotation, half);

            isReloading = false;
            weaponMotion = null;
        }

        IEnumerator Tween(Transform target, Vector3 fromPos, Vector3 toPos, Quaternion fromRot, Quaternion toRot, float duration)
        {
            if (duration <= 0f)
            {
                target.localPosition = toPos;
                target.localRotation = toRot;
                yield break;
            }

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / duration);
                target.localPosition = Vector3.Lerp(fromPos, toPos, u);
                target.localRotation = Quaternion.Slerp(fromRot, toRot, u);
                yield return null;
            }

            target.localPosition = toPos;
            target.localRotation = toRot;
        }
    }

    public interface IDamageable
    {
        void ApplyDamage(float amount, Vector3 hitPoint, Vector3 hitNormal);
    }
}
