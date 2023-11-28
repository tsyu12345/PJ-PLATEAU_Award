using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using PUN2;
using DeviceManager;

namespace GameManager {


    public class GameController : MonoBehaviour {
        public bool countdownStarted = false;
        public bool gameStarted = false;
        public bool gameFinished = false;
        public bool waitingPlayers = false;
        public float countdownTime = 5.0f;
        public delegate void GameEvent();
        public static event GameEvent OnGameStart;
        public TextMeshProUGUI CountDownUI;


        void Start() {
            PUN2Manager.OnPlayerEntered += (player) => {
                Debug.Log("Player Entered");
                if (PhotonNetwork.IsMasterClient) {
                    if (PhotonNetwork.PlayerList.Length == 2) {
                        waitingPlayers = false;
                        countdownStarted = true;
                    }
                }
            };

            PUN2Manager.OnPlayerLeft += (player) => {
                Debug.Log("Player Left");
            };
        }

        void Update() {
            if(countdownStarted) {
                CountDown();
            }
        }

        public void CountDown() {
            countdownTime -= Time.deltaTime;
            CountDownUI.text = ((int)countdownTime).ToString();

            if (countdownTime < 0) {
                CountDownUI.text = "GO!";
                Debug.Log("Countdown: GO!");
                // カウントダウンが0になったらGO!
                countdownStarted = false;
                // カウントダウンUIを非表示にする
                CountDownUI.text = "";
                gameStarted = true;
                OnGameStart?.Invoke();
            }
        }

        public void resetCountDownTimer() {
            countdownTime = 5.0f;
            countdownStarted = false;
        }

    }


}