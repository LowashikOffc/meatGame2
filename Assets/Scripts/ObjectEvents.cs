using System.Collections;
using UnityEngine;

public class ObjectEvents : MonoBehaviour
{
    private ObjectConfig _conf;
    private GroundSoundConfig _gsconf;
    private float _distance;
    private Vector3 _lastPos;
    private float _timeToNextSound = 0.1f;
    private float _currentTimeSound = 0.2f;
    [SerializeField] private float _minimumQuiet = 0.7f;
    [SerializeField] private float _minimumDefault = 2f;
    private void Start()
    {
        _conf = GetComponent<ObjectConfig>();
        _gsconf = GetComponent<GroundSoundConfig>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null) return;
        Materials m = _gsconf.Material;
        if (m == Materials.Metal) playSound(SoundBase.Metal, SoundBase.QuietMetal);
        if (m == Materials.Wood) playSound(SoundBase.Wood, SoundBase.QuietWood);
        _currentTimeSound = _timeToNextSound;
    }

    private void FixedUpdate()
    {
        _currentTimeSound -= Time.fixedDeltaTime;
        if (transform.name == "Ventilation") Debug.Log(_currentTimeSound);
        _distance = Vector3.Distance(transform.position, _lastPos) * 100;
        if (Vector3.Distance(_lastPos, transform.position) > 0.01f)
        {
            EnergySafe _safe;
            if (transform.TryGetComponent<EnergySafe>(out _safe))
            {
                //Debug.Log(_safe);
                //Debug.Log(_safe.GetCableIN());
                if (_safe.GetCableIN()) _safe.GetCableIN().Disconnect(_safe.GetCableIN().GetRigidbody(true));
                if (_safe.GetCableOUT()) _safe.GetCableOUT().Disconnect(_safe.GetCableOUT().GetRigidbody(false));
            }
        }
        _lastPos = transform.position;
    }

    private void playSound(SoundBase sound, SoundBase quietSound)
    {
        if (_currentTimeSound > 0) return;
        if (_distance >= _minimumDefault) SoundService.Instance.PlaySound(sound, transform.position, 2);
        else if (_distance <= _minimumDefault && _distance >= _minimumQuiet) SoundService.Instance.PlaySound(quietSound, transform.position, 0.7f);
    }
}
