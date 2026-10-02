using UnityEngine;

namespace RetwineMake.Player
{
    [RequireComponent(typeof(FirstPersonController))]
    public class FallRespawn : MonoBehaviour
    {
        [SerializeField] float fallYThreshold = -5f;
        [SerializeField] Vector3 respawnPosition = new Vector3(0f, 1f, -17f);

        FirstPersonController controller;

        void Awake()
        {
            controller = GetComponent<FirstPersonController>();
        }

        void Update()
        {
            if (transform.position.y < fallYThreshold)
                controller.Teleport(respawnPosition);
        }
    }
}
