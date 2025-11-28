using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Spine Wheel/Table")]
public class SpineWheelTableSO : ScriptableObject
{
    public ItemDataSO Item;
    public int Probability; // 가중치
    public bool IsPickup;
}
