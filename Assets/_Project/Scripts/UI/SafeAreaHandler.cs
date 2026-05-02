using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Ajusta el padding del VisualElement raíz del UIDocument
/// al Safe Area del dispositivo (notches, cámaras frontales, etc.)
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class SafeAreaHandler : MonoBehaviour
{
    private UIDocument uiDocument;
    private Rect lastSafeArea = Rect.zero;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        ApplySafeArea();
    }

    private void Update()
    {
        // Reaplica si el safe area cambió (rotación en móvil)
        if (Screen.safeArea != lastSafeArea)
            ApplySafeArea();
    }

    private void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        lastSafeArea = safeArea;

        // Calcular márgenes en píxeles
        float left = safeArea.x;
        float bottom = safeArea.y;
        float right = Screen.width - (safeArea.x + safeArea.width);
        float top = Screen.height - (safeArea.y + safeArea.height);

        // Aplicar como padding al VisualElement raíz
        VisualElement root = uiDocument.rootVisualElement;
        root.style.paddingLeft = left;
        root.style.paddingRight = right;
        root.style.paddingTop = top;
        root.style.paddingBottom = bottom;

        // Codigo de depuración para verificar los valores
        // Debug.Log($"[SafeArea] L:{left} R:{right} T:{top} B:{bottom}");
    }
}