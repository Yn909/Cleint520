using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AdvancedButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    Vector3 originalScale;
    Image img;

    public Color hoverColor = new Color(1f, 1f, 1f, 1f);
    public Color normalColor = new Color(0.8f, 0.8f, 0.8f, 1f);

    public AudioSource clickSound;

    void Start()
    {
        originalScale = transform.localScale;
        img = GetComponent<Image>();
        img.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * 1.1f;
        img.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
        img.color = normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.localScale = originalScale * 0.9f;

        if (clickSound != null)
            clickSound.Play();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = originalScale * 1.1f;
    }
}