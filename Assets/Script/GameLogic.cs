using UnityEngine;
using System.Collections;
using System.Linq;

public class GameLogic : MonoBehaviour
{
    #region Singleton

    public static GameLogic Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    #endregion

    public static float[]   Lanes = { -2.6f, 0, 2.6f };
    public static float     RangeBetweenCovers = 5.35f;

    public LayerMask    CoversLayer;
    public LayerMask    UnitsLayer;
    public GameObject   SoldierInfo;
    public GameObject   GameOverScreen;

    private CameraBehaviour _camera;
    private AliensSpawner   _spawner;
    private bool            _assaultStarted = false;

    private Soldier _selected;
    public Soldier Selected
    {
        get { return _selected; }
        set
        {
            if (_selected == value)
                return;

            SoldierInfo.SetActive(value != null);
            _selected = value;
        }
    }

    public bool Assault { get { return _spawner.AreAliensActive; } }

    public delegate void AssaultEndedEventHandler();
    public event AssaultEndedEventHandler AssaultEnded;
    private void OnAssaultEnded()
    {
        if (AssaultEnded != null)
            AssaultEnded();
    }

	void Start ()
    {
        Random.seed = (int) Time.realtimeSinceStartup;
        _camera = Camera.main.GetComponent<CameraBehaviour>();
        _spawner = GameObject.FindGameObjectWithTag("AliensSpawner").GetComponent<AliensSpawner>();
        SoldierInfo.SetActive(false);
	}
	
	void Update ()
    {
        if (!Assault && _assaultStarted)
        {
            OnAssaultEnded();
            _assaultStarted = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, UnitsLayer))
            {
                if (Selected != null)
                    Selected.SetInfoVisibility(false);

                var collider = hit.collider;
                Selected = collider.GetComponent<Soldier>();
                Selected.SetInfoVisibility(true);
            }
            else if (!Assault && Selected != null && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, CoversLayer))
            {
                var collider = hit.collider;
                StartMovingSoldier(Selected.transform, collider.gameObject);
            }
        }
	}

    public void StartAssault()
    {
        _spawner.StartSpawning();
        _assaultStarted = true;
    }

    public void StartMovingSoldier(Soldier soldier, GameObject cover, bool cameraFollow = true)
    {
        _camera.Target = soldier.transform;
        soldier.StartMoving(cover);
    }

    public void StartMovingSoldier(Transform soldier, GameObject cover, bool cameraFollow = true)
    {
        StartMovingSoldier(soldier.GetComponent<Soldier>(), cover, cameraFollow);
    }

    public bool IsAnySlotFree(Transform cover, out Vector3 target)
    {
        foreach (Transform slot in cover)
        {
            if (!Physics.OverlapSphere(slot.position, 0.1f, UnitsLayer).Any())
            {
                target = slot.position;
                return true;
            }
        }
        target = Vector3.zero;
        return false;
    }

    public void EndGame()
    {
        Time.timeScale = 0;
        GameOverScreen.SetActive(true);
    }

    public void Reset()
    {
        GameOverScreen.SetActive(false);

        _spawner.KillEmAll();

        foreach (var soldier in GameObject.FindGameObjectsWithTag("Soldier"))
            Destroy(soldier);

        var terrain = GameObject.FindGameObjectWithTag("Terrain").GetComponent<GenerateTerrain>();
        terrain.Reset();

        var mcc = GameObject.FindGameObjectWithTag("MCC").GetComponent<MobileCommandCenter>();
        mcc.Reset();

        Alien.Waypoints = null;

        Time.timeScale = 1;
    }
}
