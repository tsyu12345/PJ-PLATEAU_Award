using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;

using GameManager;
using DeviceManager;

//TODO:適切にクラスを分割する
//現在はスピード優先で実装しているため、ゲームロジックの全てをこのクラスに書いている
public class PlayerController : MonoBehaviourPunCallbacks {
    [Header("Player Object")]
    [SerializeField]
    public GameObject destination; // 目的地
    public GameObject playerCamera; // プレイヤーのカメラ
    [Header("Player Situation")]
    public float CurrentSpeed;
    public bool walking = false;
    public bool running = false;
    [Header("Settings")]
    public string NickName = "TEST Unit1"; // プレイヤー名
    public delegate void GoalEvent(); //TODO:ゴールした者のデータオブジェクトを使す
    private float Strength;
    private DeviceInputManager deviceInputManager;
    private GameController gameManager;
    private NavMeshAgent agent;
    private Animator _animator;
    private Queue<Action> _mainThreadActions;
    private bool loaded = false;
    public bool isGoal = false;
    private static GoalEvent onGoal;
    public float endTime;

    private float totalDistance;

    public delegate void OnLoadEvent();
    public static OnLoadEvent OnLoad;


    
    void Update() {
        //agent.SetDestination(destination.transform.position);
        if(loaded == false) { 
            //Debug.LogWarning("loaded is false");
            return; 
        }

        agent.SetDestination(destination.transform.position);
        UpdateDistanceMeter();
        if(gameManager.gameStarted == false) { 
            //Debug.LogWarning("gameManager.gameStarted is false");
            return; 
        }
        if(agent.remainingDistance < 150) {
            gameManager.nearGoal = true;
        }
        if (agent.remainingDistance <= agent.stoppingDistance && isGoal==false) {//ゴール時の処理
            isGoal = true;
            Wait();
            deviceInputManager.RemoveEventListener(OnDeviceInput);

            endTime = Time.time;
            var GoalUI = gameManager.GoalUI;
            GoalUI.SetActive(true);
            var resultTime = endTime - gameManager.startTime;
            var GoalTimeText = gameManager.GoalText;
            var resultText = ConvertSecondsToMinutes((int)resultTime);
            GoalTimeText.text = resultText;

            gameManager.gameFinished = true;
            onGoal?.Invoke();
        } else {
            ChangeAnimation();
        }
        while (_mainThreadActions.Count > 0) {
            var action = _mainThreadActions.Dequeue();
            if(action != null) { 
                action.Invoke();
            }
        }
    }

    /// <summary>
    /// 自身の初期化処理
    /// PhotonNetwork.Instantiateの生成処理後に必要な初期化処理を行う  
    /// </summary>
    public override void OnEnable() {
        base.OnEnable();
        if(!photonView.IsMine) {
            //カメラを無効化する
            Destroy(playerCamera);
            Debug.Log("Camera Disabled");
            return;
        }
        deviceInputManager = GetComponent<DeviceInputManager>();
        //FIXME:NullReferenceException
        gameManager = GameObject.Find("GameObserver").GetComponent<GameController>();
        //Debug.LogWarning("gameManeer.gameStarted" + gameManager.gameStarted);
        if(gameManager == null) {
            Debug.LogError("GameController is not found");
        }
        _mainThreadActions =  new Queue<Action>();
        agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        PhotonNetwork.NickName = NickName;
        //自身の名前表示は不要
        var nameObj = GameObject.Find("PlayerName");
        Destroy(nameObj);

        deviceInputManager.AddEventListener(OnDeviceInput);
        //NOTE:PUNでスポーンすると目的地を正しく取得できないため、ここで目的地を設定する
        destination = GameObject.FindWithTag("Finish");
        agent.SetDestination(destination.transform.position);

        //初期は待機状態
        Wait();

        Camera myCamera = playerCamera.GetComponent<Camera>();
        myCamera.enabled = true;
        Debug.Log("Camera Enabled");
        //子要素のカメラオブジェクトにAudioListenerを追加する
        playerCamera.AddComponent<AudioListener>();

        //初期の経路距離を取得する
        totalDistance = CalcRemainDistance();

        loaded = true;
        OnLoad?.Invoke();
    }

    public override void OnDisable() {
        //deviceInputManager.DisConnect();
        deviceInputManager.RemoveEventListener(OnDeviceInput);
    }


    public override void OnPlayerEnteredRoom(Player newPlayer) {
        ChangeCameraActivate();
    }


    private void Wait() {
        walking = false;
        running = false;
        agent.isStopped = true;
        CurrentSpeed = 0.0f;
    }

    private void Walk() {
        walking = true;
        running = false;
        agent.isStopped = false;
    }

    private void Splint() {
        walking = false;
        running = true;
        agent.isStopped = false;
    }

    public static string ConvertSecondsToMinutes(int totalSeconds) {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return string.Format("{0:D2}:{1:D2}", minutes, seconds);
    }

    private void ChangeAnimation() {
        if(running) {
            _animator.SetBool("running", true);
        } else {
            _animator.SetBool("running", false);
        }

        if(walking) {
            _animator.SetBool("walking", true);
        } else {
            _animator.SetBool("walking", false);
        }
    }

    /// <summary>
    /// デバイス入力を受け取った時のコールバック
    /// </summary>
    /// <param name="json"></param>
    /// 
    private void OnDeviceInput(string json) {
        if(gameManager.gameStarted == false) { return; }
        if(isGoal == true) { return; }
        InputData data = JsonConvert.DeserializeObject<InputData>(json);
        Strength = data.strength;
        CurrentSpeed = CalcSpeed(Strength);
        //キューの要素が多くなりすぎないように、規定サイズでクリアする
        if(_mainThreadActions.Count > 10) {
            _mainThreadActions.Clear();
        }
    
        //NOTE: メインスレッドで実行しないと速度が変わらない
        _mainThreadActions.Enqueue(() => {
            if(CurrentSpeed > 0 && CurrentSpeed < 3) {
                Walk();
            } else if(CurrentSpeed >= 5) {
                Splint();
            } else {
                Wait();
            }
            agent.speed = CurrentSpeed;
            gameManager.PowerMeter.text = string.Format("{0:F2} p", Strength);
            gameManager.SpeedMeter.text = string.Format("{0:F1}", CurrentSpeed);
        });   
    }


    /// <summary>
    /// y = 2xの関数でスピードを計算する
    /// 
    /// </summary>
    /// <param name="strength"></param>
    /// <returns></returns>
    private static float CalcSpeed(float strength) {
        return 2.1f * strength;
    }


    private float CalcRemainDistance() {
        NavMeshPath path = agent.path; //経路パス（曲がり角座標のVector3配列）を取得
        float dist = 0f;
        Vector3 corner = transform.position; //自分の現在位置
        //曲がり角間の距離を累積していく
        for (int i = 0; i < path.corners.Length; i++){
            Vector3 corner2 = path.corners[i];
            dist += Vector3.Distance(corner, corner2);
            corner = corner2;
        }
        return dist;
    }


    private void UpdateDistanceMeter() {
        var remainDistance = CalcRemainDistance();
        //進んだ割合を計算する
        var ratio = 1 - (remainDistance / totalDistance);
        gameManager.DistanceMeter.value = ratio;
        //m to kmに変換する
        remainDistance = remainDistance / 1000;
        gameManager.DistanceMeterText.text = string.Format("{0:F1}", remainDistance);
    }


    private void ChangeCameraActivate() {
        if (photonView.IsMine) {
            // このオブジェクトがローカルプレイヤーの場合のみカメラを有効化
            
        } else {
            // リモートプレイヤーの場合はカメラを無効化（オブジェクトの削除）
            
        }
    }


}

[Serializable]
public class InputData {
    public string user_id;
    public float strength;
    public string client_type;
}

