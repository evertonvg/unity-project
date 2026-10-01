using UnityEngine;

namespace RetwineMake.Gameplay
{
    public class PlayerInventory : MonoBehaviour
    {
        public bool HasEquipment { get; private set; }
        public bool HasSecurityPass { get; private set; }
        public bool HasBriefcase { get; private set; }

        public void GrantEquipment() => HasEquipment = true;
        public void GrantSecurityPass() => HasSecurityPass = true;
        public void GrantBriefcase() => HasBriefcase = true;
    }
}
