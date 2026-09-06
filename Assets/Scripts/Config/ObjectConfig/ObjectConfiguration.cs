using UnityEngine;

[CreateAssetMenu(fileName = "newObject", menuName = "Holding/ObjectConfig")]
public class ObjectConfiguration : ScriptableObject
{
    public Vector3 PositionOffset;
    public Vector3 RotationOffset;
}

