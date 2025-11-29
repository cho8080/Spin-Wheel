using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;


// SpineWheel 상태를 정의
public enum SpineWheelState
{
    Idle,
    Spinning
}
public class SpinWheel : MonoBehaviour
{
     [Header("UI")]
    [SerializeField] private Transform  _wheel;
    [SerializeField] private Button _spinBtn;
    
     [Header("Item")]

    [SerializeField] private List<ItemTableSO> _itemList;

    private SpineWheelState _currentState;
    private bool _canBtnClick;

    private ItemDataSO _itemDataSO;

    void Start()
    {
        _spinBtn.onClick.AddListener(SpineBtnClick);

        ChangeState(SpineWheelState.Idle);
    }

    /// <summary>
    /// Spin 버튼 클릭 이벤트
    /// </summary>
    private void SpineBtnClick()
    {
        // 클릭이 가능하다면
        if(_canBtnClick)
        { 
            // 상태를 Spinning으로 전환하고
            ChangeState(SpineWheelState.Spinning);

            // 일정 시간동안 회전 시킨다
             _wheel.DOLocalRotate(new Vector3(0,0,-360*12), 4f, RotateMode.FastBeyond360)
              .SetEase(Ease.OutQuart)
              .OnComplete(()=>
              {
                // 회전 완료 후에는 상태를 Idle로 전환한다.
                ChangeState(SpineWheelState.Idle);

                ItemDataSO itemDataSO = DrawItem(_itemList);
                _itemDataSO = itemDataSO;

                if(_itemDataSO == null)
                    return;
                
                AddressableManager.Instance.ShowEndGamePopUp(_itemDataSO);

              });    
        }
    }

    /// <summary>
    /// 상태 전환 함수
    /// </summary>
    /// <param name="newState">상태</param>
    private void ChangeState(SpineWheelState newState)
    {
        _currentState = newState;

        switch (_currentState)
        {
            case SpineWheelState.Idle:
                _canBtnClick = true;
            break;
               case SpineWheelState.Spinning:
                _canBtnClick = false;
            break;           
        }
    }

public ItemDataSO DrawItem(List<ItemTableSO> table)
{
    int total = 0;
    foreach (var t in table)
        total += t.Probability;

    int rand = Random.Range(0, total);

    int acc = 0;
    foreach (var t in table)
    {
        acc += t.Probability;
        if (rand < acc)
            return t.Item;
    }

    return null;
}
}
