﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSwitching : MonoBehaviour
{
    #region Variables

    [SerializeField]
    private GameObject initialMenu = null;

    private GameObject currentMenu = null;

    #endregion

    private void Awake()
    {
        currentMenu = initialMenu;
    }

    public void OpenAddPatientMenu(GameObject addPatientMenu)
    {
        OpenMenu(addPatientMenu);
    }

    public void OpenEditPatientMenu(GameObject editPatientMenu)
    {
        OpenMenu(editPatientMenu);
    }

    public void OpenMenu(GameObject newMenu)
    {
        currentMenu.SetActive(false);
        currentMenu = newMenu;
        newMenu.SetActive(true);
    }

    public void OpenInitialMenu()
    {
        currentMenu.SetActive(false);
        currentMenu = initialMenu;
        initialMenu.SetActive(true);
    }
}