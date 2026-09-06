using UnityEngine;

public class SoundCollision : MonoBehaviour
{
    private float _startVolume;
    private float _targetVolume;
    [SerializeField] private float _collisionVolume = 0.2f;

    [SerializeField] private Transform _sourceParent;
    private AudioSource _source;
    public Transform _player;

    [SerializeField] private bool _draw;

    private void Start()
    {
        if (_sourceParent == null) _sourceParent = transform;
        _source = _sourceParent.GetComponent<AudioSource>();
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _startVolume = _source.volume;
        _targetVolume = _startVolume;
    }

    public void SetVolume(float volume)
    {
        _targetVolume = volume;
    }

    private void OnDrawGizmos()
    {
        if (!_draw) return;
        if (_player == null) _player = GameObject.FindGameObjectWithTag("Player").transform;
        if (Physics.Raycast(_sourceParent.position, _player.position - _sourceParent.position, out RaycastHit hit))
        {
            Debug.DrawLine(_sourceParent.position, hit.point);
        }
    }

    private void FixedUpdate()
    {
        if (Physics.Raycast(_sourceParent.position, _player.position - _sourceParent.position, out RaycastHit hit))
        {
            if (hit.transform != _player)
            {
                _targetVolume = _startVolume * _collisionVolume;
            }
            else
            {
                _targetVolume = _startVolume;
            }
        }
        _source.volume = Mathf.Lerp(_source.volume, _targetVolume, 0.7f);
    }
}
