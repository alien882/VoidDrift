using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Ajusta la calidad visual según las capacidades del dispositivo.
/// Se ejecuta una sola vez al inicio.
/// </summary>
public class QualityController : MonoBehaviour
{
    [SerializeField] private Volume globalVolume;

    private void Start()
    {
        ApplyQualitySettings();
    }

    private void ApplyQualitySettings()
    {
        // Dispositivos con menos de 1GB de VRAM usan calidad baja
        bool isLowEnd = SystemInfo.graphicsMemorySize < 1024;

        if (isLowEnd && globalVolume != null)
        {
            if (globalVolume.profile.TryGet<Bloom>(out Bloom bloom))
            {
                bloom.intensity.value = 1f; // Reducir bloom
            }

            Debug.Log("[Quality] Dispositivo de gama baja — efectos reducidos");
        }

        // Reducir partículas en WebGL
#if UNITY_WEBGL
            QualitySettings.particleRaycastBudget = 64;
#endif
    }
}
