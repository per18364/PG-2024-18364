using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPanel : MonoBehaviour
{
    [SerializeField] RectTransform rect;
    [SerializeField] GameObject[] Content;

    private Vector2 size;

    void Awake() {
        size = rect.sizeDelta;
        rect.sizeDelta = new Vector2(0, size.y);
    }

    void Start() {
        foreach (GameObject ob in Content) {
            ob.SetActive(false);
        }
        StartCoroutine(PopUpTutorial());
    }

    public IEnumerator PopUpTutorial(){
        yield return new WaitForSeconds(0.2f);

        rect.LeanSize(size, 0.3f).setEase(LeanTweenType.easeInCubic);

        yield return new WaitForSeconds(0.4f);

        foreach (GameObject ob in Content) {
            ob.SetActive(true);
        }
    }

    public void CompleteInstruction() {
        foreach (GameObject ob in Content) {
            ob.SetActive(false);
        }

        rect.LeanSize(new Vector2(0, size.y), 0.3f).setEase(LeanTweenType.easeInCubic);
    }

    
}
