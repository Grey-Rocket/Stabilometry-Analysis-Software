﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StabilometryAnalysis
{
    using static ChartSupportScript;

    // Note about spawning charts. Start with spawning only one chart (because of speed).
    // This one chart should be stretched trhough the entire space.
    // When clicking on a chart toggler change sizes.
    // When clicking on any chart it opens the menu with data.
    public class StabilometryAnalysisParameterMenuScript : LineChartParentScript
    {
        #region Variables
        public MainScript mainScript { get; set; } = null;

        [SerializeField]
        private AccordionToggler[]
            parameterTogglers = null,
            taskTogglers = null;

        [SerializeField]
        private GameObject
            lineChartPrefab = null,
            chartHolder = null,
            chartMask = null;

        [SerializeField] private ScrollbarScript scrollbarScript = null;

        [SerializeField] private GameObject measurementMenu = null;
        [SerializeField] private AccordionRadioHandler poseRadioHandler = null;
        [SerializeField] private BackgroundBlockerScript backgroundBlocker = null;

        [SerializeField]
        private AccordionDropdownSelector
            minimumDuration = null,
            maximumDuration = null,
            firstDate = null,
            lastDate = null;

        private RectTransform chartHolderRect = null;

        private List<GameObject> instantiatedCharts = null;

        private List<StabilometryMeasurement> patientData = null;
        // Smaller data size based on pose.
        private List<StabilometryMeasurement> relevantData = null;

        private float initialChartYposition = 0;
        private float previousChartAreaSize = 0;
        private float chartAreaSize = 0;
        private float previousScrollbarValue = 0;
        private bool scrollbarSet = false;
        #endregion

        private void Awake()
        {
            chartHolderRect = chartHolder.GetComponent<RectTransform>();

            initialChartYposition = chartHolderRect.localPosition.y;

            instantiatedCharts = new List<GameObject>();
        }

        private void OnEnable()
        {
            patientData = SortMeasurements(mainScript.database.GetAllMeasurements(mainScript.currentPatient));

            if(patientData.Count <= 0)
            {
                mainScript.menuSwitching.OpenInitialMenu();
                return;
            }

            SetDataLimiters(patientData);

            relevantData = GetRelevantData(patientData, poseRadioHandler.selectedPose, firstDate.dateValue, lastDate.dateValue,
                minimumDuration.durationValue, maximumDuration.durationValue);
            UpdateCharts();

            if (backgroundBlocker.hasData)
                backgroundBlocker.ReEnable();
        }

        private void Update()
        {
            if (poseRadioHandler.valueChanged)
            {
                poseRadioHandler.valueChanged = false;
                relevantData = GetRelevantData(patientData, poseRadioHandler.selectedPose, firstDate.dateValue, lastDate.dateValue,
                    minimumDuration.durationValue, maximumDuration.durationValue);

                UpdateCharts();
            }

            if (DataLimiterChanged())
            {
                relevantData = GetRelevantData(patientData, poseRadioHandler.selectedPose, firstDate.dateValue, lastDate.dateValue,
                    minimumDuration.durationValue, maximumDuration.durationValue);
                UpdateCharts();
            }
            else if (HasAnyToggleChanged())
                UpdateCharts();

            if (scrollbarSet && scrollbarScript.valuePositon != previousScrollbarValue)
                UpdatePosition(scrollbarScript.valuePositon);
        }

        private void SetDataLimiters(List<StabilometryMeasurement> data)
        {
            //Debug.LogError("Move these things to a static class");
            List<MyDateTime> dateList = new List<MyDateTime>();
            List<int> durationList = new List<int>();

            foreach (StabilometryMeasurement element in data)
            {
                if (!ListHasDate(dateList, element.dateTime))
                    dateList.Add(element.dateTime);

                durationList.AddRange(GetDurations(durationList, element));
            }

            bool isLover = true;

            // Date List should be already sorted.
            firstDate.SetDates(dateList, isLover);
            lastDate.SetDates(dateList, !isLover);

            durationList = OrderList(durationList);

            minimumDuration.SetDurations(durationList, isLover);
            maximumDuration.SetDurations(durationList, !isLover);

        }

        private bool DataLimiterChanged()
        {
            bool result = minimumDuration.valueChanged || maximumDuration.valueChanged || firstDate.valueChanged || lastDate.valueChanged;

            minimumDuration.valueChanged = false;
            maximumDuration.valueChanged = false;
            firstDate.valueChanged = false;
            lastDate.valueChanged = false;

            return result;
        }

        private void UpdatePosition(float newValue)
        {
            previousScrollbarValue = newValue;

            chartHolderRect.localPosition =
                new Vector3(
                    chartHolderRect.localPosition.x,
                    ConvertToPosition(newValue, chartAreaSize, ((RectTransform)chartMask.transform).rect.size.y, initialChartYposition),
                    chartHolderRect.localPosition.z);
        }

        private float ConvertToPosition(float newValue, float chartAreaSize, float maskSize, float initialAreaPosition)
        {
            // 0 mmeans at the top, 1 means at the bottom.

            float maxChartPosition = initialAreaPosition + chartAreaSize - maskSize * 0.8f;

            float result = newValue * (maxChartPosition - initialChartYposition) + initialChartYposition;
            return result;
        }

        /// <summary>
        /// Checks if any toggler has been changed.
        /// </summary>
        /// <returns></returns>
        private bool HasAnyToggleChanged()
        {
            bool result = false;

            foreach (AccordionToggler toggler in parameterTogglers)
            {
                if (toggler.ToggleChanged)
                {
                    toggler.ToggleChanged = false;
                    result = true;
                }
            }

            foreach (AccordionToggler toggler in taskTogglers)
            {
                if (toggler.ToggleChanged)
                {
                    toggler.ToggleChanged = false;
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Checks to see which tasks have been chosen.
        /// </summary>
        /// <param name="taskTogglers"></param>
        /// <returns></returns>
        private static List<Task> GetChosenTasks(AccordionToggler[] taskTogglers)
        {
            List<Task> result = new List<Task>();

            for (int i = 0; i < taskTogglers.Length; i++)
                if (taskTogglers[i].GetToggle().isOn)
                    result.Add((Task)i);

            return result;
        }

        /// <summary>
        /// Spawns charts and updates scrollbar.
        /// </summary>
        private void UpdateCharts()
        {
            SpawnCharts(GetChosenParameters(this.parameterTogglers), GetChosenTasks(this.taskTogglers));

            // Update Scrollbar size.
            float maskSize = ((RectTransform)chartMask.transform).rect.size.y;
            chartAreaSize = GetCurrentChartAreaSize(instantiatedCharts, maskSize);

            scrollbarScript.SetSize(chartAreaSize, maskSize);

            scrollbarSet = true;

            if (previousChartAreaSize != chartAreaSize)
            {
                previousChartAreaSize = chartAreaSize;
                scrollbarScript.valuePositon = 0;
            }
        }

        /// <summary>
        /// Destroys all charts and spawns new ones.
        /// </summary>
        /// <param name="allParameters"></param>
        /// <param name="allTasks"></param>
        private void SpawnCharts(List<Parameter> allParameters, List<Task> selectedTasks)
        {
            foreach (GameObject instance in instantiatedCharts)
                Destroy(instance);

            instantiatedCharts = new List<GameObject>();

            for (int i = 0; i < allParameters.Count; i++)
            {
                GameObject instance = Instantiate(lineChartPrefab, chartHolder.transform);
                StandardLineChartScript chartScript = instance.GetComponent<StandardLineChartScript>();

                bool smallChart = true;

                chartScript.SetSize(smallChart);
                chartScript.SetPosition(i, chartHolder.GetComponent<RectTransform>().rect.size);
                chartScript.SetChartData(GetCurrentChartData(allParameters[i]), allParameters[i], selectedTasks);

                chartScript.SetParent(i, this, backgroundBlocker);
                instantiatedCharts.Add(instance);
            }
        }

        private List<ChartData> GetCurrentChartData(Parameter currentParameter)
        {
            List<ChartData> result = new List<ChartData>();

            foreach (StabilometryMeasurement measurement in relevantData)
                result.Add(measurement.GetData(currentParameter));

            return result;
        }   

        public override void OpenAnalysisMenu(int index)
        {

            backgroundBlocker.Disable();
            for (int i = 0; i < patientData.Count; i++)
            {
                if (patientData[i].ID == relevantData[index].ID)
                {
                    mainScript.menuSwitching.OpenMenu(measurementMenu);
                    mainScript.stabilometryMeasurementScript.SetData(patientData, i);
                    break;
                }
            }
        }

        public void BackButtonClick()
        {
            mainScript.menuSwitching.OpenPreviousMenu();
        }
    }
}