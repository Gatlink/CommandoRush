using UnityEngine;
using System.Collections;
using System.Linq;

// I want to believe.

public class Alien : MonoBehaviour
{
    private static Vector3[] Waypoints = null;

    public float Speed = 15;
    public int HitPoints = 3;

    private int _lastIdx = 0;

    void Start()
    {
        transform.forward = Vector3.back;

        if (Waypoints == null)
        {
            var objs = GameObject.FindGameObjectsWithTag("Waypoint");
            Waypoints = objs.Select(o => o.transform.position).OrderBy(t => t.z).Reverse().ToArray();
        }
    }

	void Update ()
    {
        if (transform.position.z <= 0)
            Destroy(gameObject);

        var direction = Vector3.back;
        
        while (_lastIdx < Waypoints.Length && transform.position.z <= Waypoints[_lastIdx].z)
            _lastIdx++;

        if (_lastIdx < Waypoints.Length)
        {
            if (_lastIdx + 1 < Waypoints.Length && Vector3.Distance(Waypoints[_lastIdx], transform.position) > Vector3.Distance(Waypoints[_lastIdx + 1], transform.position))
                _lastIdx++;
            direction = Waypoints[_lastIdx] - transform.position;
            direction.y = 0;
            direction.Normalize();
        }
        transform.position = transform.position + direction * Speed * Time.deltaTime;
	}

    private void Hit()
    {
        --HitPoints;
    }
}
