using Unity.Cinemachine;
using UnityEngine;

public class SceneDefaultCameraRegisterer : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private bool changeToActiveCamera = false;

    private void Start()
    {
        CameraManager.Instance.RegisterSceneDefaultCamera(cinemachineCamera, changeToActiveCamera);
    }
}
