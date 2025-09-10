using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Detection Settings")]
    public float radius;
    public Transform detectionPoint;
    public LayerMask layerToDetect;

    [Header("Rendering Settings")]
    public Material detectMat;
    Renderer _renderer;
    Material _originalMat;


    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _originalMat = _renderer.material;
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] detection = Physics.OverlapSphere(detectionPoint.position, radius, layerToDetect);

        if (detection != null && detection.Length > 0)
        {
            _renderer.material = detectMat;
        } else
        {
            _renderer.material = _originalMat;
        }
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.gray;
    //    Gizmos.DrawWireSphere(detectionPoint.position, radius);
    //}
}
