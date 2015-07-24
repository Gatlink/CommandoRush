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
    public int Armor = 10;

    private GameObject[] _covers;
    private Button[] _orderButtons;

    void Start()
    {
        var orderObjs = GameObject.FindGameObjectsWithTag("Order");
        _orderButtons = orderObjs.Select(o => o.GetComponent<Button>()).ToArray();

        GameLogic.Instance.AssaultEnded += ActivateOrderButtons;

        Reset();
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
        GameObject cover;
        if (!IsAnySlotFree(out cover))
            return;

        var soldier = Instantiate(SoldierPrefab);
        soldier.transform.position = transform.position;
        GameLogic.Instance.StartMovingSoldier(soldier.transform, cover, cameraFollow:false);
    }

    private bool IsAnySlotFree(out GameObject availableCover)
    {
        foreach (var cover in _covers)
        {
            Vector3 slot;
            if (GameLogic.Instance.IsAnySlotFree(cover.transform, out slot))
            {
                availableCover = cover;
                return true;
            }
        }

        availableCover = null;
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

    public void Hit(int damage)
    {
        --Armor;
        if (Armor <= 0)
            GameLogic.Instance.EndGame();
    }

    public void Reset()
    {
        var covers = new List<GameObject>();
        foreach (Transform t in transform)
        {
            if (t.gameObject.layer == LayerMask.NameToLayer("Covers"))
                covers.Add(t.gameObject);
        }
        _covers = covers.ToArray();

        StartSpawning();
    }
}