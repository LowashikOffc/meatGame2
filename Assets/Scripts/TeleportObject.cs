using UnityEngine;

public class TeleportObject : MonoBehaviour
{
    [SerializeField] private Teleport _refScript;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.CompareTag("Player")) return;

        _refScript.TeleportToPos(transform);
    }
}
