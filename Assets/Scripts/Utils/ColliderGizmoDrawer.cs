using UnityEngine;

#if UNITY_EDITOR
[ExecuteAlways]
public class ColliderGizmoDrawer : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Collider[] colliders = GetComponents<Collider>();
        Gizmos.color = Color.magenta;

        foreach (var col in colliders)
        {
            if (col is BoxCollider box)
            {
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = box.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
                Gizmos.matrix = oldMatrix;
            }
            else if (col is SphereCollider sphere)
            {
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = sphere.transform.localToWorldMatrix;
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
                Gizmos.matrix = oldMatrix;
            }
            else if (col is CapsuleCollider capsule)
            {
                DrawWireCapsule(capsule);
            }
            else if (col is MeshCollider meshCol && meshCol.sharedMesh != null)
            {
                Gizmos.DrawWireMesh(meshCol.sharedMesh, meshCol.transform.position, meshCol.transform.rotation, meshCol.transform.lossyScale);
            }
        }
    }

    // Draws a wireframe for a CapsuleCollider
    private void DrawWireCapsule(CapsuleCollider capsule)
    {
        // For simplicity, draw as a wire sphere at each end and a wire cylinder in between
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = capsule.transform.localToWorldMatrix;
        Vector3 center = capsule.center;
        float radius = capsule.radius;
        float height = Mathf.Max(capsule.height, radius * 2f);
        int direction = capsule.direction;

        Vector3 up = Vector3.up, forward = Vector3.forward, right = Vector3.right;
        if (direction == 0) { up = Vector3.right; right = Vector3.up; }
        else if (direction == 2) { up = Vector3.forward; forward = Vector3.up; }

        float cylinderHeight = height - 2 * radius;
        Vector3 top = center + up * (cylinderHeight / 2f);
        Vector3 bottom = center - up * (cylinderHeight / 2f);

        Gizmos.DrawWireSphere(top, radius);
        Gizmos.DrawWireSphere(bottom, radius);
        Gizmos.DrawLine(top + right * radius, bottom + right * radius);
        Gizmos.DrawLine(top - right * radius, bottom - right * radius);
        Gizmos.DrawLine(top + forward * radius, bottom + forward * radius);
        Gizmos.DrawLine(top - forward * radius, bottom - forward * radius);

        Gizmos.matrix = oldMatrix;
    }
}
#endif