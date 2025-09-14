using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

public class CameraFOVChanger : MonoBehaviour, IInitializable
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    public float OriginalFOV { get; private set; }
    
    public void Initialize()
    {
        OriginalFOV = GetCameraFOV();
    }

    public float GetCameraFOV() => cinemachineCamera.Lens.FieldOfView;
    
    public void SetCameraFOV(float targetFOV)
    {
        cinemachineCamera.Lens.FieldOfView = targetFOV;
    }
    
    public void TweenCameraFOV(float targetFOV, float duration, Ease easeType)
    {
        DOTween.Kill(cinemachineCamera);
        DOTween.To(
            SetCameraFOV,
            GetCameraFOV(),
            targetFOV,
            duration
        ).SetEase(easeType)
        .SetId(cinemachineCamera);
    }
}
