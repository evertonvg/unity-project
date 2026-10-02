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

        [Header("Reload hands & magazine")]
        public Transform magazine;
        public Transform gripHand;
        public Transform supportHand;
        [Tooltip("Rifle-style angled mag rock in/out (AK) vs a straight pull for a pistol")]
        public bool angledMagazine;

        [NonSerialized] public int currentAmmo;
        [NonSerialized] public Vector3 restLocalPosition;
        [NonSerialized] public Quaternion restLocalRotation;
        [NonSerialized] public Vector3 magazineRestLocalPosition;
        [NonSerialized] public Quaternion magazineRestLocalRotation;
        [NonSerialized] public Vector3 supportHandRestLocalPosition;
        [NonSerialized] public Quaternion supportHandRestLocalRotation;
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
                if (w.magazine != null)
                {
                    w.magazineRestLocalPosition = w.magazine.localPosition;
                    w.magazineRestLocalRotation = w.magazine.localRotation;
                }
                if (w.supportHand != null)
                {
                    w.supportHandRestLocalPosition = w.supportHand.localPosition;
                    w.supportHandRestLocalRotation = w.supportHand.localRotation;
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
            // Phase split: dip down -> eject old mag -> brief reach -> seat new mag -> rise back up.
            float tDown = w.reloadDuration * 0.12f;
            float tEject = w.reloadDuration * 0.28f;
            float tReach = w.reloadDuration * 0.14f;
            float tSeat = w.reloadDuration * 0.32f;
            float tUp = w.reloadDuration - tDown - tEject - tReach - tSeat;

            Vector3 downPos = w.restLocalPosition + new Vector3(0f, -w.reloadDropDistance, 0f);
            Quaternion downRot = w.restLocalRotation * Quaternion.Euler(w.reloadTiltAngle, 0f, 0f);

            yield return StartParallel(
                Tween(w.weaponModel, w.weaponModel.localPosition, downPos, w.weaponModel.localRotation, downRot, tDown),
                MoveHandTo(w, WellLocalPos(w), WellLocalRot(w, false), tDown));

            if (w.magazine != null)
            {
                Vector3 ejectPos = w.magazineRestLocalPosition + (w.angledMagazine
                    ? new Vector3(0.02f, -0.2f, -0.08f)
                    : new Vector3(0f, -0.22f, 0f));
                Quaternion ejectRot = w.magazineRestLocalRotation * Quaternion.Euler(w.angledMagazine ? -45f : -10f, 0f, w.angledMagazine ? 25f : 0f);
                yield return Tween(w.magazine, w.magazine.localPosition, ejectPos, w.magazine.localRotation, ejectRot, tEject);
                w.magazine.gameObject.SetActive(false);
            }
            else
            {
                yield return new WaitForSeconds(tEject);
            }

            yield return new WaitForSeconds(tReach);

            if (w.magazine != null)
            {
                Vector3 insertStartPos = w.magazineRestLocalPosition + (w.angledMagazine
                    ? new Vector3(-0.02f, -0.2f, 0.1f)
                    : new Vector3(0f, -0.22f, 0f));
                Quaternion insertStartRot = w.magazineRestLocalRotation * Quaternion.Euler(w.angledMagazine ? 35f : -10f, 0f, w.angledMagazine ? -20f : 0f);
                w.magazine.localPosition = insertStartPos;
                w.magazine.localRotation = insertStartRot;
                w.magazine.gameObject.SetActive(true);

                yield return StartParallel(
                    Tween(w.magazine, insertStartPos, w.magazineRestLocalPosition, insertStartRot, w.magazineRestLocalRotation, tSeat),
                    MoveHandTo(w, WellLocalPos(w), WellLocalRot(w, false), tSeat));
            }
            else
            {
                yield return new WaitForSeconds(tSeat);
            }

            w.currentAmmo = w.magazineSize;
            Debug.Log($"[Weapon] Reloaded {w.weaponName} - {w.currentAmmo}/{w.magazineSize}");

            yield return StartParallel(
                Tween(w.weaponModel, w.weaponModel.localPosition, w.restLocalPosition, w.weaponModel.localRotation, w.restLocalRotation, tUp),
                MoveHandHome(w, tUp));

            isReloading = false;
            weaponMotion = null;
        }

        Vector3 WellLocalPos(WeaponSlot w) => w.magazine != null ? w.magazineRestLocalPosition + new Vector3(0f, -0.05f, 0f) : w.supportHandRestLocalPosition;
        Quaternion WellLocalRot(WeaponSlot w, bool home) => home ? w.supportHandRestLocalRotation : w.supportHandRestLocalRotation * Quaternion.Euler(20f, 0f, 0f);

        IEnumerator MoveHandTo(WeaponSlot w, Vector3 pos, Quaternion rot, float duration)
        {
            if (w.supportHand == null) { yield return new WaitForSeconds(duration); yield break; }
            yield return Tween(w.supportHand, w.supportHand.localPosition, pos, w.supportHand.localRotation, rot, duration);
        }

        IEnumerator MoveHandHome(WeaponSlot w, float duration)
        {
            if (w.supportHand == null) { yield return new WaitForSeconds(duration); yield break; }
            yield return Tween(w.supportHand, w.supportHand.localPosition, w.supportHandRestLocalPosition, w.supportHand.localRotation, w.supportHandRestLocalRotation, duration);
        }

        IEnumerator StartParallel(IEnumerator a, IEnumerator b)
        {
            bool aDone = false, bDone = false;
            StartCoroutine(RunAndFlag(a, () => aDone = true));
            StartCoroutine(RunAndFlag(b, () => bDone = true));
            while (!aDone || !bDone)
                yield return null;
        }

        IEnumerator RunAndFlag(IEnumerator routine, Action onDone)
        {
            yield return StartCoroutine(routine);
            onDone();
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
