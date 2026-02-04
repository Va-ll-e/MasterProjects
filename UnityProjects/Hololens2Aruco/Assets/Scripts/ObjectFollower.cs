using UnityEngine;

public class ObjectFollower : MonoBehaviour
{
    public Transform followTraget;

    public Vector3 localPositionOffset;
    public Vector3 localRotationOffset;

    void LateUpdate()
    {
        if (!followTraget) return;
        
        transform.position = followTraget.TransformPoint(localPositionOffset);
        
    }

}
