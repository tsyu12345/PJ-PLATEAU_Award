using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DeviceManager;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;


public class PlayerController : MonoBehaviourPunCallbacks { 
    public GameObject destination; // 目的地
    [Header("Player Situation")]
    public int CurrentSpeed;
    public bool walking = false;
    public bool running = false;
    [Header("Settings")]
    public string NickName = "TEST Unit1"; // プレイヤー名
    public float SplitModeThreshold = 1.5f;
    public int displayNumber = 2;
    private float Strength;
    private DeviceInputManager deviceInputManager;
    private NavMeshAgent agent;
    private Animator _animator; 

    /// <summary>
    /// 初期化処理
    /// </summary>
    void Start() {
        
    }

    void Update() {
        //agent.SetDestination(destination.transform.position);
        if (agent.remainingDistance <= agent.stoppingDistance) {
            onGoal();
        } else {
            ChangeAnimation();
        }
    }


    public override void OnEnable() {
        base.OnEnable();
        // PhotonNetwork.Instantiateの生成処理後に必要な初期化処理を行う
        if(!photonView.IsMine) {
            return;
        }
        deviceInputManager = GetComponent<DeviceInputManager>();
        agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        PhotonNetwork.NickName = NickName;
        //カメラコンポーネントのディスプレイ割り当て
        Camera camera = GetComponentInChildren<Camera>();
        camera.targetDisplay = displayNumber;


        deviceInputManager.OnCleanedDataReceived += OnDeviceInput;
        //NOTE:PUNでスポーンすると目的地を正しく取得できないため、ここで目的地を設定する
        destination = GameObject.FindWithTag("Finish");
        agent.SetDestination(destination.transform.position);
    }



    private void onGoal() {
        walking = false;
        running = false;
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
        if(Strength >= SplitModeThreshold) {
            CurrentSpeed = 5;
            running = true;
            walking = false;
        } else if(Strength == 0.0f) {
            CurrentSpeed = 0;
            running = false;
            walking = false;
        } else {
            CurrentSpeed = 3;
            running = false;
            walking = true;
        }
        agent.speed = CurrentSpeed;
    }


}

[Serializable]
public class InputData {
    public string user_id;
    public float strength;
    public string client_type;
}

