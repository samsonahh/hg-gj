using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Post-processing/Custom/Pixelation")]
public class PixelationVolume : VolumeComponent, IPostProcessComponent
{
    public ClampedIntParameter pixelSize = new ClampedIntParameter(1, 1, 32);

    public bool IsActive() => pixelSize.value > 1;
    public bool IsTileCompatible() => false;
}