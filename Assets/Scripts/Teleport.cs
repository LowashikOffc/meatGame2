using System.Collections;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    [SerializeField] private Transform _player;
    private Rigidbody _rigidbody;
    [SerializeField] private Transform _pos1;
    [SerializeField] private Transform _pos2;

    private float _waitTime = 2;

    private void Start()
    {
        _rigidbody = _player.GetComponent<Rigidbody>();
    }

    private void Tp(Transform pos)
    {
        _player.position = pos.position + Vector3.up * _player.localScale.y / 2;
        _rigidbody.velocity = (new Vector3(_rigidbody.velocity.x, -_rigidbody.velocity.y*0.9f, _rigidbody.velocity.z));
        StartCoroutine(Wait(_waitTime, pos));
    }

    public void TeleportToPos(Transform pos)
    {
        SoundService.Instance.PlaySound(SoundBase.Teleport, pos.position, 0.7f);
        if (pos == _pos1)
        {
            _pos2.GetComponent<Collider>().enabled = false;
            Tp(_pos2);
        }
        if (pos == _pos2)
        {
            _pos1.GetComponent<Collider>().enabled = false;
            Tp(_pos1);
        }
    }
    
    IEnumerator Wait(float t, Transform pos)
    {
        yield return new WaitForSeconds(t);
        pos.GetComponent<Collider>().enabled = true;
    }
}
