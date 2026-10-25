using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Transform _targetPosition;
    [SerializeField] private Transform _player;
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private float _sinusMoveAmplitude;
    [SerializeField] private float _sinusMoveSpeed;
    [SerializeField] private float _crouchSinusMultiply;
    [SerializeField] private float _cameraSmoothing;

    private Rigidbody _playerRb;
    private PlayerMovement _pm;
    private Quaternion _rotation;
    private float H, V;
    private float _sinTime;
    private float _bobWeight;          // 0..1 Ч плавное включение/выключение покачивани€
    private float _currentStrafeAngle;
    private float _targetStrafeAngle;
    private bool _grounded, _climbing, _walking, _crouching;

    private void Start()
    {
        _playerRb = _player.GetComponent<Rigidbody>();
        _pm = _player.GetComponent<PlayerMovement>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _pm._crouchAction += state => _crouching = state;
    }

    private void LateUpdate()
    {
        _grounded = _pm.GetGrounded();
        _climbing = _pm.GetClimbing();
        _walking = InputReceiver.Instance.MoveInput.magnitude > 0;

        V += Input.GetAxis("Mouse X") * _rotationSpeed;
        H = Mathf.Clamp(H - Input.GetAxis("Mouse Y") * _rotationSpeed, -80, 80);
        _rotation = Quaternion.Euler(H, V, 0);

        // ‘аза крутитс€ всегда Ч синус непрерывный, скачков фазы нет
        _sinTime += Time.deltaTime;

        // ѕлавно включаем/выключаем покачивание (фикс дЄргань€)
        float targetBob = (_walking && (_grounded || _climbing)) ? 1f : 0f;
        _bobWeight = Mathf.MoveTowards(_bobWeight, targetBob, Time.deltaTime * 6f);

        _playerRb.MoveRotation(Quaternion.Euler(0, V, 0));
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            _rotation * Quaternion.Euler(RotY(), 0, StrafeRotation()),
            Time.deltaTime * (100f / _cameraSmoothing));

        float bobY = SinY() + SinMove(_bobWeight, 2f);
        float bobX = SinMove(_bobWeight, 1f);

        transform.position = _targetPosition.position
                             + Vector3.up * bobY
                             + transform.right * bobX;
    }

    // ѕокачивание по вертикали/горизонтали. mult Ч множитель частоты (2 дл€ Y, 1 дл€ Z)
    private float SinMove(float weight, float freqMult)
    {
        if (weight <= 0f) return 0f;
        float amp = 0.1f * _sinusMoveAmplitude * (_crouching ? _crouchSinusMultiply : 1f);
        float freq = _sinusMoveSpeed * (_crouching ? _crouchSinusMultiply : 1f) * freqMult;
        return Mathf.Sin(_sinTime * freq) * amp * weight;
    }

    private float SinY() => Mathf.Sin(Time.time * 2f) * 0.03f;

    private float RotY() => -Mathf.Lerp(0f, 3f * _playerRb.velocity.y, 0.1f);

    private float StrafeRotation()
    {
        Vector2 moveInput = InputReceiver.Instance.MoveInput;
        _targetStrafeAngle = moveInput != Vector2.zero ? -moveInput.x : 0f;
        _currentStrafeAngle = Mathf.Lerp(_currentStrafeAngle, _targetStrafeAngle, Time.deltaTime * 8f);
        return _currentStrafeAngle;
    }
}