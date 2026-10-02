using System.Collections;
using UnityEngine;

namespace RetwineMake.Enemies
{
    public class GuardWeapon : MonoBehaviour
    {
        [SerializeField] Transform weaponModel;
        [SerializeField] GameObject muzzleFlash;

        [Header("Fire kick")]
        [SerializeField] float kickBackDistance = 0.05f;
        [SerializeField] float kickUpAngle = 10f;
        [SerializeField] float kickOutTime = 0.04f;
        [SerializeField] float kickRecoverTime = 0.15f;
        [SerializeField] float muzzleFlashDuration = 0.06f;

        Vector3 restLocalPosition;
        Quaternion restLocalRotation;
        Coroutine motion;

        void Awake()
        {
            if (weaponModel != null)
            {
                restLocalPosition = weaponModel.localPosition;
                restLocalRotation = weaponModel.localRotation;
            }

            if (muzzleFlash != null)
                muzzleFlash.SetActive(false);
        }

        public void PlayFireAnimation()
        {
            if (weaponModel == null)
                return;

            if (motion != null)
                StopCoroutine(motion);
            motion = StartCoroutine(FireRoutine());
        }

        IEnumerator FireRoutine()
        {
            if (muzzleFlash != null)
                muzzleFlash.SetActive(true);

            Vector3 kickPos = restLocalPosition + new Vector3(0f, 0f, -kickBackDistance);
            Quaternion kickRot = restLocalRotation * Quaternion.Euler(-kickUpAngle, 0f, 0f);

            yield return Tween(restLocalPosition, kickPos, restLocalRotation, kickRot, kickOutTime);

            yield return new WaitForSeconds(Mathf.Max(0f, muzzleFlashDuration - kickOutTime));
            if (muzzleFlash != null)
                muzzleFlash.SetActive(false);

            yield return Tween(weaponModel.localPosition, restLocalPosition, weaponModel.localRotation, restLocalRotation, kickRecoverTime);

            motion = null;
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
}
