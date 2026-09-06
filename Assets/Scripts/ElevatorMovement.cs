using UnityEngine;

[System.Serializable]
public class Point
{
    public Transform _pos;
    public Transform _button;
}

public class ElevatorMovement : MonoBehaviour
{
    [SerializeField] private int _pointIndex = 0;
    [SerializeField] private Point[] _points;

    [SerializeField] private Camera _camera;
    [SerializeField] private LayerMask _targetLayer;

    [SerializeField] private Transform _upElevatorB;
    [SerializeField] private Transform _downElevatorB;

    [SerializeField] private Transform _circle;

    [SerializeField] private Rigidbody _base;
    [SerializeField] private Transform _buttons;
    private Vector3 _lastBasePos = Vector3.zero;

    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _distance;
    //[SerializeField] float multiplyDist = 1;

    [SerializeField] private AudioSource _move;
    [SerializeField] private AudioSource _stop;

    [SerializeField] private Vector3 _targetPosition;

    private bool _isMoving = false;
    private bool _enabled = false;
    private bool _lastEnabled;

    [SerializeField] private EnergySafe _safe;
    [SerializeField] private float _energy;
    [SerializeField] private float _energyUsing;

    private float _lastY;
    private float _rotation;
    void Start()
    {
        _targetPosition = transform.position;
        _camera = Camera.main;
        InputReceiver.Instance._interact += Call;
    }

    private void Up(int newIndex)
    {
        if (_energy <= 0) return;
        int maxIndex = _points.Length - 1;

        if (newIndex == -1) _pointIndex++;

        if (_pointIndex >= maxIndex) _pointIndex = maxIndex;
        Debug.Log("Up: " + _pointIndex);
        Move();
    }

    private void Down(int newIndex)
    {
        if (_energy <= 0) return;
        if (newIndex == -1) _pointIndex--;

        if (_pointIndex <= 0) _pointIndex = 0;
        Debug.Log("Down: "+_pointIndex);
        Move();
    }

    private void Move()
    {
        float index = 0;
        foreach (var point in _points)
        {
            if (index == _pointIndex)
            {
                CallElevator(point._pos.position);
                break;
            }
            index++;
        }
    }

    private void Call()
    {
        if (_isMoving == true) return;
        if (_energy <= 0) return;
        RaycastHit hit;
        if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out hit, _distance, ~_targetLayer))
        {
            int newIndex = 0;
            foreach (var p in _points)
            {
                if (hit.transform == p._button)
                {
                    if (_pointIndex < newIndex)
                    {
                        Debug.Log($"Selected Index: {newIndex}, Button: {p._button.name}");
                        _pointIndex = newIndex;
                        Move();
                        break;
                    }
                    else if (_pointIndex > newIndex)
                    {
                        Debug.Log($"Selected Index: {newIndex}, Button: {p._button.name}");
                        _pointIndex = newIndex;
                        Move();
                        break;
                    }
                }
                newIndex++;
            }
            if (hit.transform == _upElevatorB)
            {
                Up(-1);
            }
            else if (hit.transform == _downElevatorB)
            {
                Down(-1);
            }
        }
    }

    private void SoundAction()
    {
        if (_enabled == true)
        {
            _stop.Stop();
            _move.time = 0;
            _move.Play();
        }
        else
        {

            _move.Stop();
            _stop.time = 0;
            _stop.Play();
        }
    }

    public string GetParameter(string name)
    {
        if (name == "_energy") return _energy.ToString();
        return "";
    }

    private void FixedUpdate()
    {
        if (_safe != null)
        {
            _energy += _safe.OutputEnergy();

        }
        if (_energy >= _energyUsing && _enabled) _energy -= _energyUsing;
    }

    void Update()
    {
        if (Vector3.Distance(_targetPosition, _base.position) > 0.01f)
        {
            if (_energy <= 0) return;
            _enabled = true;
            if (_enabled != _lastEnabled) SoundAction();
        }
        else
        {
            _enabled = false;
            if (_enabled != _lastEnabled) SoundAction();
        }
        _lastEnabled = _enabled;
        if (_energy < _energyUsing && _enabled) return;
        float speed = _moveSpeed;
        _base.position = Vector3.MoveTowards(_base.position, _targetPosition, Time.deltaTime * speed);
        _buttons.position = Vector3.MoveTowards(_buttons.position, _targetPosition, Time.deltaTime * speed);
        float baseDelta = Vector3.Distance(_lastBasePos, _base.position);
        float YDelta = _base.position.y - _lastY;
        _rotation = _rotation + YDelta * 50;

        _circle.transform.rotation = Quaternion.Euler(_circle.transform.rotation.x, _circle.transform.rotation.y, _rotation);
        if (baseDelta > 0)
        {
            _isMoving = true;
        }
        else _isMoving = false;
        _lastBasePos = _base.position;
        _lastY = _base.transform.position.y;
    }

    private void CallElevator(Vector3 pos)
    {
        _targetPosition = pos;
    }
}
