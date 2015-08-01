using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System.Linq;

// I want to believe.

public class Alien : MonoBehaviour
{
    public static Vector3[] Waypoints = null;

    public MonoBehaviour MCC;
    public float        Staggering = 1;
    public float        Speed = 15;
    public int          Armor = 3;
    public int          Damage = 1;
    public float        AttackSpeed = 1;
    public float        Range = 5;

    protected int   _lastIdx = 0;
    protected bool  _attacking = false;
    protected float _lastAttack = 0;
    protected float _stagger;

    void Start()
    {
        transform.forward = Vector3.back;
        _stagger = Random.Range(-1f, 1f) * Staggering;

        if (Waypoints == null)
        {
            var objs = GameObject.FindGameObjectsWithTag("Waypoint");
            Waypoints = objs.Select(o => o.transform.position).OrderByDescending(t => t.z).ToArray();
        }
    }

	protected virtual void Update ()
    {
        if (_attacking)
            return;

        if (Vector3.Distance(transform.position, MCC.transform.position) <= Range)
        {
            if (Time.time - _lastAttack >= AttackSpeed)
            {
                Attack(MCC);
                _lastAttack = Time.time;
            }
            return;
        }

        var direction = Vector3.back;
        while (_lastIdx < Waypoints.Length && transform.position.z <= Waypoints[_lastIdx].z)
            _lastIdx++;

        if (_lastIdx < Waypoints.Length)
        {
            if (_lastIdx + 1 < Waypoints.Length && Vector3.Distance(Waypoints[_lastIdx], transform.position) > Vector3.Distance(Waypoints[_lastIdx + 1], transform.position))
                _lastIdx++;
            var waypoint = Waypoints[_lastIdx];
            waypoint.x += _stagger;
            direction = waypoint - transform.position;
            direction.y = 0;
            direction.Normalize();
        }
        transform.position = transform.position + direction * Speed * Time.deltaTime;
	}

    private void Hit()
    {
        --Armor;
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    public void Attack(MonoBehaviour target)
    {
        var direction = target.transform.position - transform.position;
        direction.y = 0;
        direction.Normalize();
        transform.forward = direction;
        StartCoroutine(AttackAnim(target));
    }

    public IEnumerator AttackAnim(MonoBehaviour target)
    {
        _attacking = true;

        // Anticipation
        yield return StartCoroutine(GoThereInTime(-transform.forward, 0.1f, 5));

        // Rush forward
        yield return StartCoroutine(GoThereInTime(transform.forward, 0.1f, 30));

        if(target != null)
            target.SendMessage("Hit", Damage);

        // Push back
        yield return StartCoroutine(GoThereInTime(-transform.forward, 0.2f, 15));

        _attacking = false;
    }

    private IEnumerator GoThereInTime(Vector3 direction, float duration, float speed)
    {
        var time = Time.time;
        while (Time.time - time < duration)
        {
            transform.position = transform.position + direction * Time.deltaTime * speed;
            yield return null;
        }
    }
}
