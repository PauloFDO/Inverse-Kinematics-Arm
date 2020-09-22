using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Motor : MonoBehaviour
{
    public Vector3 RotationAxis;
    public Vector3 StartOffset;
    public Transform _transform;
    public char _rotationAxis;
    public bool isHeadMotor;
    public bool isEnd;

    public float MinAngle;
    public float MaxAngle;
    public float Angle;

    private void Awake()
    {
        _transform = this.transform;
    }

    private void Start()
    {
        SetUpAngle();
    }

    private void Update()
    {
        StartOffset = _transform.localPosition;
    }

    public void SetUpAngle()
    {
        if (_rotationAxis == 'x')
        {
            Angle = transform.localRotation.eulerAngles.x;
        }
        else if (_rotationAxis == 'y')
        {
            Angle = transform.localRotation.eulerAngles.y;
        }
        else if (_rotationAxis == 'z')
        {
            Angle = transform.localRotation.eulerAngles.z;
        }
    }

    public void AddAngle(float angle, bool isHead)
    {
        if (isHeadMotor == isHead)
            Angle = angle;
    }

    public void ApplyRotation()
    {
        switch (_rotationAxis)
        {
            case 'x':
                transform.localEulerAngles = new Vector3(Angle, 0, 0);
                break;
            case 'y':
                transform.localEulerAngles = new Vector3(0, Angle, 0);
                break;
            case 'z':
                transform.localEulerAngles = new Vector3(0, 0, Angle);
                break;
        }
    }
}