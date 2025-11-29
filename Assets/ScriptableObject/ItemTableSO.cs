using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum GachaGrade
{
    R,
    SR,
    SSR
}

[CreateAssetMenu(menuName = "Spine Wheel/Table")]
public class ItemTableSO : ScriptableObject
{
    public ItemDataSO Item;
    public int Probability; // 가중치
    public GachaGrade Grade; // 등급 (R, SR, SSR)
    public bool IsPickup;
}
