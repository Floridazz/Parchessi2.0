﻿using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "DiceDescription", menuName = "ScriptableObjects/DiceDescription", order = 1)]
public class DiceDescription : ScriptableObject
{
    public int DiceID;
    
    public int DiceLowerRange;
    public int DiceUpperRange;

    
}
