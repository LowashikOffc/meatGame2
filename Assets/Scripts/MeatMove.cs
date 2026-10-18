using System.Collections;
using UnityEngine;

[System.Serializable]
public class Mat
{
    public Material _material;
    public Vector2 _startScale;
}

public class MeatMove : MonoBehaviour
{
    [SerializeField] private Mat[] _fleshMaterials;
    [SerializeField] private float _scaleSpeed;
    [SerializeField] private float _maxScale;
    [SerializeField] private float _minScale;
    private bool _isBreatheIn;
    private float _scale;
    private float _scaleVelocity;

    private void Start()
    {
        _scale = _maxScale;
        StartCoroutine(StateChange());
    }
    void Update()
    {
        float targetScale = _isBreatheIn ? _maxScale : _minScale;
        _scale = Mathf.SmoothDamp(_scale, targetScale, ref _scaleVelocity, 1f / _scaleSpeed);
        foreach (Mat m in _fleshMaterials)
        {
            m._material.mainTextureScale = new Vector2(m._startScale.x * _scale, m._startScale.y * _scale);
        }
    }

    private void OnValidate()
    {
        foreach (Mat m in _fleshMaterials)
        {
            m._material.mainTextureScale = new Vector2(m._startScale.x * _maxScale, m._startScale.y * _maxScale);
        }
    }

    IEnumerator StateChange()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1.8f, 2.2f));
            _isBreatheIn = !_isBreatheIn;
        }
    }
}
