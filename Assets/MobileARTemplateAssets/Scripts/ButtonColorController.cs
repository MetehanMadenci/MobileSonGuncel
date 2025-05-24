using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ButtonColorController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TextMeshProUGUI buttonText;

    public Color normalText = Color.white;
    public Color hoverText = new Color32(48, 140, 238, 255);    // #308CE
    public Color activeText = new Color32(0, 4, 49, 255);        // #000431
    public Color disabledText = Color.red;

    private bool isToggled = false;
    private bool isPointerOver = false;
    private Button button;

    public bool disableToggle = false;  // Reset gibi aktifleþmeyecek butonlar için

    void Awake()
    {
        button = GetComponent<Button>();
    }

    void Start()
    {
        ResetToNormalState();
    }

    void Update()
    {
        if (button != null && !button.interactable && buttonText != null)
        {
            buttonText.color = disabledText;
        }
    }

    void OnEnable()
    {
        if (button != null && button.interactable && buttonText != null)
        {
            buttonText.color = isToggled ? activeText : normalText;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;

        if (button != null && button.interactable && buttonText != null)
        {
            buttonText.color = hoverText;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;

        if (button != null && button.interactable && buttonText != null)
        {
            buttonText.color = isToggled ? activeText : normalText;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;

        if (!disableToggle)
        {
            isToggled = !isToggled;

            if (buttonText != null)
            {
                buttonText.color = isToggled ? activeText : (isPointerOver ? hoverText : normalText);
            }
        }
    }

    public void SetActiveState()
    {
        isToggled = true;
        if (buttonText != null)
            buttonText.color = activeText;
    }

    public void ResetToNormalState()
    {
        isToggled = false;
        if (button != null && buttonText != null)
        {
            buttonText.color = isPointerOver ? hoverText : normalText;
        }
    }
}
