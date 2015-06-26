using UnityEngine;
using UnityEngine.UI;

public class DisableButtonsDuringAssault : MonoBehaviour
{
    public Button[] Buttons;

    public void Update()
    {
        foreach(var button in Buttons)
            button.interactable = !GameLogic.Instance.Assault;
    }
}