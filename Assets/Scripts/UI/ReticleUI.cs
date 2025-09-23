using UnityEngine;
using UnityEngine.UI;

public class ReticleUI : MonoBehaviour
{
    [Header("Reticle Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color enemyColor = Color.red;
    [SerializeField] private float rayDistance = 100f;
    [SerializeField] private LayerMask enemyLayer;
    [Tooltip("Camera raycast for detection")]
    [SerializeField] private Camera raycastCamera;

    [SerializeField] private Image reticleImage;

    [Header("Debug")]
    [SerializeField] private bool debugDrawRayGizmo = false;
    [SerializeField] private Color debugRayColor = Color.green;
    [SerializeField] private Color debugRayHitColor = Color.red;

    private bool _lastHitEnemy = false;
    private Vector3 _lastRayOrigin;
    private Vector3 _lastRayDirection;
    private float _lastRayDistance;
    private Vector3 _lastHitPoint;

    private void Awake()
    {
        if (reticleImage == null)
            reticleImage = GetComponent<Image>();
        if (raycastCamera == null)
            raycastCamera = Camera.main;
    }

    private void Update()
    {
        if (raycastCamera == null || reticleImage == null)
            return;

        Ray ray = raycastCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        bool hitEnemy = Physics.Raycast(ray, out hit, rayDistance, enemyLayer);

        reticleImage.color = hitEnemy ? enemyColor : normalColor;

        // Store ray info for gizmo drawing
        _lastRayOrigin = ray.origin;
        _lastRayDirection = ray.direction;
        _lastRayDistance = rayDistance;
        _lastHitEnemy = hitEnemy;
        _lastHitPoint = hitEnemy ? hit.point : ray.origin + ray.direction * rayDistance;
    }

    private void OnDrawGizmos()
    {
        if (!debugDrawRayGizmo || raycastCamera == null)
            return;

        Gizmos.color = _lastHitEnemy ? debugRayHitColor : debugRayColor;
        Gizmos.DrawLine(_lastRayOrigin, _lastHitPoint);
        Gizmos.DrawSphere(_lastHitPoint, 0.05f);
    }

    public float RayDistance
    {
        get => rayDistance;
        set => rayDistance = value;
    }
}