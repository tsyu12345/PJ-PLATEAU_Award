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
        public bool waitingPlayers = true;
        public float countdownTime = 5.0f;
        public delegate void GameEvent();
        public static event GameEvent OnGameStart;
        public TextMeshProUGUI CountDownUI;
        public string waitingText = "Waiting for other players";

        private float dotTimer = 0.0f;
        private int dotCount = 0;


        void Start() {
            PUN2Manager.OnPlayerEntered += (player) => {
                Debug.Log("Player Entered");
                if (PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length == 2) {
                    countdownStarted = true;
                    waitingPlayers = false;
                } else {
                    waitingPlayers = true;
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
            if(waitingPlayers) {
                UpdateWaitingText();
            }
        }

        public void CountDown() {
            countdownTime -= Time.deltaTime;
            CountDownUI.text = ((int)countdownTime).ToString();

            if (countdownTime < 0) {
                CountDownUI.text = "GO!";
                // カウントダウンが0になったらGO!
                countdownStarted = false;
                StartCoroutine(HideCountdownUI());
                gameStarted = true;
                OnGameStart?.Invoke();
            }
        }

        public void resetCountDownTimer() {
            countdownTime = 5.0f;
            countdownStarted = false;
        }

        private void UpdateWaitingText() {
            dotTimer += Time.deltaTime;

            if (dotTimer >= 1.0f) {
                dotTimer = 0f;
                dotCount++;
                if (dotCount > 3) {
                    dotCount = 0;
                }

                CountDownUI.text = waitingText + " " + new string('.', dotCount);
            }
        }

        private IEnumerator HideCountdownUI() {
            yield return new WaitForSeconds(1); // 1秒待機
            CountDownUI.text = ""; // UIを非表示にする
            CountDownUI.gameObject.SetActive(false);
        }

    }


}