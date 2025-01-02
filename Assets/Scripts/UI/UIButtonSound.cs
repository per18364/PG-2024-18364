using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FMODUnity;

public class UIButtonSound : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    [SerializeField] private EventReference clickSound; // Assign the click sound in the Inspector
    [SerializeField] private EventReference hoverSound; // Optional hover sound

    private void Start()
    {
        Button button = GetComponent<Button>();
        if (button)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    public void PlayClickSound()
    {
        print("CLICK");
        AudioManager.Instance.PlayUISound(clickSound);
    }

    public void OnSelect(BaseEventData eventData)
    {
        PlayHoverSound(); // Trigger sound when navigating with controller or keyboard
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayHoverSound(); // Trigger sound when hovering with a mouse
    }

    private void PlayHoverSound()
    {        
        AudioManager.Instance?.PlayUISound(hoverSound);
    }    
}
