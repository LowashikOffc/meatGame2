using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Transform _targetPosition;
    [SerializeField] private Transform _player;
    private Rigidbody _playerRb;
    [SerializeField] private Vector3 _lastPosition;
    [SerializeField] private float _rotationSpeed;
    private Quaternion _rotation;
    private float H;
    private float V;
    private float _lastVelMagZ;
    [SerializeField] private float _sinusMoveAmplitude;
    [SerializeField] private float _sinusMoveSpeed;
    [SerializeField] private float _crouchSinusMultiply;
    [SerializeField] private float _cameraMoveSpeed;
    [SerializeField] private float _cameraSmoothing;

    private PlayerMovement _pm;

    private float _sinTime = 0;
    private float _sinSavedTime = 0;

    private bool _grounded = false;
    private bool _climbing = false;
    private bool _walking = false;
    private bool _crouching = false;
    private void Start()
    {
        _playerRb = _player.GetComponent<Rigidbody>();
        _pm = _player.GetComponent<PlayerMovement>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _pm._crouchAction += Crouch;
    }

    private float SinY()
    {
        float sin = Mathf.Sin(Time.time * 2) * 0.03f;
        return sin;
    }
    private float SinYMove()
    {
        if (!_grounded && !_climbing) return 0;
        float m;
        if (_walking) m = 0.1f * _sinusMoveAmplitude;
        else if (_crouching) m = 0.1f * _sinusMoveAmplitude * _crouchSinusMultiply;
        else m = 0;
        float sin = Mathf.Sin(_sinTime * _sinusMoveSpeed * 2) * Mathf.Lerp(m, _lastVelMagZ, Time.deltaTime * 0.1f);
        if (_crouching) sin = Mathf.Sin(_sinTime * _sinusMoveSpeed * _crouchSinusMultiply * 2) * Mathf.Lerp(m, _lastVelMagZ, Time.deltaTime * 0.1f);
        return sin;
    }
    private float SinZMove()
    {
        if (!_grounded && !_climbing) return 0;
        float m;
        if (_walking) m = 0.1f * _sinusMoveAmplitude;
        else if (_crouching) m = 0.1f * _sinusMoveAmplitude * _crouchSinusMultiply;
        else m = 0;
        float sin = Mathf.Sin(_sinTime * _sinusMoveSpeed) * Mathf.Lerp(m, _lastVelMagZ, Time.deltaTime * 0.1f);
        if (_crouching) sin = Mathf.Sin(_sinTime * _sinusMoveSpeed * _crouchSinusMultiply) * Mathf.Lerp(m, _lastVelMagZ, Time.deltaTime * 0.1f);
        _lastVelMagZ = m;
        return sin;
    }
    private void Crouch(bool state)
    {
        _crouching = state;
        //Debug.Log(state);
    }
    
    private float _currentStrafeAngle;
    private float _targetStrafeAngle;
    private float StrafeRotation()
    {
        Vector2 moveInput = InputReceiver.Instance.MoveInput;

        if (moveInput != Vector2.zero)
        {
            _targetStrafeAngle = -moveInput.x * 1f;
        }
        else
        {
            _targetStrafeAngle = 0f;
        }

        _currentStrafeAngle = Mathf.Lerp(_currentStrafeAngle, _targetStrafeAngle, Time.deltaTime * 8f);

        return _currentStrafeAngle;
    }

    private void LateUpdate()
    {
        _grounded = _pm.GetGrounded();
        _climbing = _pm.GetClimbing();
        if (InputReceiver.Instance.MoveInput.magnitude > 0)
        {
            _walking = true;
            _sinSavedTime = _sinTime;
        }
        else
        {
            _walking = false;
            _sinTime = _sinSavedTime;
        }

        V += Input.GetAxis("Mouse X") * _rotationSpeed;
        H -= Input.GetAxis("Mouse Y") * _rotationSpeed;

        H = Mathf.Clamp(H, -80, 80);

        _rotation = Quaternion.Euler(H, V, 0);

        _sinTime += Time.deltaTime;
        _playerRb.MoveRotation(Quaternion.Euler(0, V, 0));
        transform.rotation = Quaternion.Slerp(transform.rotation, _rotation * Quaternion.Euler(0,0, StrafeRotation()), Time.deltaTime * (100 / _cameraSmoothing));
        Vector3 newPos = _targetPosition.position + Vector3.up * SinY() + Vector3.up * SinYMove() + transform.right * SinZMove();
        Vector3 targetPosition = newPos;// Vector3.Lerp(transform.position, newPos, Time.deltaTime * _cameraMoveSpeed);
       
        transform.position = targetPosition;
        _lastPosition = _targetPosition.position;
    }
}
