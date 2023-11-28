using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using DeviceManager;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;

//TODO:適切にクラスを分割する
//現在はスピード優先で実装しているため、ゲームロジックの全てをこのクラスに書いている
public class PlayerController : MonoBehaviourPunCallbacks { 
    public GameObject destination; // 目的地
    [Header("Player Situation")]
    public float CurrentSpeed;
    public bool walking = false;
    public bool running = false;
    [Header("Settings")]
    public float countdownTime = 5.0f;
    public string NickName = "TEST Unit1"; // プレイヤー名
    public float SplitModeThreshold = 1.5f;
    private float Strength;
    private DeviceInputManager deviceInputManager;
    private NavMeshAgent agent;
    private Animator _animator;
    private bool countdownStarted = false;
    private bool gameStarted = false;
    [SerializeField]
    private TextMeshProUGUI CountDownUI;
    private Queue<Action> _mainThreadActions;

    void Update() {
        //agent.SetDestination(destination.transform.position);
        
        if(agent == null || _animator == null) {
            return;
        }
        var others = PhotonNetwork.PlayerListOthers;
        if (others.Length == 0 && !countdownStarted) {
            Wait();
            return;
        }

        //
        if (!countdownStarted && gameStarted == false) {
            countdownStarted = true;
            countdownTime = 5.0f; // カウントダウンをリセット
            return;
        } else if (countdownStarted && countdownTime > 0) {
            countdownTime -= Time.deltaTime;
            CountDownUI.text = ((int)countdownTime).ToString();
            Debug.Log("Countdown: " + countdownTime);
            return; // カウントダウンが0になるまで待機
        } else if (countdownTime < 0) {
            CountDownUI.text = "GO!";
            Debug.Log("Countdown: GO!");
            // カウントダウンが0になったらGO!
            countdownStarted = false;
            gameStarted = true;
            // カウントダウンUIを非表示にする
            CountDownUI.text = "";
            return;
        }
        
        
        agent.SetDestination(destination.transform.position);
        if (agent.remainingDistance <= agent.stoppingDistance) {
            onGoal();
        } else {
            ChangeAnimation();
        }
        while (_mainThreadActions.Count > 0) {
            var action = _mainThreadActions.Dequeue();
            if(action != null) { 
                action.Invoke();
            }
        }
        Debug.Log("Agent Speed: " + agent.speed);
    }

    /// <summary>
    /// 自身の初期化処理
    /// PhotonNetwork.Instantiateの生成処理後に必要な初期化処理を行う  
    /// </summary>
    public override void OnEnable() {
        base.OnEnable();
        if(!photonView.IsMine) {
            return;
        }
        deviceInputManager = GetComponent<DeviceInputManager>();
        _mainThreadActions =  new Queue<Action>();
        agent = GetComponent<NavMeshAgent>();
        Debug.Log("Agent" + agent);
        _animator = GetComponent<Animator>();

        PhotonNetwork.NickName = NickName;

        deviceInputManager.AddEventListener(OnDeviceInput);
        //NOTE:PUNでスポーンすると目的地を正しく取得できないため、ここで目的地を設定する
        destination = GameObject.FindWithTag("Finish");
        agent.SetDestination(destination.transform.position);

        CountDownUI = GameObject.Find("CountDown").GetComponent<TextMeshProUGUI>();
        gameStarted = true;
    }


    private void Wait() {
        walking = false;
        running = false;
        agent.isStopped = true;
        CurrentSpeed = 0.0f;
    }

    private void onGoal() {
        walking = false;
        running = false;
        agent.isStopped = true;
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
        InputData data = JsonConvert.DeserializeObject<InputData>(json);
        Strength = data.strength;
        if(gameStarted == false) { return; }
        if(Strength >= SplitModeThreshold) {
            CurrentSpeed = 5.0f;
            running = true;
            walking = false;
        } else if(Strength == 0.0f) {
            CurrentSpeed = 0.0f;
            running = false;
            walking = false;
        } else {
            CurrentSpeed = 3.0f;
            running = false;
            walking = true;
        }
        //Debug.Log("Current Speed: " + CurrentSpeed);
        
        //NOTE: メインスレッドで実行しないと速度が変わらない
        _mainThreadActions.Enqueue(() => {
            agent.speed = CurrentSpeed;
        });
        
    }


}

[Serializable]
public class InputData {
    public string user_id;
    public float strength;
    public string client_type;
}

