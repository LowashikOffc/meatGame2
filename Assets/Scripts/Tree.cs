using UnityEngine;

public class Tree : MonoBehaviour
{
    void Start()
    {
        float _scaleXZ = Random.Range(0.8f, 1.2f);
        float _scaleY = Random.Range(0.7f, 1.5f);
        transform.localScale = new Vector3(_scaleXZ, _scaleY, _scaleXZ);
        int rotation = Random.Range(0, 360);
        transform.rotation = Quaternion.Euler(0, rotation, 0);
    }
}
