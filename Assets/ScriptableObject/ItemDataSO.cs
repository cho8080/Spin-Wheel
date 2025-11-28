using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Spine Wheel/ItemData")]
public class ItemDataSO : ScriptableObject
{
  public int ItemID;
  public string ItemName;
  public Sprite Icon;
}
