using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DeviceManager;
using Newtonsoft.Json;


public class PlayerScript : MonoBehaviour { 
    public GameObject destination; // 目的地
    [Header("Player Situation")]
    public int CurrentSpeed;
    public bool walking = false;
    public bool running = false;
    [Header("Settings")]
    public float SplitModeThreshold = 1.5f;
    private float Strength;
    private DeviceInputManager deviceInputManager;
    private NavMeshAgent agent;
    private Animator _animator;

    /// <summary>
    /// 初期化処理
    /// </summary>
    void Start() {
        deviceInputManager = GetComponent<DeviceInputManager>();
        agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        agent.SetDestination(destination.transform.position);
        deviceInputManager.OnCleanedDataReceived += OnDeviceInput;
    }

    void Update() {
        
        if (agent.remainingDistance <= agent.stoppingDistance) {
            onGoal();
        } else {
            ChangeAnimation();
        }
    }


    private void onGoal() {
        walking = false;
        running = false;
    }

    private void ChangeAnimation() {
        if(running) {
            _animator.SetBool("isRun", true);
        } else {
            _animator.SetBool("isRun", false);
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

