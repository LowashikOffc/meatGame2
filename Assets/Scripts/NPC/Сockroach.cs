using System.Collections;
using UnityEngine;

public class Сockroach : MonoBehaviour
{
    private float _targetSpeed;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _runSpeed;
    [SerializeField] private float _maxMoveDistance = 3f;
    [SerializeField] private float _rayLength = 2f;
    [SerializeField] private float _minDistanceFromWall = 0.5f;
    [SerializeField] private LayerMask _wallLayer = -1;
    [SerializeField] private float _sphereRadius = 0.5f;

    [SerializeField] private string _targetObjectName = "TargetObject"; // Имя объекта для поиска
    [SerializeField] private float _searchRadius = 5f; // Радиус поиска
    [SerializeField] private float _approachDistance = 0.5f; // Дистанция остановки у цели
    private Transform _nearestTarget; // Ближайший найденный объект

    private Vector3 _targetPosition;
    private Transform _player;
    private bool _isStuck = false;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        FindNewTarget();
        StartCoroutine(WalkTimer());
    }

    private void Walk()
    {
        // Проверяем, не застряли ли
        if (IsStuck())
        {
            _isStuck = true;
            FindNewTarget();
            return;
        }

        FindNearestTargetObject();
        float distToPlayer = Vector3.Distance(_player.position, transform.position);

            // Движение к цели
        Vector3 direction = (_targetPosition - transform.position);

        if (_nearestTarget != null && distToPlayer > 4f)
        {
            direction = _nearestTarget.position - transform.position;
        }
        direction.y = 0;
        if (direction.magnitude > 0.1f)
        {
            // Поворачиваемся
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction.normalized),
                Time.deltaTime * 50f
            );

            // Двигаемся
            Vector3 moveStep = direction.normalized * _targetSpeed * Time.deltaTime;

            // Проверяем стену перед движением
            if (!WillHitWall(moveStep))
            {
                transform.position += moveStep;
                _isStuck = false;
            }
            else
            {
                // Если стена - ищем новый путь
                FindNewTarget();
            }
        }
        else
        {
            // Достигли цели - ищем новую
            FindNewTarget();
        }
    }

    private void FindNearestTargetObject()
    {
        // Находим все объекты с заданным именем
        GameObject[] objects = GameObject.FindObjectsOfType<GameObject>();
        Transform nearest = null;
        float minDistance = float.MaxValue;

        foreach (GameObject obj in objects)
        {
            if (obj.name == _targetObjectName)
            {
                float distance = Vector3.Distance(transform.position, obj.transform.position);
                // Проверяем, что объект в радиусе поиска и он ближе текущего
                if (distance <= _searchRadius && distance < minDistance)
                {
                    minDistance = distance;
                    nearest = obj.transform;
                }
            }
        }

        _nearestTarget = nearest;
    }
    private void FindNewTarget()
    {
        // Проверяем расстояние до игрока
        float distToPlayer = Vector3.Distance(_player.position, transform.position);

        if (distToPlayer < 4f)
        {
            // Отодвигаемся от игрока
            _targetSpeed = _runSpeed;
            Vector3 awayFromPlayer = (transform.position - _player.position).normalized;
            _targetPosition = transform.position + awayFromPlayer * _targetSpeed;
            return;
        }
        else _targetSpeed = _moveSpeed;

        // Проверяем стену перед собой
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.SphereCast(origin, _sphereRadius, transform.forward, out hit, _rayLength, _wallLayer))
        {
            if (hit.distance < 1.5f)
            {
                // Двигаемся в сторону от стены
                float side = Random.Range(0, 2) == 0 ? 1f : -1f;
                _targetPosition = transform.position + transform.right * side * 2f;
                return;
            }
        }

        // Случайная цель
        Vector2 randomVector = new Vector2(
            Random.Range(-_maxMoveDistance, _maxMoveDistance),
            Random.Range(-_maxMoveDistance, _maxMoveDistance)
        );
        _targetPosition = transform.position + new Vector3(randomVector.x, 0, randomVector.y);
    }

    private bool WillHitWall(Vector3 moveStep)
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        RaycastHit hit;

        return Physics.SphereCast(origin, _sphereRadius, moveStep.normalized, out hit, moveStep.magnitude + 0.2f, _wallLayer);
    }

    private bool IsStuck()
    {
        // Проверяем, не внутри ли стены
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Collider[] colliders = Physics.OverlapSphere(origin, _sphereRadius, _wallLayer);
        return colliders.Length > 0;
    }

    private IEnumerator WalkTimer()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(0.01f, 0.03f));
            if (!_isStuck)
            {
                Walk();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & _wallLayer) != 0)
        {
            // Отталкиваемся от стены
            Vector3 awayFromWall = Vector3.zero;
            foreach (ContactPoint contact in collision.contacts)
            {
                awayFromWall += contact.normal;
            }
            awayFromWall.Normalize();
            transform.position += awayFromWall * 0.3f;

            // Ищем новый путь
            FindNewTarget();
        }
    }
}