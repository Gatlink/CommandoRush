using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AliensSpawner : MonoBehaviour
{
    public static List<Transform> Aliens;

    public GameObject AlienPrefab;
    public int Count = 10;
    public float TimeBetweenSpawn = 1;

    public bool AreAliensActive { get { return transform.childCount > 0; } }

    void Start()
    {
        Aliens = new List<Transform>();
    }

    void LateUpdate()
    {
        var aliens = Aliens.ToArray();
        var newList = new List<Transform>();
        for (var i = aliens.Length - 1; i >= 0; --i)
        {
            if (aliens[i].GetComponent<Alien>().HitPoints <= 0)
                Destroy(aliens[i].gameObject);
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
        var alien = Instantiate(AlienPrefab);
        var position = transform.position;
        position.x = lane;
        alien.transform.position = position;
        alien.transform.parent = transform;
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
    }
}
