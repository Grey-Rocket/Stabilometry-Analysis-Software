﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StabilometryAnalysis.Axes;

namespace StabilometryAnalysis
{
    // Formulas for ellipse was taken from https://www1.udel.edu/biology/rosewc/kaap686/reserve/cop/center%20of%20position%20conf95.pdf
    public class EllipseValues
    {
        #region Variables
        public float area = 0;

        public Vector2 mean = Vector2.zero;
        public Vector2[] eigenVectors = null;

        public float semiMajorAxis = -1;
        public float semiMinorAxis = -1;

        private const float ellipse95Multiplicator = 5.991f;
        #endregion


        public EllipseValues(float area)
        {
            this.area = area;
        }

        public EllipseValues(List<DataPoint> stabilometryData)
        {
            if (stabilometryData.Count < 1)
                return;
            // else        

            List<Vector2> normalizedVector = GetVectorList(stabilometryData);

            mean = CalculateMean(normalizedVector);

            CMatrix cMatrix = CalculateCMatrix(normalizedVector, mean);

            float[] eigenValues = CalculateEigenValues(cMatrix);

            semiMajorAxis = Mathf.Sqrt(eigenValues[0] / (normalizedVector.Count - 1));
            semiMinorAxis = Mathf.Sqrt(eigenValues[1] / (normalizedVector.Count - 1));

            // Calculates the smallest area of ellipse where at least 95% of the data points are located.
            this.area = ellipse95Multiplicator * Mathf.PI * semiMajorAxis * semiMinorAxis;

            eigenVectors = new Vector2[2];

            this.eigenVectors[0] = CalculateEigenVector(eigenValues[0], cMatrix);
            this.eigenVectors[1] = CalculateEigenVector(eigenValues[1], cMatrix);
        }

        public EllipseValues(float area, Vector2[] eigenVectors, float semiMajorAxis, float semiMinorAxis)
        {
            this.area = area;
            this.eigenVectors = eigenVectors;
            this.semiMajorAxis = semiMajorAxis;
            this.semiMinorAxis = semiMinorAxis;
        }

        public List<Vector2> GetEllipsePoints(int numberOfPoints)
        {
            List<float> radianValues = GetRadianValues(numberOfPoints);

            return CalculateEllipsePoints(radianValues, this.semiMajorAxis, this.semiMinorAxis, this.eigenVectors);
        }

        private static Vector2 CalculateMean(List<Vector2> stabilometryData)
        {
            Vector2 result = new Vector2();

            foreach (Vector2 point in stabilometryData)
                result += point;

            return result / stabilometryData.Count;
        }

        private static CMatrix CalculateCMatrix(List<Vector2> stabilometryData, Vector2 mean)
        {
            float Cxx = 0;
            float Cxy = 0;
            float Cyy = 0;

            foreach (Vector2 point in stabilometryData)
            {
                Vector2 modifiedPoint = point - mean;
                Cxx += Mathf.Pow(modifiedPoint.x, 2);
                Cxy += modifiedPoint.x * modifiedPoint.y;
                Cyy += Mathf.Pow(modifiedPoint.y, 2);
            }

            return new CMatrix(Cxx, Cxy, Cyy);
        }

        /// <summary>
        /// Calculates eigenVectors from cMatrix. The first eigen value is larger than the second.
        /// </summary>
        /// <param name="cMatrix"></param>
        /// <returns></returns>
        private static float[] CalculateEigenValues(CMatrix cMatrix)
        {
            float[] result = new float[2];

            float firstPart = (cMatrix.Cxx + cMatrix.Cyy) / 2f;

            float secondPart = Mathf.Sqrt(Mathf.Pow(cMatrix.Cxy, 2) + Mathf.Pow((cMatrix.Cxx - cMatrix.Cyy) / 2, 2));

            result[0] = firstPart + secondPart;
            result[1] = firstPart - secondPart;

            return result;
        }

        /// <summary>
        /// Returns list of Vector2 points that start at 0,0;
        /// </summary>
        /// <param name="stabilometryData"></param>
        /// <returns></returns>
        private static List<Vector2> GetVectorList(List<DataPoint> stabilometryData)
        {
            List<Vector2> result = new List<Vector2>();

            foreach (DataPoint point in stabilometryData)
                result.Add(point.GetVecotor2(BOTH));

            return result;
        }

        /// <summary>
        /// Calculates points that will be used for drawing ellipse.
        /// </summary>
        /// <param name="semiMajorAxis"></param>
        /// <param name="semiMinorAxis"></param>
        /// <param name="eigenvalues"></param>
        /// <param name="cMatrix"></param>
        /// <returns></returns>
        private static List<Vector2> CalculateEllipsePoints(List<float> radianValues, float semiMajorAxis, float semiMinorAxis, Vector2[] eigenVectors)
        {
            List<Vector2> result = new List<Vector2>();

            float sqrtMultip = Mathf.Sqrt(ellipse95Multiplicator);

            foreach (float value in radianValues)
            {

                Vector2 finalValue = sqrtMultip * (Mathf.Cos(value) * semiMajorAxis * eigenVectors[0]
                    + Mathf.Sin(value) * semiMinorAxis * eigenVectors[1]);

                result.Add(finalValue);
            }

            return result;
        }

        private static Vector2 CalculateEigenVector(float eigenvalue, CMatrix cMatrix)
        {
            Vector2 result = new Vector2();

            float tempValue = (cMatrix.Cxx - eigenvalue) / cMatrix.Cxy;

            result.x = 1f / Mathf.Sqrt(1 + Mathf.Pow(tempValue, 2));
            result.y = -tempValue * result.x;

            return result;
        }

        /// <summary>
        /// Returns the number of radian values starting from 0 to 2pi. 
        /// Values have the same distance between them.
        /// </summary>
        /// <param name="startValue"></param>
        /// <param name="valueNumber"></param>
        /// <returns></returns>
        private static List<float> GetRadianValues(int valueNumber)
        {
            List<float> result = new List<float>();

            if (valueNumber <= 0)
            {
                Debug.LogError($"{valueNumber} is lower than 0");
                return result;
            }

            float increment = 2 * Mathf.PI / valueNumber;

            for (int i = 0; i < valueNumber; i++)
                result.Add(increment * i);

            return result;
        }

        #region Testing
        /// <summary>
        /// Function for testing functions
        /// </summary>
        public static void TestFunctions()
        {
            List<DataPoint> dataPoints = new List<DataPoint>();
            dataPoints.Add(new DataPoint(1, 1, 1));
            dataPoints.Add(new DataPoint(2, 1, 2));
            dataPoints.Add(new DataPoint(3, 2, 1));
            dataPoints.Add(new DataPoint(4, 5, -1));

            List<Vector2> normalizedVector = GetVectorList(dataPoints);

            //test CalculateMean
            Vector2 mean = TestCalculateMean(normalizedVector);
            //test CalculateCMatrix

            TestCalculateCMatrix(normalizedVector, mean);

            //For preetier numbers
            CMatrix matrix = new CMatrix(17, -8, 5);

            //test CalculateEigenVectors
            float[] eigenValues = TestCalculateEigenValues(matrix);
            //test CalculateEllipsePoints

            float semiMajorAxis = Mathf.Sqrt(eigenValues[0] / (normalizedVector.Count - 1));
            float semiMinorAxis = Mathf.Sqrt(eigenValues[1] / (normalizedVector.Count - 1));

            TestAreaSize(semiMajorAxis, semiMinorAxis);

            Vector2[] eigenVectors = TestCalculateEigenVector(eigenValues, matrix);

            List<float> radianValues = TestGetRadianValues();

            //TestCalculateEllipsePoints(radianValues, semiMajorAxis, semiMinorAxis, eigenVectors);
        }

        private static Vector2 TestCalculateMean(List<Vector2> data)
        {
            Vector2 result = new Vector2(2.25f, 0.75f);
            Vector2 newValue = CalculateMean(data);

            bool same = (result == newValue);

            if (!same)
                Debug.LogError($"{result} vs {newValue}");

            Debug.Log($"CalcuateMean works {same}");

            return result;
        }

        private static void TestCalculateCMatrix(List<Vector2> stabilometryData, Vector2 mean)
        {
            CMatrix result = new CMatrix(10.75f, -6.75f, 4.75f);
            CMatrix test = CalculateCMatrix(stabilometryData, mean);

            bool same = (test.Cxx == result.Cxx && test.Cxy == result.Cxy && test.Cyy == result.Cyy);

            if (!same)
                Debug.LogError($"{result.Cxx}, {result.Cxy}, {result.Cyy} vs {test.Cxx}, {test.Cxy}, {test.Cyy}");

            Debug.Log($"CMatrix is the same {same}");

        }

        private static float[] TestCalculateEigenValues(CMatrix matrix)
        {
            float[] result = new float[2] { 21, 1 };

            float[] test = CalculateEigenValues(matrix);

            bool same = (result[0] == test[0] && result[1] == test[1]);

            if (!same)
                Debug.LogError($"{result} vs {test}");

            Debug.Log($"Eigenvalues are the same {same}");

            return result;
        }

        private static void TestCalculateEllipsePoints(List<float> radianValues, float semiMajorAxis, float semiMinorAxis, Vector2[] eigenVectors)
        {
            List<Vector2> result = new List<Vector2>();

            float sqrtMultip = Mathf.Sqrt(ellipse95Multiplicator);

            foreach (float value in radianValues)
            {

                Vector2 finalValue = sqrtMultip * (Mathf.Cos(value) * semiMajorAxis * eigenVectors[0]
                    + Mathf.Sin(value) * semiMinorAxis * eigenVectors[1]);

                result.Add(finalValue);
            }

            List<Vector2> testResult = CalculateEllipsePoints(radianValues, semiMajorAxis, semiMinorAxis, eigenVectors);

            ListsIdentical(result, testResult);
        }

        private static Vector2[] TestCalculateEigenVector(float[] eigenValue, CMatrix matrix)
        {
            Vector2[] result = new Vector2[2];
            result[0] = new Vector2(
                2 * Mathf.Sqrt(5) / 5,
                -Mathf.Sqrt(5) / 5
                );

            result[1] = new Vector2(
                Mathf.Sqrt(5) / 5,
                 2 * Mathf.Sqrt(5) / 5
                );

            Vector2 test1 = CalculateEigenVector(eigenValue[0], matrix);
            bool firstSame = (test1 == result[0]);

            if (!firstSame)
                Debug.LogError($"{result[0]} vs {test1}");

            Debug.Log($"EigenVectors0 are the same {firstSame}");

            Vector2 test2 = CalculateEigenVector(eigenValue[1], matrix);

            bool secondSame = (test2 == result[1]);

            if (!secondSame)
                Debug.LogError($"{result[1]} vs {test2}");

            Debug.Log($"EigenVectors1 are the same {secondSame }");

            return result;
        }

        private static List<float> TestGetRadianValues()
        {
            List<float> result = new List<float>();
            result.Add(0);
            result.Add(Mathf.PI / 2);
            result.Add(Mathf.PI);
            result.Add(3 * Mathf.PI / 2);

            List<float> testData = GetRadianValues(4);

            if (testData.Count != result.Count)
                Debug.LogError($"test data has {testData.Count} data while result has {result.Count} data");

            for (int i = 0; i < result.Count; i++)
            {
                if (result[i] != testData[i])
                    Debug.LogError($"{result[i]} is not the same as {testData[i]}");
            }

            return result;
        }

        private static void TestAreaSize(float semiMajorAxis, float semiMinorAxis)
        {
            // Calculates the smallest area of ellipse where at least 95% of the data points are located.
            float area = ellipse95Multiplicator * Mathf.PI * semiMajorAxis * semiMinorAxis;

            float testArea = ellipse95Multiplicator * Mathf.PI * Mathf.Sqrt(21f / 3f) * Mathf.Sqrt(1f / 3f);

            bool same = (area == testArea);
            Debug.Log($"Eigenvalues are the same {same}");

            if (!same)
                Debug.LogError($"{area} vs {testArea}");
        }

        private static bool ListsIdentical(List<Vector2> first, List<Vector2> second)
        {
            if (first.Count != second.Count)
            {
                Debug.LogError($"{first.Count} vs {second.Count}");
                return false;
            }

            for (int i = 0; i < first.Count; i++)
                if (first[i] != second[i])
                {
                    Debug.LogError($"{first[i]} is not the same as {second[i]}");
                    return false;
                }

            return true;
        }
        #endregion
    }
}