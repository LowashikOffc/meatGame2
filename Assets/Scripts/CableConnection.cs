using UnityEngine;

public class CableConnection : MonoBehaviour
{
    [SerializeField] private Cable _main;

    [Header("false - out; true - in")]
    [SerializeField] private bool _type;
    [SerializeField] private bool _connected;

    [SerializeField] private Rigidbody _rb;
    public bool _inHands = false;
    public float _timeToNextConnect = 0;

    private void Start()
    {
        _rb = transform.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_rb.angularDrag > 1)
        {
            _inHands = true;
            _connected = false;
        }
        else _inHands = false;

        if (!_inHands)
        {
            _timeToNextConnect = 0.5f;
        }
        _timeToNextConnect -= Time.fixedDeltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_connected) return;

        var outPos = collision.transform.Find("ConnectPositionOUT");
        var inPos = collision.transform.Find("ConnectPositionIN");

        if ((outPos != null && inPos == null) || (outPos == null && inPos != null) || (outPos != null && inPos != null))
        {
            if (_timeToNextConnect > 0) return;

            Transform obj = collision.transform;

            EnergySafe safe;
            if (obj.TryGetComponent<EnergySafe>(out safe))
            {
                if (safe.GetBoolIN() == false && _type == true)
                {
                    if (inPos == null) return;
                    _main.Connect(safe, _type, inPos);
                    _connected = true;
                }
                else if (safe.GetBoolOUT() == false && _type == false)
                {
                    if (outPos == null) return;
                    _main.Connect(safe, _type, outPos);
                    _connected = true;
                }
            }
        }
    }
}
