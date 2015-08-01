using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AliensSpawner : MonoBehaviour
{
    public static List<Transform> Aliens;

    public GameObject[] AlienPrefabs;
    public int Count = 10;
    public float GrowthFactor = 1.1f;
    public float TimeBetweenSpawn = 1;

    private Transform _mcc;

    public bool AreAliensActive { get { return transform.childCount > 0; } }

    void Start()
    {
        Aliens = new List<Transform>();
        _mcc = GameObject.FindGameObjectWithTag("MCC").transform;
    }

    void LateUpdate()
    {
        var aliens = Aliens.ToArray();
        var newList = new List<Transform>();
        for (var i = aliens.Length - 1; i >= 0; --i)
        {
            var alien = aliens[i].GetComponent<Alien>();
            if (alien.Armor <= 0)
                alien.Die();
            else
                newList.Add(aliens[i]);
        }
        newList.Reverse();
        Aliens = newList;
    }

    public void StartSpawning()
    {
        if (!AreAliensActive)
            StartCoroutine(SpawnWave());
    }

    private GameObject SpawnAlien()
    {
        var lane = GameLogic.Lanes[Random.Range(0, GameLogic.Lanes.Length)];
        var alien = Instantiate(AlienPrefabs[Random.Range(0, AlienPrefabs.Length)]);
        var position = transform.position;
        position.x = lane;
        position.y = alien.transform.localScale.y / 2f;
        alien.transform.position = position;
        alien.transform.parent = transform;
        alien.GetComponent<Alien>().MCC = _mcc.GetComponent<MobileCommandCenter>();
        Aliens.Add(alien.transform);
        return alien;
    }

    private IEnumerator SpawnWave()
    {
        for (var i = 0; i < Count; ++i)
        {
            SpawnAlien();
            yield return new WaitForSeconds(TimeBetweenSpawn);
        }
        Count = (int) Mathf.Floor(Count * GrowthFactor);
    }

    public void KillEmAll()
    {
        foreach (var alien in Aliens)
            alien.GetComponent<Alien>().Armor = 0;
    }
}
