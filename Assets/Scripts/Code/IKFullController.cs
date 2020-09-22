using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IKFullController : MonoBehaviour
{
    [SerializeField] private Transform _bodyTargetTransform;
    [SerializeField] private Transform _headTargetTransform;

    public List<Motor> Motors;

    public List<double> radianAnglesAfterCalculation;
    public Vector3 LastTargetPosition;
    public Vector3 LastHeadTargetPosition;

    public bool turnNegativeOn;

    public float SamplingDistance = 5f;
    public float LearningRate = 100f;
    private const float DistanceThreshold = 0.01f;
    public List<int> indexesToIgnore;
    private int headStatIndex;

    private void Start()
    {
        if (Motors.Any(x => x.isHeadMotor))
            headStatIndex = Motors.IndexOf(Motors.First(x => x.isHeadMotor));
    }

    private void Update()
    {
        InverseKinematics(_bodyTargetTransform.position, false);

        if (Motors.Any(x => x.isHeadMotor))
            InverseKinematics(_headTargetTransform.position, true);
    }

    public List<Motor> GetMotor(bool isHead)
    {
        if (isHead)
        {
            return Motors.ToList();
        }

        return Motors.Where(x => x.isHeadMotor == isHead).ToList();
    }

    public Motor GetMotor(bool isHead, int index)
    {
        return GetMotor(isHead).ElementAt(index);
    }

    public List<float> GetMotorsAngles(bool isHead)
    {
        return GetMotor(isHead).Select(x => x.Angle).ToList();
    }

    public Vector3 ForwardKinematics(bool IsHead)
    {
        Vector3 prevPoint = GetMotor(IsHead, 0).transform.position;
        Quaternion rotation = Quaternion.identity;
        for (int i = 1; i < GetMotor(IsHead).Count; i++)
        {
            var Motor = GetMotor(IsHead, i - 1);
            var MotorNextPoint = GetMotor(IsHead, i);


            // Rotates around a new axis
            rotation *= Quaternion.AngleAxis(Motor.Angle, Motor.RotationAxis);
            Vector3 nextPoint = prevPoint + rotation * MotorNextPoint.StartOffset;

            prevPoint = nextPoint;
        }
        return prevPoint;
    }

    public float DistanceFromTarget(Vector3 target, bool isHead)
    {
        Vector3 point = ForwardKinematics(isHead);
        return Vector3.Distance(point, target);
    }

    public float PartialGradient(Vector3 target, int i, bool isHead)
    {
        float angleToRestore = GetMotor(isHead, i).Angle;

        float targetDistance = DistanceFromTarget(target, isHead);

        var Motor = GetMotor(isHead, i);
        Motor.AddAngle(Motor.Angle + SamplingDistance, isHead);

        float targetDistance2 = DistanceFromTarget(target, isHead);

        float gradient = (targetDistance2 - targetDistance) / SamplingDistance;

        GetMotor(isHead, i).AddAngle(angleToRestore, isHead);

        return gradient;
    }

    public void InverseKinematics(Vector3 target, bool isHead)
    {
        if (DistanceFromTarget(target, isHead) < DistanceThreshold)
            return;

        radianAnglesAfterCalculation = new List<double>();

        for (int i = GetMotor(isHead).Count - 1; i >= 0; i--)
        {
            float gradient = PartialGradient(target, i, isHead);
            var motor = GetMotor(isHead, i);

            motor.AddAngle(motor.Angle - LearningRate * gradient, isHead);
            motor.AddAngle(Mathf.Clamp(motor.Angle, motor.MinAngle, motor.MaxAngle), isHead);

            if (DistanceFromTarget(target, isHead) < DistanceThreshold)
                return;

            if (!isHead || (isHead && i >= headStatIndex))
            {
               motor.ApplyRotation();
            }
        }
    }

    public List<double> GetAnglesResultsInRadians()
    {
        radianAnglesAfterCalculation = new List<double>();
        var GetAllMotorsButEnds = GetMotor(true).Where(x => !x.isEnd).Select(x => x.Angle);

        foreach (var item in GetAllMotorsButEnds)
        {
            radianAnglesAfterCalculation.Add(ConvertInRadians(item));
        }

        return radianAnglesAfterCalculation;
    }

    public double ConvertInRadians(float degrees)
    {
        double radians = (Math.PI / 180) * degrees;
        return (radians);
    }
}