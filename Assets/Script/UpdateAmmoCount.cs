using UnityEngine;
using UnityEngine.UI;

public class UpdateAmmoCount : MonoBehaviour
{
    private Soldier _soldier;
    private Text _text;

    void Start()
    {
        _soldier = GetComponentInParent<Soldier>();
        _text = GetComponent<Text>();
    }

    void LateUpdate()
    {
        _text.text = _soldier.AmmoCount.ToString();
    }
}