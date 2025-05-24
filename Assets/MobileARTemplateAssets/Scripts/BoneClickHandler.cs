using UnityEngine;
using System.Collections.Generic;

public class BoneClickHandler : MonoBehaviour
{
    private static GameObject previouslySelected = null;
    private static Dictionary<Renderer, Material[]> previousOriginalMaterials = new Dictionary<Renderer, Material[]>();

    [SerializeField] private Material highlightBaseMaterial;
    [SerializeField] private float isolateDistance = 1.0f;

    private void OnMouseDown()
    {
        // UI'ya týklanmýþsa iptal et
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;

        string boneName = gameObject.name;
        UIManager ui = FindObjectOfType<UIManager>();
        if (ui == null) return;

        // Önceki highlight'ý geri al
        if (previouslySelected != null && previousOriginalMaterials.Count > 0)
        {
            Renderer[] prevRenderers = previouslySelected.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in prevRenderers)
            {
                if (previousOriginalMaterials.TryGetValue(r, out Material[] mats))
                {
                    r.materials = mats;
                }
            }

            previousOriginalMaterials.Clear();
        }

        // Yeni seçilen kemiðin renderer'larýný bul
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            Material[] originalMats = r.materials;
            previousOriginalMaterials[r] = originalMats;

            Material[] highlightMats = new Material[originalMats.Length];
            for (int i = 0; i < originalMats.Length; i++)
            {
                Material highlightInstance = new Material(highlightBaseMaterial);
                highlightInstance.color = new Color(1f, 1f, 0f, 1f); // sarý
                EnableTransparency(highlightInstance);
                highlightMats[i] = highlightInstance;
            }

            r.materials = highlightMats;
        }

        previouslySelected = gameObject;
        ui.SetSelectedBone(gameObject, null, isolateDistance);
        ui.ShowBoneInfo(boneName);
    }

    private void EnableTransparency(Material mat)
    {
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }
}
