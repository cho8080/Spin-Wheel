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
     [SerializeField] private ParticleSystem particle; 

     [Header("Item")]

    [SerializeField] private List<ItemTableSO> _itemList;
    [SerializeField] private int _itemCount = 8; // 룰렛의 총 아이템 개수 (Inspector에서 설정)

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
}
