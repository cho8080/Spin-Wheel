using UnityEngine.AddressableAssets;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AddressableManager : MonoBehaviour
{
    private static AddressableManager instance =null;

    [SerializeField] private AssetReferenceGameObject _endGamePopUp;

    private List<GameObject> gameObjects = new List<GameObject>();
    private void Awake() {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    public static AddressableManager Instance
    {
        get
        {
            if(instance == null)
            {
                return null;
            }
            return instance;
        }
    }
    private void Start()
    {
            StartCoroutine(InitAddressable());
    }
    IEnumerator InitAddressable()
    {
        var init = Addressables.InitializeAsync();
        yield return init;
    }

    /// <summary>
    /// 게임 종료 팝업 활성화
    /// </summary>
    public void ShowEndGamePopUp(ItemDataSO itemDataSO)
    {
        _endGamePopUp.InstantiateAsync().Completed += (obj) =>
        {
           GameObject popup = obj.Result;

           popup.transform.SetParent(FindObjectOfType<Canvas>().transform, false);

           gameObjects.Add(popup);

            EndGamePopUp endGamePopUp = popup.GetComponent<EndGamePopUp>();
       
            if(endGamePopUp == null || itemDataSO == null)
                return;
                
            endGamePopUp.SetDate(itemDataSO);
        };
    }

        /// <summary>
    /// 게임 종료 팝업 비활성화
    /// </summary>
    public void HideEndGamePopUp()
    {
       if(gameObjects.Count == 0)
            return;

        var index = gameObjects.Count -1;
        Addressables.ReleaseInstance(gameObjects[index]);
        gameObjects.RemoveAt(index);
    }
}
