using UnityEngine;

public class ParryWindowCollider : MonoBehaviour
{
    [SerializeField] private LayerMask projectileLayer;

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & projectileLayer.value) != 0)
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb != null)
            {
                
                Vector3 deflectDir = (transform.up + transform.forward).normalized;
                Vector3 force = deflectDir * 15f;
                rb.linearVelocity = Vector3.zero;
                rb.AddForce(force, ForceMode.VelocityChange);
            }
        }
    }
}