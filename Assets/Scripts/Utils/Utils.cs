using Eflatun.SceneReference;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Utils
{
    /// <summary>
    /// Returns a new Vector3 with one component (x, y, or z) set to a new value.
    /// Vector3.x = x does not work as expected in Unity, so this is a workaround.
    /// </summary>
    public static Vector3 WithX(this Vector3 vector, float x) => new Vector3(x, vector.y, vector.z);
    /// <summary>
    /// Returns a new Vector3 with one component (x, y, or z) set to a new value.
    /// Vector3.z = z does not work as expected in Unity, so this is a workaround.
    /// </summary>
    public static Vector3 WithY(this Vector3 vector, float y) => new Vector3(vector.x, y, vector.z);
    /// <summary>
    /// Returns a new Vector3 with one component (x, y, or z) set to a new value.
    /// Vector3.z = z does not work as expected in Unity, so this is a workaround.
    /// </summary>
    public static Vector3 WithZ(this Vector3 vector, float z) => new Vector3(vector.x, vector.y, z);

    /// <summary>
    /// Sets the x component of a Vector3 to a new value.
    /// Vector3.x = x does not work as expected in Unity, so this is a workaround.
    /// </summary>
    public static void SetX(this ref Vector3 vector, float x) => vector = vector.WithX(x);
    /// <summary>
    /// Sets the y component of a Vector3 to a new value.
    /// Vector3.y = y does not work as expected in Unity, so this is a workaround.
    /// </summary>
    public static void SetY(this ref Vector3 vector, float y) => vector = vector.WithY(y);
    /// <summary>
    /// Sets the z component of a Vector3 to a new value.
    /// Vector3.z = z does not work as expected in Unity, so this is a workaround.
    /// </summary>
    public static void SetZ(this ref Vector3 vector, float z) => vector = vector.WithZ(z);
    
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

    public static float GetJumpForce(float height) => Mathf.Sqrt(2 * height * Mathf.Abs(Physics.gravity.y));
    
    /// <summary>
    /// Checks to see if a layermask contains a layer
    /// </summary>
    public static bool Contains(this LayerMask mask, int layer)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}
