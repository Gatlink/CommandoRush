using UnityEngine;
using UnityEngine.UI;

public class UpdateMoveCount : MonoBehaviour
{
    private Text _text;

    void Start()
    {
        _text = GetComponent<Text>();
    }

    void LateUpdate()
    {
        var soldier = GameLogic.Instance.Selected;
        if (soldier != null)
            _text.text = soldier.MoveCount.ToString();
    }
}