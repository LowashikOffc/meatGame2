using UnityEngine;

public class DoorMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody _target;
    void FixedUpdate()
    {
        _target.angularVelocity = _target.angularVelocity/2;
    }
}
