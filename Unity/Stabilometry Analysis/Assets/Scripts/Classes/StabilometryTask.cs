﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Axes;

public class StabilometryTask
{
    public int ID;
    public float
        swayPath,
        swayPathAP,
        swayPathML,
        meanDistance;

    public float
        meanSwayVelocity,
        meanSwayVelocityAP,
        meanSwayVelocityML;

    public float
        swayAverageAmplitudeAP,
        swayAverageAmplitudeML;

    public float
        swayMaximalAmplitudeAP,
        swayMaximalAmplitudeML;

    //95% of data
    public float standardEllipseArea;

    public StabilometryTask(List<DataPoint> stabilometryData)
    {
        swayPath = CalculateSwayPath(stabilometryData, Both);
        swayPathAP = CalculateSwayPath(stabilometryData, AP);
        swayPathML = CalculateSwayPath(stabilometryData, ML);
        meanDistance = CalculateMeanDistance(stabilometryData);

        meanSwayVelocity = CalculateMeanSwayVelocity(stabilometryData, Both);
        meanSwayVelocityAP = CalculateMeanSwayVelocity(stabilometryData, AP);
        meanSwayVelocityML = CalculateMeanSwayVelocity(stabilometryData, ML);

        swayAverageAmplitudeAP = CalculateAverageAmplitude(stabilometryData, AP, swayPathAP);
        swayAverageAmplitudeML = CalculateAverageAmplitude(stabilometryData, ML, swayPathML);

        swayMaximalAmplitudeAP = CalculateMaximalAmplitude(stabilometryData, AP);
        swayMaximalAmplitudeML = CalculateMaximalAmplitude(stabilometryData, ML);

        standardEllipseArea = CalculateStandardEllipseArea(stabilometryData);
    }

    /// <summary>
    /// Sums all the path.
    /// </summary>
    /// <param name="stabilometryData"></param>
    /// <param name="axes"></param>
    /// <returns></returns>
    private float CalculateSwayPath(List<DataPoint> stabilometryData, Axes axes)
    {
        float result = 0f;

        if (stabilometryData.Count <= 1)
            return result;
        //else

        Vector2 previousValue = stabilometryData[0].GetVecotor2(axes);

        for (int i = 1; i < stabilometryData.Count; i++)
        {
            result += Vector2.Distance(previousValue, stabilometryData[i].GetVecotor2(axes));

            previousValue = stabilometryData[i].GetVecotor2(axes);
        }

        return result;
    }

    /// <summary>
    /// Mean distance of from the starting point.
    /// </summary>
    /// <param name="stabilometryData"></param>
    /// <returns></returns>
    private float CalculateMeanDistance(List<DataPoint> stabilometryData)
    {
        float result = 0f;

        if (stabilometryData.Count <= 1)
            return result;
        //else

        Vector2 firstValue = stabilometryData[0].GetVecotor2(Both);

        for (int i = 1; i < stabilometryData.Count; i++)
            result += Vector2.Distance(stabilometryData[i].GetVecotor2(Both), firstValue);

        return result / stabilometryData.Count;
    }

    /// <summary>
    /// Mean velocity.
    /// </summary>
    /// <param name="stabilometryData"></param>
    /// <param name="axes"></param>
    /// <returns></returns>
    private float CalculateMeanSwayVelocity(List<DataPoint> stabilometryData, Axes axes)
    {
        float result = 0f;

        if (stabilometryData.Count <= 1)
            return result;
        //else

        Vector2 previousValue = stabilometryData[0].GetVecotor2(axes);
        float previousTime = stabilometryData[0].time;

        for (int i = 1; i < stabilometryData.Count; i++)
        {
            result += Vector2.Distance(stabilometryData[i].GetVecotor2(axes), previousValue) / (stabilometryData[i].time - previousTime);
            previousValue = stabilometryData[i].GetVecotor2(axes);
            previousTime = stabilometryData[i].time;
        }

        return result / (stabilometryData.Count - 1);
    }


    /// <summary>
    /// Calculates the average amplitude in given direction.
    /// </summary>
    /// <param name="stabilometryData"></param>
    /// <param name="axes"></param>
    /// <param name="swayPath"></param>
    /// <returns></returns>
    private float CalculateAverageAmplitude(List<DataPoint> stabilometryData, Axes axes, float swayPath)
    {

        if (stabilometryData.Count <= 1)
            return 0f;
        //else

        int directionChanges = 1;

        float previousValue = (axes == ML) ? stabilometryData[0].x : stabilometryData[0].y;
        float currentValue = (axes == ML) ? stabilometryData[1].x : stabilometryData[1].y;

        bool valueIncreasing = (currentValue > previousValue);

        for (int i = 1; i < stabilometryData.Count; i++)
        {
            currentValue = (axes == ML) ? stabilometryData[i].x : stabilometryData[i].y;

            if (valueIncreasing && (currentValue < previousValue))
            {
                valueIncreasing = false;
                directionChanges++;
            }
            else if (!valueIncreasing && (currentValue > previousValue))
            {
                valueIncreasing = true;
                directionChanges++;
            }

            previousValue = currentValue;
        }

        return swayPath / directionChanges;
    }

    /// <summary>
    /// Returns the maximum amplitude in given axis.
    /// </summary>
    /// <param name="stabilometryData"></param>
    /// <param name="axes"></param>
    /// <returns></returns>
    private float CalculateMaximalAmplitude(List<DataPoint> stabilometryData, Axes axes)
    {

        if (stabilometryData.Count <= 1)
            return 0f;
        //else

        float maxValue = (axes == ML) ? stabilometryData[0].x : stabilometryData[0].y;
        float minValue = maxValue;

        foreach (DataPoint point in stabilometryData)
        {
            float compareValue = (axes == ML) ? point.x : point.y;

            if (compareValue > maxValue)
                maxValue = compareValue;

            if (compareValue < minValue)
                minValue = compareValue;
        }

        return maxValue - minValue;
    }

    private float CalculateStandardEllipseArea(List<DataPoint> stabilometryData)
    {
        float result = 0f;

        return result;
    }
}
