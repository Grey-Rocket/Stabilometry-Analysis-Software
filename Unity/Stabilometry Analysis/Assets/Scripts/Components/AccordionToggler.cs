﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AccordionToggler : MonoBehaviour
{
    #region Variables
    public bool ToggleChanged { get; set; } = false;
    private Toggle toggle { get; set; } = null;
    public StabilometryAnalysisParameterMenuScript  AnalysisMenuScript {get; set;} = null;
    #endregion

    private void Awake()
    {
        toggle = transform.GetChild(0).GetComponent<Toggle>();
    }

    public Toggle GetToggle()
    {
        if (toggle == null)
            toggle = transform.GetChild(0).GetComponent<Toggle>();
        return toggle;
    }

    public void ChangeToggle()
    {
        toggle.isOn = !toggle.isOn;
        ToggleChanged = true;
    }

}
