using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MunitionDisplay : MonoBehaviour
{
    [SerializeField] Sprite[] ammunition_sprites;
    [SerializeField] Image ammunition_display;
    [SerializeField] GameObject gate_left;
    [SerializeField] GameObject gate_right;

    [SerializeField] GameObject inner_;
    [SerializeField] GameObject outer_;
    [SerializeField] private TMP_Text _textAmmo;

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(testChange());
    }

    IEnumerator testChange()
    {
        int iteration = 0;
        while(true)
        {
            Debug.Log("Iteration : "+  iteration.ToString());
            
            ChangeAmmo(0);
            yield return new WaitForSeconds(7);
            ChangeAmmo(1);
            yield return new WaitForSeconds(7);
            ChangeAmmo(2);
            yield return new WaitForSeconds(7);
        }
    }

    public void ChangeAmmo (int ammo)
    {
        StartCoroutine(ChangeAmmoAnimation(ammo));
    }

    public void AmmoText(int actualMunition, int maximumMunition)
    {
        _textAmmo.text = actualMunition + "/" + maximumMunition;
    }

    IEnumerator ChangeAmmoAnimation(int ammo)
    {
        gate_left.LeanMoveLocalX(-36.77f, 0.3f).setEaseOutBounce();
        gate_right.LeanMoveLocalX(0f, 0.5f).setEaseOutBounce();
        inner_.LeanRotate(new Vector3(0, 0, 180), 0.4f).setEaseInBounce();
        outer_.LeanRotate(new Vector3(0, 0, -180), 0.4f).setEaseInBounce();
        //LeanTween.rotateLocal(inner_, new Vector3(0, 0, -360), 0.5f);
        yield return new WaitForSeconds(0.4f);
        ammunition_display.sprite = ammunition_sprites[ammo];
        yield return new WaitForSeconds(0.1f);
        gate_left.LeanMoveLocalX(-72.44f, 0.3f).setEaseInBounce();
        gate_right.LeanMoveLocalX(35.83f, 0.3f).setEaseInBounce();
        inner_.LeanRotate(new Vector3(0, 0, 0), 0.3f).setEaseInBounce();
        outer_.LeanRotate(new Vector3(0, 0, -0), 0.3f).setEaseInBounce();
    }
}
