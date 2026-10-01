using UnityEngine;

namespace RetwineMake.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class EscapeZone : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[EscapeZone] OnTriggerEnter: {other.name}");
            var inv = other.GetComponentInParent<PlayerInventory>();
            if (inv == null)
                return;

            if (inv.HasBriefcase && inv.HasEquipment)
            {
                ObjectiveManager.Instance?.CompleteObjective("escape");
                ObjectiveManager.Instance?.CompleteMission();
            }
            else
            {
                Debug.Log("[EscapeZone] Objectives incomplete - can't leave yet.");
            }
        }
    }
}
