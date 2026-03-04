using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new(0f, 0f, -10f);
    [SerializeField] private float followSpeed = 5f;

    private Transform target;

    private void Start()
    {
        // Попробуем найти игрока при старте
        TrySetTarget(GlobalController.instance?.CurrentPlayer?.transform);

        // Подпишемся на событие появления игрока
        if (GlobalController.instance != null)
        {
            GlobalController.instance.OnPlayerSpawned += HandlePlayerSpawned;
        }
    }

    private void OnDestroy()
    {
        if (GlobalController.instance != null)
        {
            GlobalController.instance.OnPlayerSpawned -= HandlePlayerSpawned;
        }
    }

    private void HandlePlayerSpawned(GameObject player)
    {
        TrySetTarget(player.transform);
    }

    private void TrySetTarget(Transform newTarget)
    {
        if (newTarget != null)
        {
            target = newTarget;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
    }
}
