using UnityEngine;

public class CameraPos : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private Vector3 _addPos;
    private Vector3 _lastPos;
    void Update()
    {
        Vector3 newPos = Vector3.Lerp(_lastPos, _player.position + Vector3.up * _player.localScale.y * _addPos.y, Time.deltaTime * 15f);
        transform.position = newPos;
        _lastPos = transform.position;
    }
}
