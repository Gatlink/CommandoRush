using UnityEngine;
using System.Collections;

public class CameraBehaviour : MonoBehaviour
{
    public Transform Target;
    public float Speed = 10;
    public Vector3 Distance = new Vector3(0, 10, -5);
    public float ZoomSpeed = 1;
    public float MinSize = 5;
    public float MaxSize = 20;

    private Vector3 _velocity = Vector3.up;
	
	void Update ()
    {
        var direction = Vector3.zero;
        if (Input.GetKey(KeyCode.LeftArrow))
            direction += Vector3.left;
        if (Input.GetKey(KeyCode.UpArrow))
            direction += Vector3.forward;
        if (Input.GetKey(KeyCode.RightArrow))
            direction += Vector3.right;
        if (Input.GetKey(KeyCode.DownArrow))
            direction += Vector3.back;

        if (Target != null && direction == Vector3.zero)
        {
            var target = Target.position + Distance;
            var smoothTime = Vector3.Distance(transform.position, target) / Speed;
            transform.position = Vector3.SmoothDamp(transform.position, target, ref _velocity, smoothTime);
        }
        else
        {
            Target = null;
            transform.position = Vector3.SmoothDamp(transform.position, transform.position + direction, ref _velocity, 1 / (Speed * 2));
        }

        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - Input.mouseScrollDelta.y * ZoomSpeed, MinSize, MaxSize); 
	}
}
