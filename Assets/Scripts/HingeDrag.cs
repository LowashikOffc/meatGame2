using UnityEngine;

public class HingeDrag : MonoBehaviour
{
    [SerializeField] private Rigidbody _target;
    [SerializeField] private float _value;
    void FixedUpdate()
    {
        _target.angularVelocity = _target.angularVelocity/_value;
    }
}
