using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Soldier : MonoBehaviour
{
    private static GameObject[] Covers;

    public static int MaxAmmo = 60;
    public static int MaxMove = 3;

    public float    Speed = 10;
    public float    Range = 5;
    public float    RateOfFire = 0.5f;
    public int      AmmoCount;
    public int      MoveCount;
    public int      Armor = 5;

    private Transform   _target;
    private Light       _muzzleFlash;
    private GameObject  _info;
    private int         _currentCoverIndex;

    void Awake()
    {
        if (Covers == null)
        {
            var objs = GameObject.FindGameObjectsWithTag("Cover");
            Covers = objs.OrderBy(o => o.transform.position.z).ToArray();
        }

        AmmoCount = MaxAmmo;
        MoveCount = MaxMove + 1;
        _muzzleFlash = GetComponentInChildren<Light>();
        _info = GetComponentInChildren<Canvas>().gameObject;
        _info.SetActive(false);
        _currentCoverIndex = -1;

        GameLogic.Instance.AssaultEnded += ResetMovement;
    }

    void Update()
    {
        if (!GameLogic.Instance.Assault || AmmoCount == 0)
            return;

        if (_target != null)
        {
            var direction = _target.position - transform.position;
            direction.y = 0;
            direction.Normalize();
            transform.forward = direction;
            return;
        }

        foreach (var alien in AliensSpawner.Aliens)
        {
            if (DistanceToTarget(alien.position) <= Range)
            {
                _target = alien;
                StartCoroutine(Shoot());
                break;
            }
        }
    }

    #region Move

    public void StartMoving(GameObject cover)
    {
        StartCoroutine(Move(cover));
    }

    public IEnumerator Move(GameObject cover)
    {
        if (_currentCoverIndex >= 0 && Covers[_currentCoverIndex] == cover)
            yield break;

        var path = BuildPath(cover);
        if (path == null || path.Count > MoveCount)
            yield break;

        while (path.Any())
        {
            var slot = path.Dequeue();
            yield return StartCoroutine(MoveTo(slot));
            --MoveCount;
        }

        if (cover == Covers[Covers.Length - 1])
            GameLogic.Instance.EndGame();
    }

    private Queue<Vector3> BuildPath(GameObject cover)
    {
        var checkedIdx = _currentCoverIndex;
        var path = new Queue<Vector3>();
        var destIdx = FindCoverIndex(cover);
        var destZ = cover.transform.position.z;
        var currentZ = _currentCoverIndex < 0 ? float.MinValue : Covers[_currentCoverIndex].transform.position.z;

        System.Func<int> nextIdx = () => destIdx > _currentCoverIndex ? checkedIdx + 1 : checkedIdx - 1;
        System.Func<bool> isNextOnSameLine = delegate ()
        {
            var next = nextIdx();
            var pos = Covers[checkedIdx].transform.position;
            return next < Covers.Length && next >= 0 && pos.z == Covers[next].transform.position.z;
        };

        while (checkedIdx < 0 || Covers[checkedIdx] != cover)
        {
            checkedIdx = nextIdx();
            if (checkedIdx < 0 || checkedIdx >= Covers.Length) return null; // no more covers

            var z = Covers[checkedIdx].transform.position.z;
            if ((currentZ < destZ && z > destZ) || (currentZ > destZ && z < destZ)) return null; // too far
            if (Covers[checkedIdx] != cover && _currentCoverIndex >= 0 && z == currentZ) continue; // Same line as start, skip

            // check if we are on the same line as the destination
            if (z == destZ && Covers[checkedIdx] != cover)
                continue;

            Vector3 slot;
            if (GameLogic.Instance.IsAnySlotFree(Covers[checkedIdx].transform, out slot))
            {
                path.Enqueue(slot);
                if (Covers[checkedIdx] != cover && isNextOnSameLine())
                    checkedIdx = nextIdx();
            }
            else if (!isNextOnSameLine())
                return null;
        }

        _currentCoverIndex = checkedIdx;
        return path;
    }

    private int FindCoverIndex(GameObject cover)
    {
        if (_currentCoverIndex >= 0 && cover == Covers[_currentCoverIndex])
            return _currentCoverIndex;

        for (var i = 1; i < Covers.Length; ++i)
        {
            if (_currentCoverIndex + i < Covers.Length && Covers[_currentCoverIndex + i] == cover)
                return _currentCoverIndex + i;

            if (_currentCoverIndex - i >= 0 && Covers[_currentCoverIndex - i] == cover)
                return _currentCoverIndex - i;
        }

        return -1;
    }

    private IEnumerator MoveTo(Vector3 destination)
    {
       destination.y = transform.position.y;
        transform.forward = (destination - transform.position).normalized;
        var velocity = Vector3.forward;
        do
        {
            var smoothTime = Vector3.Distance(transform.position, destination) / Speed;
            var newPos = Vector3.SmoothDamp(transform.position, destination, ref velocity, smoothTime);    
            transform.position = newPos;
            yield return null;
        } while (transform.position != destination);
        transform.forward = Vector3.forward;
    }

    #endregion Move

    #region Shoot

    private IEnumerator Shoot()
    {
        while (_target != null && AmmoCount > 0 && DistanceToTarget(_target.position) <= Range)
        {
            StartCoroutine(MuzzleFlash());
            _target.SendMessage("Hit");
            --AmmoCount;
            yield return new WaitForSeconds(RateOfFire);
        }
        _target = null;
        transform.forward = Vector3.forward;
    }

    private IEnumerator MuzzleFlash()
    {
        _muzzleFlash.intensity = 8;
        yield return new WaitForSeconds(0.1f);
        _muzzleFlash.intensity = 0;
    }

    #endregion Shoot

    #region Utils

    public void SetInfoVisibility(bool visibility)
    {
        _info.SetActive(visibility);
    }

    private float DistanceToTarget(Vector3 target)
    {
        var diff = target - transform.position;
        diff.y = 0;
        return diff.magnitude;
    }

    private void ResetMovement()
    {
        MoveCount = MaxMove;
    }

    #endregion Utils
}