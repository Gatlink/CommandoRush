using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class MobileCommandCenter : MonoBehaviour
{
    public GameObject SoldierPrefab;
    public int SoldierPerUnit = 4;
    public float TimeBetweenSpawn = 0.5f;

    private Transform[] _covers;
    private Button[] _orderButtons;

    void Start()
    {
        var orderObjs = GameObject.FindGameObjectsWithTag("Order");
        _orderButtons = orderObjs.Select(o => o.GetComponent<Button>()).ToArray();

        GameLogic.Instance.AssaultEnded += ActivateOrderButtons;

        var covers = new List<Transform>();
        foreach (Transform t in transform)
        {
            if (t.gameObject.layer == LayerMask.NameToLayer("Covers"))
                covers.Add(t);
        }
        _covers = covers.ToArray();

        StartSpawning();
    }

    void Update()
    {
        if (GameLogic.Instance.Assault)
            DeactivateOrderButtons();
    }

    #region Reinforcement

    public void CallReinforcement()
    {
        GameLogic.Instance.AssaultEnded += StartSpawning;
        DeactivateOrderButtons();
    }

    public void StartSpawning()
    {
        StartCoroutine(SpawnUnits());
    }

    private IEnumerator SpawnUnits()
    {
        for (var i = 0; i < SoldierPerUnit; ++i)
        {
            SpawnSoldier();
            yield return new WaitForSeconds(TimeBetweenSpawn);
        }
        GameLogic.Instance.AssaultEnded -= StartSpawning;
    }

    private void SpawnSoldier()
    {
        Vector3 slot;
        if (!IsAnySlotFree(out slot))
            return;

        var soldier = Instantiate(SoldierPrefab);
        soldier.transform.position = transform.position;
        GameLogic.Instance.StartMovingSoldier(soldier.transform, slot, cameraFollow:false);
    }

    private bool IsAnySlotFree(out Vector3 slot)
    {
        foreach (var cover in _covers)
        {
            if (GameLogic.Instance.IsAnySlotFree(cover, out slot))
            {
                return true;
            }
        }

        slot = Vector3.zero;
        return false;
    }

    #endregion

    #region Resupply

    public void AskForResupply()
    {
        GameLogic.Instance.AssaultEnded += Resupply;
        DeactivateOrderButtons();
    }

    private void Resupply()
    {
        foreach (var soldier in GameObject.FindGameObjectsWithTag("Soldier"))
        {
            soldier.GetComponent<Soldier>().AmmoCount = Soldier.MaxAmmo;
        }
        GameLogic.Instance.AssaultEnded -= Resupply;
    }

    #endregion

    private void ActivateOrderButtons()
    {
        foreach (var button in _orderButtons)
            button.interactable = true;
    }

    private void DeactivateOrderButtons()
    {
        foreach (var button in _orderButtons)
            button.interactable = false;
    }
}