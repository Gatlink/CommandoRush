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

    public static float[] Lanes = { -2.6f, 0, 2.6f };

    public LayerMask    CoversLayer;
    public LayerMask    UnitsLayer;

    private CameraBehaviour _camera;
    private Soldier         _selected;
    private AliensSpawner   _spawner;
    private bool            _assaultStarted = false;

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
	}
	
	void Update ()
    {
        if (!Assault && _assaultStarted)
        {
            OnAssaultEnded();
            _assaultStarted = false;
        }

        if (!Assault && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, UnitsLayer))
            {
                if (_selected != null)
                    _selected.SetInfoVisibility(false);

                var collider = hit.collider;
                _selected = collider.GetComponent<Soldier>();
                _selected.SetInfoVisibility(true);
            }
            else if (_selected != null && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, CoversLayer))
            {
                var collider = hit.collider;
                Vector3 target;
                if (IsAnySlotFree(collider.transform, out target))
                    StartMovingSoldier(_selected.transform, target);
            }
            else if (_selected != null)
            {
                _selected.SetInfoVisibility(false);
                _selected = null;
            }
        }
	}

    public void StartAssault()
    {
        _spawner.StartSpawning();
        _assaultStarted = true;
    }

    public void StartMovingSoldier(Soldier soldier, Vector3 target, bool cameraFollow = true)
    {
        _camera.Target = soldier.transform;
        soldier.StartMoving(target);
    }

    public void StartMovingSoldier(Transform soldier, Vector3 target, bool cameraFollow = true)
    {
        StartMovingSoldier(soldier.GetComponent<Soldier>(), target, cameraFollow);
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
}
