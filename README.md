Unity Item Gacha System (2025.12.29~2025.12.02)

![UI 연출(조하늘)](https://github.com/user-attachments/assets/42d8251a-e65c-4919-b0f8-252c592ace85)

[ 동영상 보기 ]
https://www.youtube.com/watch?v=ta-nE_Mr8h4

[ 작업 일지 ]
https://cho8080.tistory.com/108

Unity 기반 모바일 게임을 대상으로 제작한 확률형 아이템 시스템(UI 중심) 포트폴리오 프로젝트입니다.
실제 라이브 서비스 환경을 고려하여 데이터 기반 구조, 확률 알고리즘, 연출 시스템, UI 아키텍처를 중심으로 구현했습니다.

## 기술 스택 (Tech Stack)

###  Unity UI 구성
- Unity UGUI
- 모바일 세로형(1080x1920) 기준 UI 구성

###  데이터 구조
- ScriptableObject 확률 테이블
- 아이템 데이터 및 아이템 확률을 ScriptableObject로 분리
- 코드 수정 없이 확률 및 픽업 가차 조정 가능

###  연출 시스템
- DOTween 룰렛 연출  
  - Spin Wheel 회전, 감속 등 UI 연출 처리  
  - Ease Curve 기반 자연스러운 연출 구현
    
- Shader Graph 연출
  - 버튼 UI에 Rainbow Shader Graph 적용
  - 강조 효과로 시각적 피드백 제공

- Timeline UI 연출
  - 게임 시작 시 Fade-in 효과 구현
  - CanvasGroup Alpha 기반 자연스러운 UI 등장
  
- Addressables 연출 로딩  
  - PopUp 비동기 로딩  
  - 메모리 최적화 및 리소스 관리 구조 적용

###  UI 구조
- 상태 머신(State Machine) + 이벤트 구조  
  - `Idle → Spinning` 흐름 제어  
  - 중복 클릭 방지 및 UI 상태 충돌 방지


---


##  알고리즘

###  누적 확률(Prefix Sum) 랜덤 알고리즘
- 각 아이템의 확률을 누적합으로 변환 후 구간 기반 랜덤 추첨

###  천장(Pity) 시스템
- 일정 횟수 미획득 시 SSR 확정 처리

---

##  주요 기능

- 아이템 확률형 아이템 시스템
- 등급별(R / SR / SSR) 연출 분기


---

##  포트폴리오 제작 목적

- Unity UI 실무 구조 설계 역량 증명
- 데이터 기반 확률형 아이템 시스템 구현 능력 검증
- 모바일 라이브 서비스용 UI 흐름 및 연출 처리 경험 정리
- 상태 머신, 이벤트 구조 등 실전 최적화 설계 적용

