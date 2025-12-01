using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using Unity.VisualScripting;


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
     [SerializeField] private ParticleSystem particle; 

     [Header("Item")]

    [SerializeField] private List<ItemTableSO> _itemList;
    [SerializeField] private int _itemCount = 8; // 룰렛의 총 아이템 개수 

    [Header("ItemGrade")]
    [SerializeField] private int _pityCount = 0;   // 현재 천장 누적
    [SerializeField] private int _pityMax = 50;    // 몇 번 실패하면 SSR 확정

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

            // 먼저 아이템을 뽑는다
            ItemDataSO itemDataSO = DrawItem(_itemList);
            _itemDataSO = itemDataSO;

            if(_itemDataSO == null)
            {
                ChangeState(SpineWheelState.Idle);
                return;
            }

            // 뽑은 아이템의 인덱스를 찾는다
            int targetIndex = GetItemIndex(_itemDataSO);

            // 목표 각도 계산 (각 아이템당 360 / itemCount도씩 차지)
            float anglePerItem = 360f / _itemCount;

            // 핸들이 위쪽에 고정되어 있으므로, 해당 아이템이 위로 오도록 회전
            // 반시계방향 회전이므로 음수, targetIndex만큼 회전하면 해당 아이템이 위로 옴
            float targetAngle = targetIndex * anglePerItem;

            // 여러 바퀴 돌고 목표 각도에 도달하도록 계산
            int fullRotations = 12; // 12바퀴 회전
            float offset = 90f; // 초기 위치 오프셋 조정
            float finalAngle = -(360f * fullRotations + targetAngle + offset);

            // 일정 시간동안 회전 시킨다
             _wheel.DOLocalRotate(new Vector3(0, 0, finalAngle), 4f, RotateMode.FastBeyond360)
              .SetEase(Ease.OutQuart)
              .OnComplete(()=>
              {
                ItemTableSO itemTable = GetItemTableSO(_itemDataSO);

                if(_itemDataSO != null && itemTable != null && itemTable.Grade == GachaGrade.SSR)
                  {
                      CameraShake cameraShake = FindObjectOfType<CameraShake>();
                      if(cameraShake != null)
                      {
                          cameraShake.TriggerShake(0.1f, 0.1f);
                      }
                  }
                // 회전 완료 후에는 상태를 Idle로 전환한다.
                ChangeState(SpineWheelState.Idle);

                if(particle != null)
                {
                   // particle.transform.position = _wheel.position;
                    particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    particle.Clear();
                    particle.Play();
                }

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

/// <summary>
/// 아이템 추첨
/// </summary>
public ItemDataSO DrawItem(List<ItemTableSO> table)
{
    // 시행 횟수 증가
    _pityCount++;

    // 천장 시스템
    if(_pityCount >= _pityMax)
        {
            // SSR 아이템 랜덤 지급
            ItemDataSO ssrItem = GetRandomGradeItem(table, GachaGrade.SSR);
            _pityCount = 0;
            return ssrItem;
        }

    // 일반 확률 뽑기
    // 모든 아이템 확률의 총합을 저장할 변수
    int total = 0;

    // 각 아이템의 확률을 total에 누적
    foreach (var t in table)
        total += t.Probability;

    // 0 ~ 전체 확률 범위 내에서 랜덤 값 생성
    int rand = Random.Range(0, total);

     // 누적 확률을 저장할 변수
    int acc = 0;

    // 테이블을 처음부터 순회하면서
    foreach (var t in table)
    {
        // 현재 아이템까지의 누적 확률 계산
        acc += t.Probability;

        // 랜덤 값이 현재 누적 확률보다 작다면
        if (rand < acc)
            return t.Item; // 해당 구간에 속하는 아이템을 반환
    }

    return null;
}

/// <summary>
/// 뽑은 아이템이 룰렛의 몇 번째 인덱스인지 찾기
/// </summary>
private int GetItemIndex(ItemDataSO item)
{
    for (int i = 0; i < _itemList.Count; i++)
    {
        if (_itemList[i].Item == item)
            return i;
    }

    // 못 찾으면 0번 인덱스 반환
    return 0;
}
    private ItemDataSO  GetRandomGradeItem(List<ItemTableSO> table, GachaGrade gachaGrade)
    {
        List<ItemTableSO> ssrItems = new List<ItemTableSO>();
        foreach (var item in table)
        {
            if(item.Grade == gachaGrade)
            {
                ssrItems.Add(item);
            }
        }

         if (ssrItems.Count == 0)
        {
            return null;
        }
        int rand = Random.Range(0, ssrItems.Count);

        return ssrItems[rand].Item;
    }

    /// <summary>
    /// _itemList에서 특정 ItemDataSO와 일치하는 ItemTableSO를 반환
    /// </summary>
    private ItemTableSO GetItemTableSO(ItemDataSO itemDataSO)
    {
        for (int i = 0; i < _itemList.Count; i++)
        {
            if (_itemList[i].Item == itemDataSO)
                return _itemList[i];
        }

        // 못 찾으면 null 반환
        return null;
    }
}
