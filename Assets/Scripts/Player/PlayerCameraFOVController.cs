using NaughtyAttributes;
using UnityEngine;

public class PlayerCameraFOVController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CameraFOVChanger fovChanger;
    [InfoBox("The X-axis is the player's (speed / player.StrictSpeedCap) while the Y-axis is ((FOV increase from the starting FOV of the camera) / maxFOV).", EInfoBoxType.Normal)]
    [SerializeField] private AnimationCurve fovCurve;
    [SerializeField] private float maxFOV = 90f;
    [InfoBox("Lerp speed is how smooth you want the camera FOV to change.")]
    [SerializeField] private float lerpSpeed = 10f;
    
    private void Update()
    {
        float playerNormalizedSpeed = playerController.PlanarVelocity.magnitude / playerController.StrictSpeedCap;
        float curveResult = fovCurve.Evaluate(playerNormalizedSpeed);
        float targetFOV = curveResult * (maxFOV - fovChanger.OriginalFOV) + fovChanger.OriginalFOV;
        float finalFOV = Mathf.Lerp(fovChanger.GetCameraFOV(), targetFOV, lerpSpeed * Time.deltaTime);
        fovChanger.SetCameraFOV(finalFOV);
    }
}