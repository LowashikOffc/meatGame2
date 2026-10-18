using UnityEngine;

[System.Serializable]
public class LodLevel
{
    [HideInInspector] public string _name;
    [HideInInspector] public int _level;
    public GameObject _obj;
    public float _distance;

    public void Upd(int level)
    {
        _level = level;
        _name = "Level " + _level.ToString();
    }
}

public class LOD : MonoBehaviour
{
    private Camera _camera = Camera.main;
    [SerializeField] private LodLevel[] _levels;
    private void OnValidate()
    {
        int a = 0;
        foreach(var level in _levels)
        {
            a++;
            level.Upd(a);
        }
    }

    private void FixedUpdate()
    {
        float sqrCamDist = (_camera.transform.position - transform.position).magnitude;

        foreach (var level in _levels)
        {
            bool shouldBeActive = level._distance * level._distance <= sqrCamDist;
            if (level._obj.activeSelf != shouldBeActive)
                level._obj.SetActive(shouldBeActive);
        }
    }
}
