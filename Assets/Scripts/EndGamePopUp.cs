using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndGamePopUp : MonoBehaviour
{
    [SerializeField] private List<Button> _closeButtons =new List<Button>();
    [SerializeField] private Image _itemImage;

    private ItemDataSO _itemDataSO;

    private void Start()
    {
        foreach (var button in _closeButtons)
        {
            button.onClick.AddListener(ClosePopUp);
        }
    }
    public void SetDate(ItemDataSO itemDataSO)
    {
        _itemDataSO = itemDataSO;
        _itemImage.sprite = _itemDataSO.Icon;
    }
    private void ClosePopUp()
    {
        AddressableManager.Instance.HideEndGamePopUp();
    }
}
