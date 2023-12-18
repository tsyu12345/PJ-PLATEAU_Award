using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

using PUN2;
using DeviceManager;

namespace GameManager {


    public class GameController : MonoBehaviour {
        [Header("GameMode")]
        public bool isMultiplayerMode = false;
        public bool isOfflineMode = false;
        [Header("Game Situations")]
        public bool countdownStarted = false;
        public bool gameStarted = false;
        public bool nearGoal = false;
        public bool gameFinished = false;
        public bool waitingPlayers = false;
        public bool loadingRoom = false;
        
        [Header("Game Settings")]
        public float countdownTime = 5.0f;
        public delegate void GameEvent();
        public static event GameEvent OnGameStart;
        public TextMeshProUGUI CountDownUI;
        public string waitingText = "Waiting for other players";

        [Header("Game Objects")]
        public AudioClip startSE;
        public AudioClip mainBGM;
        public AudioClip WaitingBGM;
        public AudioClip NearGoalBGM;
        public AudioClip GoalBGM;
        public GameObject PlayerUI;
        public TextMeshProUGUI PowerMeter;
        public TextMeshProUGUI SpeedMeter;
        public Button BackButton;
        [SerializeField]
        private GameObject loadingUI;


        private float dotTimer = 0.0f;
        private int dotCount = 0;
        private int PlayerCount = 0;
        private AudioSource audioSource;
        public float startTime;

        public GameObject GoalUI;
        public TextMeshProUGUI GoalText;



        void Start() {
            GoalUI.SetActive(false);
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = WaitingBGM;
            audioSource.Play();
            audioSource.loop = true;
            GetLobbySelection();
            if(isMultiplayerMode && isOfflineMode) {
                Debug.LogError("[GameController]MultiplayerMode and OfflineMode cannot be true at the same time");
            }

            PUN2Manager.OnConnectedMaster += () => {
                loadingRoom = true;
                loadingUI.SetActive(true);
            };
            PUN2Manager.OnEnterRoom += (int PlayerCount) => {
                loadingRoom = false;
                loadingUI.SetActive(false);
            };
            if (isMultiplayerMode) {
                onlineModeStart();
            } else if (isOfflineMode) {
                offlineModeStart();
            }

            BackButton.onClick.AddListener(() => {
                SceneManager.LoadScene("LobbyScene");
            });
        }

        void Update() {
            
            if(countdownStarted) {
                CountDown();
            }
            if(waitingPlayers) {
                UpdateWaitingText();
            }
            if (nearGoal && audioSource.clip != NearGoalBGM) {
                ChangeBGM(NearGoalBGM, true);
            }
            if(gameFinished) {
                if(audioSource.clip != GoalBGM) {
                    ChangeBGM(GoalBGM, false);
                }
                OnFinishGame();
            }
            
        }

        public void CountDown() {
            countdownTime -= Time.deltaTime;
            CountDownUI.text = ((int)countdownTime).ToString();

            if (countdownTime < 0) {
                CountDownUI.text = "GO!";
                //効果音を鳴らす
                audioSource.PlayOneShot(startSE);
                // カウントダウンが0になったらGO!
                countdownStarted = false;
                StartCoroutine(HideCountdownUI());
                gameStarted = true;
                OnGameStart?.Invoke();
                //BGMを設定
                audioSource.clip = mainBGM;
                audioSource.Play();
                audioSource.loop = true;
                startTime = Time.time;
                
            }
        }

        public void resetCountDownTimer() {
            countdownTime = 5.0f;
            countdownStarted = false;
        }



        private void GetLobbySelection() {
            isOfflineMode = LobbyScene.isPractice;
            if(isOfflineMode) {
                isMultiplayerMode = false;
            } else {
                isMultiplayerMode = true;
            }
            if(LobbyScene.audioSource != null) {
                LobbyScene.audioSource.Stop();
            }
        }


        private void OnFinishGame() {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            //PlayerUI.SetActive(false);
        }


        /// <summary>
        /// オンラインモードでのゲーム初期化処理
        /// そのラッパー
        /// </summary>
        private void onlineModeStart() {
            PUN2Manager.OnEnterRoom += (int PlayerCount) => {
                this.PlayerCount = PlayerCount;
                if(gameStarted == false) {
                    StartCountdown();
                }
            };

            PUN2Manager.OnPlayerEntered += (Player player) => {
                this.PlayerCount = PhotonNetwork.PlayerList.Length;
                if(gameStarted == false) {
                    StartCountdown();
                }
            };

            PUN2Manager.OnPlayerLeft += (player) => {
                this.PlayerCount = PhotonNetwork.PlayerList.Length;
            };
        }


        private void offlineModeStart() {
            this.PlayerCount = 1;
            StartCountdown();
        }


        private void StartCountdown() {
            Debug.LogWarning("[GameController]" + PhotonNetwork.PlayerList.Length);
            if (isMultiplayerMode) {
                if(PlayerCount >= 2 ) { //他プレイヤーが1人以上いる場合
                    countdownStarted = true;
                    waitingPlayers = false;
                } else {
                    waitingPlayers = true;
                }
            } else if (isOfflineMode) {
                countdownStarted = true;
                waitingPlayers = false;
            }
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

        private void ChangeBGM(AudioClip newClip, bool loop) {
            if (audioSource.clip != newClip) {
                audioSource.Stop();
                audioSource.clip = newClip;
                audioSource.loop = loop;
                audioSource.Play();
            }
        }

    }


}