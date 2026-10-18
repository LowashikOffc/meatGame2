using UnityEngine;

public class Clouds : MonoBehaviour
{
    [SerializeField] private Vector2 _moveTo;
    [SerializeField] private Renderer[] _layerRenderers;
    private Transform _layer;

    private void Start()
    {
        _layer = transform;
    }
    private void FixedUpdate()
    {
        foreach(var renderer in _layerRenderers)
        {
            renderer.material.mainTextureOffset += _moveTo/1000;
        }
    }
}
