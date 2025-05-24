using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Transform screenCenterTarget;

    public GameObject rightPanel;
    public GameObject middlePanel;

    public TextMeshProUGUI boneNameText;
    public TextMeshProUGUI boneDescriptionText;

    public GameObject skeletonRoot;
    public Camera arCamera;

    //public Button isolateButton;
    public Button hideOthersButton;
    public Button fadeButton;
    public Button hideButton;
    public Button resetButton;

    public Color normalColor = new Color(0.5f, 0.9f, 1f);
    public Color activeColor = new Color(0f, 0f, 0.8f);

    private GameObject selectedBone;
    private Material selectedMaterial;

    private Vector3 savedBoneWorldPosition;
    private Quaternion savedBoneWorldRotation;
    private Vector3 savedModelWorldPosition;
    private Quaternion savedModelWorldRotation;

    private Vector3 initialModelPosition;
    private Quaternion initialModelRotation;

    private bool isFaded = false;
    private bool isHidden = false;
   // private bool isIsolated = false;
    private bool othersHidden = false;
    private bool isPanelManuallyHidden = false;
    private GameObject lastBoneForWhichPanelWasHidden = null;

    private Dictionary<string, string> boneDescriptions = new Dictionary<string, string>()
    {
        { "Vertebra T9", "Göðüs omurlarýndan biridir." },
        { "Femur", "Uyluk kemiði, vücuttaki en uzun kemiktir." }
    };

    void Start()
    {
        if (skeletonRoot != null)
        {
            initialModelPosition = skeletonRoot.transform.position;
            initialModelRotation = skeletonRoot.transform.rotation;
        }

        
    }


    public void SetSelectedBone(GameObject boneObj, Material mat, float isolateDistance)
    {
        selectedBone = boneObj;
        selectedMaterial = mat;


        isFaded = false;
        isHidden = false;
        isPanelManuallyHidden = false;
        lastBoneForWhichPanelWasHidden = null;
        //isolateButton.interactable = true;
        hideOthersButton.interactable = true;
        fadeButton.interactable = true;
        hideButton.interactable = true;
    }

    
    public void ShowBoneInfo(string boneName)
    {
        boneNameText.text = boneName;

        if (boneDescriptions.ContainsKey(boneName))
            boneDescriptionText.text = boneDescriptions[boneName];
        else
            boneDescriptionText.text = "Bu kemiðe ait açýklama bulunamadý.";

        // Paneli her týklamada direkt aç
        rightPanel.SetActive(true);
        middlePanel.SetActive(true);

        isRightPanelVisible = true;
        isMiddlePanelVisible = true;
    }

    public bool isRightPanelVisible = false;

    public void ToggleRightPanel()
    {
        isRightPanelVisible = !isRightPanelVisible;
        rightPanel.SetActive(isRightPanelVisible);
    }

    public bool isMiddlePanelVisible = false;

    public void ToggleMiddlePanel()
    {
        isMiddlePanelVisible = !isMiddlePanelVisible;
        middlePanel.SetActive(isMiddlePanelVisible);
    }






    public void OnHideButton()
    {
        if (selectedBone == null) return;

        isHidden = !selectedBone.activeSelf;
        selectedBone.SetActive(isHidden);

        var colorCtrl = hideButton.GetComponent<ButtonColorController>();
        if (isHidden)
            colorCtrl?.SetActiveState();
        else
            colorCtrl?.ResetToNormalState();
    }

    private void OnFadeButton()
    {
        if (selectedBone == null) return;

        isFaded = !isFaded;

        Renderer[] renderers = selectedBone.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;

            for (int i = 0; i < materials.Length; i++)
            {
                Color color = materials[i].color;

                if (isFaded)
                {
                    color.a = 0.2f;
                    EnableTransparency(materials[i]);
                }
                else
                {
                    color.a = 1f;
                }

                materials[i].color = color;
            }
        }

        var colorCtrl = fadeButton.GetComponent<ButtonColorController>();
        if (isFaded)
            colorCtrl?.SetActiveState();
        else
            colorCtrl?.ResetToNormalState();
    }

    private void EnableTransparency(Material mat)
    {
        if (mat == null) return;

        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }


    public void OnResetButton()
    {
        resetButton.GetComponent<ButtonColorController>()?.ResetToNormalState();

        // Model pozisyonunu sýfýrla
        if (skeletonRoot != null)
        {
            skeletonRoot.transform.position = initialModelPosition;
            skeletonRoot.transform.rotation = initialModelRotation;
        }

        // Eðer kemik Isolate objesiyse çýkart
        if (selectedBone != null && selectedBone.name == "IsolatedBonePivot")
        {
            Transform originalBone = selectedBone.transform.GetChild(0);
            originalBone.SetParent(skeletonRoot.transform);
            Destroy(selectedBone);
            selectedBone = originalBone.gameObject;
        }

        // Tüm kemikleri yeniden aktif et
        Transform[] allBones = skeletonRoot.GetComponentsInChildren<Transform>(true);
        foreach (Transform bone in allBones)
        {
            if (bone == skeletonRoot.transform) continue;
            bone.gameObject.SetActive(true);
        }

        // Tüm durum bayraklarýný sýfýrla
        isFaded = false;
        isHidden = false;
        //isIsolated = false;
        othersHidden = false;

        // Buton renklerini sýfýrla
        hideOthersButton.GetComponent<ButtonColorController>()?.ResetToNormalState();
        fadeButton.GetComponent<ButtonColorController>()?.ResetToNormalState();
        hideButton.GetComponent<ButtonColorController>()?.ResetToNormalState();
    }



    /*public void OnIsolateButton()
    {
        if (selectedBone == null || skeletonRoot == null || arCamera == null || screenCenterTarget == null) return;

        StartCoroutine(DisableAllButtonsForSeconds(2f));

        if (!isIsolated)
        {
            isIsolated = true;
            hideOthersButton.interactable = false;
            SetButtonColor(isolateButton, activeColor);
            isolateButton.GetComponent<ButtonColorController>()?.SetActiveState();

            // Diðer kemikleri gizle
            foreach (Transform bone in skeletonRoot.transform)
            {
                if (bone.gameObject != selectedBone)
                    bone.gameObject.SetActive(false);
            }

            // Kemik referansý
            Transform boneRef = (selectedBone.name == "IsolatedBonePivot") ? selectedBone.transform.GetChild(0) : selectedBone.transform;
            SaveTransforms(boneRef);

            // Kemik görsel merkezi alýnýr
            Renderer rend = boneRef.GetComponentInChildren<Renderer>();
            if (rend == null) return;

            Vector3 boneCenter = rend.bounds.center;

            // Pivot wrapper oluþtur ve kemiði altýna al
            GameObject pivotWrapper = new GameObject("IsolatedBonePivot");
            pivotWrapper.transform.position = boneCenter;
            boneRef.SetParent(pivotWrapper.transform);  // gerçek kemiði baðla
            pivotWrapper.transform.SetParent(skeletonRoot.transform); // pivotu modele baðla

            if (pivotWrapper.GetComponent<DragRotate>() == null)
                pivotWrapper.AddComponent<DragRotate>();

            selectedBone = pivotWrapper;

            // Hedef konuma taþý: screenCenterTarget
            Vector3 offset = screenCenterTarget.position - boneCenter;
            skeletonRoot.transform.position += offset;

            // Kameraya doðru baksýn
            skeletonRoot.transform.rotation = Quaternion.LookRotation(arCamera.transform.forward);
        }
        else
        {
            isIsolated = false;
            hideOthersButton.interactable = true;
            SetButtonColor(isolateButton, normalColor);
            isolateButton.GetComponent<ButtonColorController>()?.ResetToNormalState();

            foreach (Transform bone in skeletonRoot.transform)
                bone.gameObject.SetActive(true);

            RestoreBoneAndModelToSavedPose();
        }
    }*/





    public void OnHideOthersButton()
    {
        if (selectedBone == null || skeletonRoot == null) return;

        var colorCtrl = hideOthersButton.GetComponent<ButtonColorController>();

        if (!othersHidden)
        {
            othersHidden = true;
            SetButtonColor(hideOthersButton, activeColor);
            colorCtrl?.SetActiveState();
            //isolateButton.interactable = false;

            Transform boneRef = (selectedBone.name == "IsolatedBonePivot") ? selectedBone.transform.GetChild(0) : selectedBone.transform;
            SaveTransforms(boneRef);

            Transform[] allBones = skeletonRoot.GetComponentsInChildren<Transform>(true);
            foreach (Transform bone in allBones)
            {
                if (bone == skeletonRoot.transform) continue;
                bone.gameObject.SetActive(bone.gameObject == selectedBone);
            }
        }
        else
        {
            othersHidden = false;
            SetButtonColor(hideOthersButton, normalColor);
            colorCtrl?.ResetToNormalState();
            //isolateButton.interactable = true;

            Transform[] allBones = skeletonRoot.GetComponentsInChildren<Transform>(true);
            foreach (Transform bone in allBones)
            {
                if (bone == skeletonRoot.transform) continue;
                bone.gameObject.SetActive(true);
            }

            RestoreBoneAndModelToSavedPose();
        }
    }

    private void SaveTransforms(Transform boneRef)
    {
        savedBoneWorldPosition = boneRef.position;
        savedBoneWorldRotation = boneRef.rotation;
        savedModelWorldPosition = skeletonRoot.transform.position;
        savedModelWorldRotation = skeletonRoot.transform.rotation;
    }

    private void RestoreBoneAndModelToSavedPose()
    {
        if (selectedBone.name == "IsolatedBonePivot")
        {
            Transform originalBone = selectedBone.transform.GetChild(0);
            originalBone.SetParent(null);
            skeletonRoot.transform.SetPositionAndRotation(savedModelWorldPosition, savedModelWorldRotation);
            originalBone.SetPositionAndRotation(savedBoneWorldPosition, savedBoneWorldRotation);
            originalBone.SetParent(skeletonRoot.transform);
            Destroy(selectedBone);
            selectedBone = originalBone.gameObject;
        }
        else
        {
            selectedBone.transform.SetParent(null);
            skeletonRoot.transform.SetPositionAndRotation(savedModelWorldPosition, savedModelWorldRotation);
            selectedBone.transform.SetPositionAndRotation(savedBoneWorldPosition, savedBoneWorldRotation);
            selectedBone.transform.SetParent(skeletonRoot.transform);
        }
    }

    private IEnumerator DisableAllButtonsForSeconds(float duration)
    {
        Button[] allButtons = FindObjectsOfType<Button>();
        foreach (var btn in allButtons)
            btn.interactable = false;

        yield return new WaitForSeconds(duration);

        foreach (var btn in allButtons)
            btn.interactable = true;

        /*if (isIsolated)
        {
            isolateButton.interactable = true;
            SetButtonColor(isolateButton, activeColor);
            isolateButton.GetComponent<ButtonColorController>()?.SetActiveState();
            hideOthersButton.interactable = false;
        }*/
    }

    private void SetButtonColor(Button btn, Color color)
    {
        if (btn != null && btn.targetGraphic != null)
            btn.targetGraphic.color = color;
    }

    private IEnumerator MoveModelToCameraView(Transform model, Vector3 targetPos, Quaternion targetRot, float duration)
    {
        Vector3 startPos = model.position;
        Quaternion startRot = model.rotation;
        float time = 0f;

        while (time < duration)
        {
            model.position = Vector3.Lerp(startPos, targetPos, time / duration);
            model.rotation = Quaternion.Slerp(startRot, targetRot, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        model.position = targetPos;
        model.rotation = targetRot;
    }
    public void SlidePanel(GameObject panel, Vector2 hiddenPos, Vector2 shownPos, float duration = 0.4f)
    {
        StartCoroutine(SlideCoroutine(panel.GetComponent<RectTransform>(), hiddenPos, shownPos, duration));
    }

    private IEnumerator SlideCoroutine(RectTransform panelRect, Vector2 from, Vector2 to, float duration)
    {
        float elapsed = 0f;
        panelRect.anchoredPosition = from;
        panelRect.gameObject.SetActive(true);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            panelRect.anchoredPosition = Vector2.Lerp(from, to, t);
            yield return null;
        }

        panelRect.anchoredPosition = to;
    }

}
