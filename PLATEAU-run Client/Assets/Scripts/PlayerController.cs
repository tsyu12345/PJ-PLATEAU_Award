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
    public GameObject destination; // 目的地
    [Header("Player Situation")]
    public float CurrentSpeed;
    public bool walking = false;
    public bool running = false;
    [Header("Settings")]
    public string NickName = "TEST Unit1"; // プレイヤー名
    public float SplitModeThreshold = 1.5f;
    private float Strength;
    private DeviceInputManager deviceInputManager;
    private GameController gameManager;
    private NavMeshAgent agent;
    private Animator _animator;
    private Queue<Action> _mainThreadActions;
    private bool loaded = false;


    void Update() {
        //agent.SetDestination(destination.transform.position);
        if(loaded == false) { return; }
        if(gameManager.gameStarted == false) { return; }

        agent.SetDestination(destination.transform.position);
        if (agent.remainingDistance <= agent.stoppingDistance) {
            Wait();
        } else {
            ChangeAnimation();
        }
        Debug.Log("Main Thread Action Count" + _mainThreadActions.Count);
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
            return;
        }
        deviceInputManager = GetComponent<DeviceInputManager>();
        //FIXME:NullReferenceException
        gameManager = GameObject.Find("GameManager").GetComponent<GameController>();
        if(gameManager == null) {
            Debug.LogError("GameController is not found");
        }
        _mainThreadActions =  new Queue<Action>();
        agent = GetComponent<NavMeshAgent>();
        Debug.Log("Agent" + agent);
        _animator = GetComponent<Animator>();

        PhotonNetwork.NickName = NickName;

        deviceInputManager.AddEventListener(OnDeviceInput);
        //NOTE:PUNでスポーンすると目的地を正しく取得できないため、ここで目的地を設定する
        destination = GameObject.FindWithTag("Finish");
        agent.SetDestination(destination.transform.position);

        //初期は待機状態
        Wait();
        //子要素のカメラオブジェクトにAudioListenerを追加する
        var camera = GetComponentInChildren<Camera>();
        camera.gameObject.AddComponent<AudioListener>();

        loaded = true;
    }

    public override void OnDisable() {
        //deviceInputManager.DisConnect();
        deviceInputManager.RemoveEventListener(OnDeviceInput);
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
        CurrentSpeed = 3.0f;
    }

    private void Splint() {
        walking = false;
        running = true;
        agent.isStopped = false;
        CurrentSpeed = 5.0f;
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
        InputData data = JsonConvert.DeserializeObject<InputData>(json);
        Strength = data.strength;
        if(Strength >= SplitModeThreshold) {
            Splint();
        } else if(Strength == 0.0f) {
            Wait();
        } else {
            Walk();
        }
    
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

