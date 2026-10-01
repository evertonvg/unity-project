using UnityEngine;
using RetwineMake.Combat;
using RetwineMake.Gameplay;

namespace RetwineMake.Enemies
{
    [RequireComponent(typeof(Health))]
    public class Civilian : MonoBehaviour
    {
        Health health;

        void Awake()
        {
            health = GetComponent<Health>();
        }

        void OnEnable() => health.OnDeath += HandleDeath;
        void OnDisable() => health.OnDeath -= HandleDeath;

        void HandleDeath()
        {
            ObjectiveManager.Instance?.FailObjective("avoid_casualties");
        }
    }
}
