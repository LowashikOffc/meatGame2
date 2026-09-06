using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Transform _camera;
    private Rigidbody _rigidbody;

    private Vector3 _lastPos;
    private Vector3 _startScale;
    [SerializeField] private Vector3 _crouchScale;

    private float _walkedDist = 0;
    [SerializeField] private float _climbToSound;
    private float _climbingDist = 0;
    [SerializeField] private float _walkToSound;

    private bool _crouching = false;
    private bool _walking = false;

    private bool _climbing = false;
    private float _climbingMinY;
    private float _climbingMaxY;
    private Vector3 _downPos;
    private Vector3 _upPos;

    [SerializeField] private float _radiusMultiply;
    [SerializeField] private float _maximumWalkAngle;
    private bool _grounded = false;
    private Transform _currentGround;
    private Vector3 _lastGroundPosition;
    private Vector3 _groundDelta;
    private float _waitForGroundCheck = 0;
    private Materials _groundMaterial;
    private HoldItem _holdItem;
    private RaycastHit hit;

    [SerializeField] private float _jumpForce;
    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _crouchSpeed;

    public event Action<bool> _crouchAction;
    private void Start()
    {
        _holdItem = GetComponent<HoldItem>();
        _startScale = transform.localScale;
        _rigidbody = GetComponent<Rigidbody>();
        _camera = Camera.main.transform;
        InputReceiver.Instance._jumpAction += Jump;
        InputReceiver.Instance._crouchAction += Crouch;
    }

    private void OnDestroy()
    {
        InputReceiver.Instance._jumpAction -= Jump;
        InputReceiver.Instance._crouchAction -= Crouch;
    }

    public bool GetGrounded()
    {
        return _grounded;
    }
    public bool GetClimbing()
    {
        return _climbing;
    }

    private void Jump()
    {
        if (GetGroundAngle() > _maximumWalkAngle) return;
        _climbing = false;
        _climbingMinY = 0;
        _climbingMaxY = 0;
        _downPos = Vector3.zero;
        _upPos = Vector3.zero;

        float sphereRadius = transform.GetComponent<CapsuleCollider>().radius * Mathf.Clamp(_radiusMultiply, 0, 1); // Радиус сферы
        float maxDistance = transform.localScale.y - Mathf.Clamp(_radiusMultiply, 0, 1) / 4;

        if (Physics.SphereCast(transform.position, sphereRadius, Vector3.down, out RaycastHit hit, maxDistance))
        {
            SoundService.Instance.PlaySound(SoundBase.Jump, transform.position, 0.7f);
            //Debug.Log("jump");
            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
            _rigidbody.AddForce(Vector3.up * _jumpForce * 100);
            _grounded = false;
            _walkedDist = _walkToSound;
            _waitForGroundCheck = 0.2f;
        }
    }

    private void OnDrawGizmos()
    {
        float sphereRadius = transform.GetComponent<CapsuleCollider>().radius * Mathf.Clamp(_radiusMultiply, 0, 1); // Радиус сферы
        float maxDistance = transform.localScale.y - Mathf.Clamp(_radiusMultiply, 0, 1) / 4;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, sphereRadius);

        // Рисуем сферу в конечной позиции
        Vector3 endPosition = transform.position + Vector3.down * maxDistance;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(endPosition, sphereRadius);

        // Рисуем линию между ними
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, endPosition);
    }

    private bool CheckForStand()
    {
        if (Physics.Raycast(transform.position, Vector3.up, _startScale.y) == true)
        {
            return false;
        }
        return true;
    }

    private void Crouch(bool state)
    {
        bool canStand = CheckForStand();
        //Debug.Log($"Can stand up = {canStand}");
        if (canStand == false) state = !canStand;
        if (state == true)
        {
            //if (canStand == true)
            //{
                transform.localScale = _crouchScale;
                if (_crouching == false && _grounded) transform.position = transform.position - Vector3.up * _startScale.y / 2;
                _crouching = true;
            //}
        }
        else if (state == false)
        {
            if (canStand == true)
            {
                transform.localScale = _startScale;
                if (_grounded) transform.position = transform.position + Vector3.up * _startScale.y / 2;
                _crouching = false;
            }
        }
        _crouchAction?.Invoke(_crouching);
    }

    private void WalkSounds()
    {
        if (_grounded == false && _climbing == true)
        {
            float dist = Mathf.Abs(Vector3.Distance(new Vector3(0, _lastPos.y, 0), new Vector3(0, _rigidbody.position.y, 0)));
            if (_climbing) _climbingDist += dist;
            //Debug.Log("Climbed: " + _climbingDist.ToString());
            if (_climbingDist >= _climbToSound)
            {
                _climbingDist = 0;
                int rand = UnityEngine.Random.Range(1, 5);
                //Debug.Log("New sound: " + rand.ToString());
                if (rand == 1) SoundService.Instance.PlaySound(SoundBase.Climb1, transform.position, 0.2f);
                if (rand == 2) SoundService.Instance.PlaySound(SoundBase.Climb2, transform.position, 0.2f);
                if (rand == 3) SoundService.Instance.PlaySound(SoundBase.Climb3, transform.position, 0.2f);
                if (rand == 4) SoundService.Instance.PlaySound(SoundBase.Climb4, transform.position, 0.2f);
            }
        }
        else if (_grounded == true && _climbing == false)
        {
            _walkedDist += Vector3.Distance(new Vector3(_lastPos.x, 0, _lastPos.z), new Vector3(_rigidbody.position.x, 0, _rigidbody.position.z));

            if (_walkedDist >= _walkToSound)
            {
                _walkedDist = 0;
                int rand = UnityEngine.Random.Range(1, 5);
                //Debug.Log(_groundMaterial);
                if (_groundMaterial == Materials.Mud)
                {
                    if (rand == 1) SoundService.Instance.PlaySound(SoundBase.MudStep1, transform.position, 0.2f);
                    if (rand == 2) SoundService.Instance.PlaySound(SoundBase.MudStep2, transform.position, 0.2f);
                    if (rand == 3) SoundService.Instance.PlaySound(SoundBase.MudStep3, transform.position, 0.2f);
                    if (rand == 4) SoundService.Instance.PlaySound(SoundBase.MudStep4, transform.position, 0.2f);
                }
                else if (_groundMaterial == Materials.Metal)
                {
                    if (rand == 1) SoundService.Instance.PlaySound(SoundBase.MetalStep1, transform.position, 0.2f);
                    if (rand == 2) SoundService.Instance.PlaySound(SoundBase.MetalStep2, transform.position, 0.2f);
                    if (rand == 3) SoundService.Instance.PlaySound(SoundBase.MetalStep3, transform.position, 0.2f);
                    if (rand == 4) SoundService.Instance.PlaySound(SoundBase.MetalStep4, transform.position, 0.2f);
                }
                else if (_groundMaterial == Materials.Wood)
                {
                    if (rand == 1) SoundService.Instance.PlaySound(SoundBase.WoodStep1, transform.position, 0.2f);
                    if (rand == 2) SoundService.Instance.PlaySound(SoundBase.WoodStep2, transform.position, 0.2f);
                    if (rand == 3) SoundService.Instance.PlaySound(SoundBase.WoodStep3, transform.position, 0.2f);
                    if (rand == 4) SoundService.Instance.PlaySound(SoundBase.WoodStep4, transform.position, 0.2f);
                }
                else
                {
                    if (rand == 1) SoundService.Instance.PlaySound(SoundBase.Step1, transform.position, 0.2f);
                    if (rand == 2) SoundService.Instance.PlaySound(SoundBase.Step2, transform.position, 0.2f);
                    if (rand == 3) SoundService.Instance.PlaySound(SoundBase.Step3, transform.position, 0.2f);
                    if (rand == 4) SoundService.Instance.PlaySound(SoundBase.Step4, transform.position, 0.2f);
                }
            }
        }
    }

    private void AddForce(Vector3 newPosition, float multiply)
    {
        _rigidbody.AddForce(newPosition, ForceMode.VelocityChange);
        _rigidbody.velocity = new Vector3(_rigidbody.velocity.x * multiply, _rigidbody.velocity.y, _rigidbody.velocity.z * multiply);
    }

    public void SetClimbing(bool state, Vector3 position, Vector3 min, Vector3 max, Vector3 downPos, Vector3 upPos, string name)
    {
        float minY = min.y;
        float maxY = max.y;
        float playerY = transform.position.y;

        _climbingMinY = minY;
        _climbingMaxY = maxY;
        _climbing = state;
        if (playerY < minY + 0.0001f) transform.position = min;
        else if (playerY > maxY + 0.0001f) transform.position = max;
        else if (playerY > minY && playerY < maxY)
        {
            if (_grounded)
            {
                transform.position = position + Vector3.up * 0.35f;
            }
            else transform.position = position;
        }
        _downPos = downPos;
        _upPos = upPos;
    }

    private void CalculateClimb(float playerY, float minY, float maxY, Vector3 downPosition, Vector3 upPosition)
    {
        //Debug.Log($"down position: {downPosition}, up positiob: {upPosition}");
        if (downPosition == Vector3.zero && upPosition == Vector3.zero) return;
        if (playerY < minY || playerY > maxY)
        {
            _climbing = false;
            _climbingMinY = 0;
            _climbingMaxY = 0;
            if (playerY < minY) transform.position = downPosition + Vector3.up * transform.localScale.y;
            else if (playerY > maxY)
            {
                transform.position = upPosition + Vector3.up * transform.localScale.y;
                Crouch(false);
            }
            _downPos = Vector3.zero;
            _upPos = Vector3.zero;
        }
        else _climbing = true;
    }

    public bool CheckForGround()
    {
        float sphereRadius = transform.GetComponent<CapsuleCollider>().radius * 0.99f; // Радиус сферы
        float maxDistance = transform.localScale.y * 1f;

        if (Physics.SphereCast(transform.position, sphereRadius, Vector3.down, out hit, maxDistance))
        {
            return true;
        }
        else return false;
    }

    public Transform GetGroundObject()
    {
        return hit.transform;
    }

    private float GetGroundAngle()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 2f))
        {
            // Угол между нормалью поверхности и вертикалью (всегда 0-90)
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            return angle;
        }

        return 0f;
    }

    private void FixedUpdate()
    {
        CalculateClimb(transform.position.y, _climbingMinY, _climbingMaxY, _downPos, _upPos);
        if (CheckForGround())
        {
            _climbing = false;
            if (_waitForGroundCheck <= 0) _grounded = true;
            _waitForGroundCheck -= Time.deltaTime;
            if (_currentGround != hit.transform)
            {
                _currentGround = GetGroundObject();
                //Debug.Log(_currentGround);
                if (_currentGround.GetComponent<GroundSoundConfig>()) _groundMaterial = _currentGround.GetComponent<GroundSoundConfig>().Material;
                else _groundMaterial = Materials.None;
                _lastGroundPosition = _currentGround.position;
                _groundDelta = Vector3.zero;
                Transform holding = _holdItem.GetItemToHold();
                if (holding != null) {
                    if (_currentGround.tag == "Carryable" && holding == _currentGround)
                    {
                        _holdItem.ResetItem();

                    }
                }
            }
            _grounded = true;
        }
        else
        {
            _currentGround = null;
            _groundDelta = Vector3.zero;
            _grounded = false;
        }
        Vector2 moveInput = InputReceiver.Instance.MoveInput;
        Vector3 moveDirection = (_camera.right * moveInput.x + _camera.forward * moveInput.y);
        if (!_climbing)
        {
            _rigidbody.drag = 0;
            _rigidbody.isKinematic = false;
            moveDirection.y = 0;
            moveDirection.Normalize();
            if (moveDirection != Vector3.zero) _walking = true;
            else _walking = false;
            if (_grounded)
            {
                _groundDelta = _lastGroundPosition - _currentGround.position;
                transform.position -= _groundDelta;
                //Debug.Log(_groundDelta);

                Vector3 newPosition = moveDirection * _movementSpeed;
                if (_walking)
                {
                    if (GetGroundAngle() > _maximumWalkAngle) return;
                    WalkSounds();
                    if (_crouching)
                    {
                        newPosition = moveDirection * _crouchSpeed;
                    }
                    AddForce(newPosition, 0.8f);
                }
                else
                {
                    if (!_crouching)
                    {
                        AddForce(newPosition, 0.8f);
                    }
                }
            }
            else if (!_grounded)
            {
                Vector3 newPosition = moveDirection * _movementSpeed * 0.08f;
                AddForce(newPosition, 0.98f);
            }
        }
        else
        {
            _rigidbody.isKinematic = true;
            float y = Mathf.Clamp(moveDirection.y, -1, 1);
            if (y < 0) y = -1;
            else if (y > 0) y = 1;
            moveDirection.y = y;
            moveDirection.x = 0;
            moveDirection.z = 0;
            moveDirection.Normalize();
            Vector3 newPosition = moveDirection * _movementSpeed * 0.05f;
            if (CheckForStand() == false && newPosition.y > 0) return;
            _rigidbody.position += newPosition;
            WalkSounds();
        }
        if (_currentGround != null) _lastGroundPosition = _currentGround.position;
        _lastPos = _rigidbody.position;
    }
}
