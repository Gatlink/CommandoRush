using UnityEngine;
using System.Collections;

public class Soldier : MonoBehaviour
{
    public static int MaxAmmo = 90;
    public static int MaxMove = 3;

    public float    Speed = 10;
    public float    Range = 5;
    public float    RateOfFire = 0.5f;
    public int      AmmoCount;
    public int      MoveCount;

    private Transform   _target;
    private Light       _muzzleFlash;
    private GameObject  _info;

    void Start()
    {
        AmmoCount = MaxAmmo;
        MoveCount = MaxMove;
        _muzzleFlash = GetComponentInChildren<Light>();
        _info = GetComponentInChildren<Canvas>().gameObject;
        _info.SetActive(false);
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

    public void SetInfoVisibility(bool visibility)
    {
        _info.SetActive(visibility);
    }

    public void StartMoving(Vector3 target)
    {
        StartCoroutine(Move(target));
    }

    private IEnumerator Move(Vector3 target)
    {
        target.y = transform.position.y;
        transform.forward = (target - transform.position).normalized;
        var velocity = Vector3.forward;
        do
        {
            var smoothTime = Vector3.Distance(transform.position, target) / Speed;
            transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, smoothTime);
            yield return null;
        } while (transform.position != target);
        transform.forward = Vector3.forward;
    }

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

    private float DistanceToTarget(Vector3 target)
    {
        var diff = target - transform.position;
        diff.y = 0;
        return diff.magnitude;
    }
}