﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteParams : MonoBehaviour
{
    // This is set to -1 when instantiated, and assigned when saving notes.
    [SerializeField] public int mIndex = -1;
    [SerializeField] public bool mActiveButtons = false;
}
