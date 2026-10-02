using UnityEngine;

namespace RetwineMake.Player
{
    [RequireComponent(typeof(FirstPersonController))]
    public class FallRespawn : MonoBehaviour
    {
        [SerializeField] float fallYThreshold = -5f;
        [SerializeField] Vector3 respawnPosition = new Vector3(0f, 1f, -17f);

        [Header("Play area bounds (catches wandering out through a gap, not just falling)")]
        [SerializeField] Vector2 xRange = new Vector2(-10f, 53f);
        [SerializeField] Vector2 zRange = new Vector2(-30f, 11f);

        FirstPersonController controller;

        void Awake()
        {
            controller = GetComponent<FirstPersonController>();
        }

        void Update()
        {
            var pos = transform.position;
            bool outOfBounds = pos.y < fallYThreshold
                || pos.x < xRange.x || pos.x > xRange.y
                || pos.z < zRange.x || pos.z > zRange.y;

            if (outOfBounds)
                controller.Teleport(respawnPosition);
        }
    }
}
