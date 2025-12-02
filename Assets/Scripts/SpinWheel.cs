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
    [SerializeField] private List<Transform> _slots;
    [SerializeField] private int _itemCount = 8; // 룰렛의 총 아이템 개수 

    [Header("ItemGrade")]
    [SerializeField] private int _pityCount = 0;   // 현재 천장 누적
    [SerializeField] private int _pityMax = 50;    // 몇 번 실패하면 SSR 확정
    [SerializeField] private int _spinCount = 0;   // 총 스핀 횟수

    private SpineWheelState _currentState;
    private bool _canBtnClick;

    private ItemDataSO _itemDataSO;
    private int _drawnItemIndex; // 뽑은 아이템의 인덱스

    void Start()
    {
        _spinBtn.onClick.AddListener(SpineBtnClick);

        ChangeState(SpineWheelState.Idle);
    }

    /// <summary>
    /// 룰렛을 초기 위치로 리셋
    /// </summary>
    public void ResetWheel()
    {
        _wheel.localRotation = Quaternion.Euler(0, 0, 0);
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

            // 먼저 아이템을 뽑는다 (인덱스도 함께 저장)
            int drawnIndex = DrawItemWithIndex(_itemList);

            if(drawnIndex < 0 || drawnIndex >= _itemList.Count)
            {
                ChangeState(SpineWheelState.Idle);
                return;
            }

            _itemDataSO = _itemList[drawnIndex].Item;
            _drawnItemIndex = drawnIndex;

            if(_itemDataSO == null)
            {
                ChangeState(SpineWheelState.Idle);
                return;
            }

            // 뽑은 아이템의 인덱스 사용
            int targetIndex = _drawnItemIndex;

            float anglePerItem = 360f / _itemList.Count;

            // 핸들이 0도일 때 첫 번째 슬롯(0번 인덱스)이 핸들 위쪽에 오도록 오프셋
            float wheelOffset = 0; // 실제 핸들 기준에 맞게 조정

            // 시계방향 회전
            float targetAngle = (_drawnItemIndex * anglePerItem) + wheelOffset;

            int fullRotations = 12;

            float finalAngle = fullRotations * 360f + targetAngle; // 반시계방향
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

                if(particle != null && itemTable != null &&
                   (itemTable.Grade == GachaGrade.SSR || itemTable.Grade == GachaGrade.SR))
                {

                    particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
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
/// 아이템 추첨 (인덱스 반환)
/// </summary>
public int DrawItemWithIndex(List<ItemTableSO> table)
{
    // 스핀 횟수 증가
    _spinCount++;

    // 튜토리얼: 처음 3번은 R, SR, SSR 순서로 고정
    // if(_spinCount == 1)
    // {
    //     // 첫 번째: R등급
    //     int rIndex = GetRandomGradeItemIndex(table, GachaGrade.R);
    //     return rIndex;
    // }
    // else if(_spinCount == 2)
    // {
    //     // 두 번째: SR등급
    //     int srIndex = GetRandomGradeItemIndex(table, GachaGrade.SR);
    //     return srIndex;
    // }
    // else if(_spinCount == 3)
    // {
    //     // 세 번째: SSR등급
    //     int ssrIndex = GetRandomGradeItemIndex(table, GachaGrade.SSR);
    //     return ssrIndex;
    // }

    // 시행 횟수 증가
    _pityCount++;

    // 천장 시스템
    if(_pityCount >= _pityMax)
    {
        // SSR 아이템 랜덤 지급
        int ssrIndex = GetRandomGradeItemIndex(table, GachaGrade.SSR);
        _pityCount = 0;
        return ssrIndex;
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
    for (int i = 0; i < table.Count; i++)
    {
        // 현재 아이템까지의 누적 확률 계산
        acc += table[i].Probability;

        // 랜덤 값이 현재 누적 확률보다 작다면
        if (rand < acc)
            return i; // 해당 인덱스 반환
    }

    return 0;
}

/// <summary>
/// 뽑은 아이템이 룰렛의 몇 번째 인덱스인지 찾기
/// </summary>
private int GetItemIndex(ItemDataSO item)
{
    // ItemTableSO까지 정확히 매칭하는 인덱스를 찾음
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
    /// 특정 등급의 아이템을 랜덤으로 선택하고 인덱스 반환
    /// </summary>
    private int GetRandomGradeItemIndex(List<ItemTableSO> table, GachaGrade gachaGrade)
    {
        List<int> gradeIndices = new List<int>();

        // 해당 등급의 모든 인덱스를 찾음
        for (int i = 0; i < table.Count; i++)
        {
            if(table[i].Grade == gachaGrade)
            {
                gradeIndices.Add(i);
            }
        }

        // 매칭되는 인덱스가 없으면 0 반환
        if (gradeIndices.Count == 0)
        {
            return 0;
        }

        // 랜덤하게 하나 선택
        int rand = Random.Range(0, gradeIndices.Count);
        return gradeIndices[rand];
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
