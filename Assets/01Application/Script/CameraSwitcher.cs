using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CameraSwitcher : MonoBehaviour
{
    public bool cameraFollow = false;
    public GameObject followTarget;
    Transform target;
    Vector3 followPoint;
    public bool shakeHand = false;
    public bool camPointSync = true;
    public Transform[] points;
    private int currentPointId = 0;   // 0:カメラ初期位置
    private Transform currentPoint;
    public float defaultFieldOfView = 45.0f;
    public float interval = 2.0f;
    public float stability = 0.0f;
    public float rotationSpeed = 2.0f;
    public float minDistance = 0.5f;
    public AnimationCurve fovCurve = AnimationCurve.Linear(1, 30, 10, 30);
	//public bool autoChange = true;

	public OnSwitchCameraDelegate OnSwitchCamera { get; set; }
	public delegate void OnSwitchCameraDelegate(int index);

    void Start()
    {
        // Target information.
        target = followTarget.transform;
        followPoint = target.position;
        ChangePosition(points[currentPointId]);
    }

    void Update()
    {
        if (shakeHand)
        {
            var param = Mathf.Exp(-rotationSpeed * Time.deltaTime);
            followPoint = Vector3.Lerp(target.position, followPoint, param);
        }
        else
        {
            followPoint = target.position;
        }

        if (cameraFollow)
        {
            transform.LookAt(followPoint);  
        }

        if (camPointSync)
        {
            PositionSync();
        }
    }

    private void PositionSync()
    {
        currentPoint = points[currentPointId].transform;
        transform.position = currentPoint.transform.position;
        if (!cameraFollow)
        {
            transform.rotation = currentPoint.transform.rotation;
        }     
    }

    public void SwitchCamera(int index)
    {
        SwitchCameraOnForceStable(index, false);
	}

    public void SwitchCameraOnForceStable(int index, bool forceStable = false)
    {
        if (index <= points.Length - 1)
        {
            currentPoint = points[index].transform;
            currentPointId = index;
            ChangePosition(points[index], false);

			if(OnSwitchCamera != null) {
				OnSwitchCamera.Invoke(index);
			}
		}
    }

    public void SwitchCameraOnTarget(int index, Transform followTarget, bool forceStable = false)
    {
        if (index <= points.Length - 1)
        {
            currentPoint = points[index].transform;
            currentPointId = index;
            ChangePositionOnTarget(points[index], followTarget, forceStable);
        }
    }

    // Change the camera position.
    private void ChangePosition(Transform destination, bool forceStable = false)
    {
        // Do nothing if disabled.
        if (!enabled) return;

        // Move to the point.
        transform.position = destination.position;

		if(cameraFollow) {
			transform.LookAt(followPoint);
		} else {
			transform.rotation = currentPoint.transform.rotation;
		}

		// Snap if stable; Shake if unstable.
		if (UnityEngine.Random.value < stability || forceStable)
            followPoint = target.position;
        else
            followPoint += UnityEngine.Random.insideUnitSphere;

        SetFieldOfView(cameraFollow);
    }

    private void ChangePositionOnTarget(Transform destination, Transform followTarget, bool forceStable = false)
    {
        // Do nothing if disabled.
        if (!enabled) return;

        forceStable = false;
        // Move to the point.
        transform.position = destination.position;

        // Snap if stable; Shake if unstable.
        if (UnityEngine.Random.value < stability || forceStable)
            followPoint = target.position;
        else
            followPoint += UnityEngine.Random.insideUnitSphere;

        SetFieldOfView(cameraFollow);
    }

    //// Update the FOV depending on the distance to the target.(When Camerafollow Mode)
    private void SetFieldOfView(bool isFollow)
    {
		Camera cam = GetComponentInChildren<Camera>();

        if (isFollow)
        {
            var dist = Vector3.Distance(target.position, transform.position);

			if(cam != null) {
				cam.fieldOfView = fovCurve.Evaluate(dist);
			}
        }
        else
        {
			if(cam != null) {
				cam.fieldOfView = defaultFieldOfView;
			}
        }
    }

    public Transform GetPointTransform(int index)
    {
        if (index <= points.Length - 1)
        {
            print("No Index");
            return currentPoint;
        }
        return points[index].transform;
    }

    // Choose a point other than the current.
    Transform ChooseAnotherPoint(Transform current)
    {
        while (true)
        {
            var next = points[UnityEngine.Random.Range(0, points.Length)];
            var dist = Vector3.Distance(next.position, target.position);
            if (next != current && dist > minDistance) return next;
        }
    }

    // Auto-changer.
    IEnumerator AutoChange()
    {
        for (var current = points[0]; true; current = ChooseAnotherPoint(current))
        {
            ChangePosition(current);
            yield return new WaitForSeconds(interval);
        }
    }

    public void StartAutoChange()
    {
        StartCoroutine("AutoChange");
    }

    public void StopAutoChange()
    {
        StopCoroutine("AutoChange");
    }
}
