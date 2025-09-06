using Eflatun.SceneReference;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Utils
{
    public static SceneReference GetCurrentScene() => SceneReference.FromScenePath(SceneManager.GetActiveScene().path);

    public static float ConvertVolumeToDecibels(float volume) => Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
    public static float ConvertDecibelsToVolume(float decibels) => Mathf.Pow(10f, decibels / 20f);

    public static string FloatToString(float value, int decimalPlaces = 2)
    {
        string format = "F" + decimalPlaces;
        return value.ToString(format);
    }

    /// <summary>
    /// Calculates the movement input relative to the camera's orientation.
    /// The return result is normalized.
    /// </summary>
    public static Vector3 GetCameraBasedMoveInput(Transform cameraTransform, Vector2 moveInput)
    {
        if (cameraTransform == null)
            return moveInput;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0; // Ignore vertical component
        right.y = 0; // Ignore vertical component

        forward.Normalize();
        right.Normalize();

        return (forward * moveInput.y + right * moveInput.x).normalized;
    }
}
