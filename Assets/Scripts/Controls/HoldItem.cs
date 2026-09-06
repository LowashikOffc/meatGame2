using UnityEngine;

public class HoldItem : MonoBehaviour
{
    private GameObject _itemToHold;
    private GameObject _player;
    private Rigidbody _rb;
    private Camera _camera;
    [SerializeField] private float _holdDistance;
    [SerializeField] private float _pickupDistance;
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private string _objectTag;

    [SerializeField] private Rigidbody _referenceRigidbody;

    private ObjectConfig _configuration;
    private PlayerMovement _pm;

    private void Start()
    {
        _player = gameObject;
        _pm = _player.GetComponent<PlayerMovement>();
        _camera = Camera.main;
        InputReceiver.Instance._interact += Pickup;
        InputReceiver.Instance._dropAction += ResetItem;
    }

    public Transform GetItemToHold()
    {
        if (_itemToHold == null) return null;
        return _itemToHold.transform;
    }

    public void SetItem(GameObject obj)
    {
        Transform groundObject = _pm.GetGroundObject();
        if (groundObject == obj.transform && groundObject.tag == _objectTag) return;
        //Debug.Log(groundObject == obj.transform);
        if (!obj && obj == _player) return;
        _itemToHold = obj;
        _rb = obj.GetComponent<Rigidbody>();
        _rb.interpolation = _referenceRigidbody.interpolation;
        _rb.angularDrag = _referenceRigidbody.angularDrag;
        _rb.drag = 5;
        _rb.isKinematic = false;
        obj.TryGetComponent<ObjectConfig>(out _configuration);
        //_itemToHold.GetComponent<Collider>().excludeLayers = _targetLayer;
    }
    public void ResetItem()
    {
        if (!_rb || !_itemToHold) return;
        //s_itemToHold.GetComponent<Collider>().excludeLayers = 0;
        _itemToHold = null;
        _rb.angularDrag = 0;
        _rb.drag = 0;
        _configuration = null;
        //_rb.velocity = Vector3.ClampMagnitude(_rb.velocity, 1);
        _rb = null;
    }

    private void Pickup()
    {
        RaycastHit hit;
        if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out hit, _pickupDistance, ~_targetLayer))
        {
            if (hit.transform.tag == _objectTag)
            {
                if (!hit.collider.gameObject.GetComponent<Rigidbody>())
                {
                    Rigidbody newRB = hit.collider.gameObject.AddComponent<Rigidbody>();
                }
                SetItem(hit.collider.gameObject);
            }
        }
    }

    

    private void FixedUpdate()
    {
        if (!_rb) return;
        Vector3 targetPosition = _camera.transform.position + _camera.transform.forward * _holdDistance;

        Vector3 newPosition = (targetPosition - _rb.position) * 10;

        Quaternion targetRotation = Quaternion.LookRotation(_camera.transform.forward);
        Quaternion deltaRotation = targetRotation * Quaternion.Inverse(_rb.rotation);


        if (_configuration)
        {
            deltaRotation = targetRotation * Quaternion.Euler(_configuration.config.RotationOffset) * Quaternion.Inverse(_rb.rotation);
            newPosition = newPosition + _configuration.config.PositionOffset;
        }

        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;

        if (!_rb.isKinematic) _rb.angularVelocity = axis * (angle * Mathf.Deg2Rad * 150 / _rb.mass);
        if (!_rb.isKinematic) _rb.velocity = Vector3.ClampMagnitude(newPosition, 100 / _rb.mass);
    }
}
