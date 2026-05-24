using UnityEngine;

/// <summary>
/// Destruye el GameObject cuando el ParticleSystem termina de reproducirse.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class AutoDestroy : MonoBehaviour
{
    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (!ps.IsAlive())
            Destroy(gameObject);
    }
}
