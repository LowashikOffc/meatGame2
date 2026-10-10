using UnityEngine;

public class LightBeam : MonoBehaviour
{
    private Camera _camera;
    [SerializeField] private Vector3 add = Vector3.zero;
    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        transform.LookAt(_camera.transform.position);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0) + add;
    }
}
