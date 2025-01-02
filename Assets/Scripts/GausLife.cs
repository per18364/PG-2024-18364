using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GausLife : MonoBehaviour
{
    private Image _lifebarImg;
    [SerializeField] private Sprite[] _lifeSprite;

    public void ChangeLife(int pos)
    {
        Debug.Log("ChangeLife!");

        if(_lifebarImg == null) _lifebarImg = GetComponent<Image>();

        if(pos < 0 || pos > _lifeSprite.Length) return;
        
        _lifebarImg.sprite = _lifeSprite[pos];
    }
}