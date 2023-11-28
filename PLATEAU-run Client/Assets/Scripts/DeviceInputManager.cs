using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;



namespace DeviceManager {
    /// <summary>
    /// デバイスからモーション入力を受け取るためのマネージャークラス
    /// TODO:呼び出し側から汎用的に使えるようにリファクタリング
    /// </summary>
    public class DeviceInputManager : MonoBehaviour {
        
        
        [Header("サーバー設定")]
        public string ServerAdressFile = "Assets/temp/ipconfig.txt";
        public string UserSettingFile = "Assets/Config/TESTUSER1.json";
        public string userId = "";

        public string clientType = "Unity";

        public delegate void CleanedDataHandler(string data);
        public event CleanedDataHandler OnCleanedDataReceived;
        private static readonly Queue<Action> _mainThreadActions = new Queue<Action>();
        private WebSocket ws;

        void Start() {

            GetUserProfile();
            var endpoint = $"/store/strength/{userId}-{clientType}";
            Connect(endpoint);
            
            ws.OnOpen += (sender, ev) => {

            };

            ws.OnMessage += (sender, ev) => {
                try {
                    //データの整形
                    string cleanedData = ev.Data.Trim('"');
                    cleanedData = cleanedData.Replace("\\\"", "\"");
                
                   // メインスレッドで実行するアクションをキューに追加
                    _mainThreadActions.Enqueue(() => {
                        OnCleanedDataReceived?.Invoke(cleanedData);
                    });
                } catch (Exception e) {
                    Debug.LogError($"Error: {e}");
                }

            };

            ws.OnError += (sender, e) => {
                Debug.Log("WebSocket Error Message: " + e.Message);
            };

            ws.OnClose += (sender, e) => {
                Debug.Log("WebSocket Close");
            };
        }

        void Update() {
            while (_mainThreadActions.Count > 0) {
                var action = _mainThreadActions.Dequeue();
                if(action != null) { 
                    action.Invoke();
                }
            }
        }

        void OnApplicationQuit() {
            DisConnect();
        }


        public void AddEventListener(CleanedDataHandler listener) {
            OnCleanedDataReceived += listener;
        }

        // イベントリスナーを削除するメソッド
        public void RemoveEventListener(CleanedDataHandler listener) {
            OnCleanedDataReceived -= listener;
        }

        public void DisConnect() {
            ws.Close();
        }

        /// <summary>
        /// ユーザーのプロフィールのJSONを読み、userIdを取得する
        /// </summary>
        private void GetUserProfile() {
            var json = File.ReadAllText(UserSettingFile);
            var user = JsonConvert.DeserializeObject<User>(json);
            userId = user.userId;
        }


        private void Connect(string endpoint) {
            var ipconfig = File.ReadAllText(ServerAdressFile);
            var ServerAddress = "ws://" + ipconfig.Trim();
            var uri = ServerAddress + $"{endpoint}";
            Debug.Log("Start Request " + uri);
            ws = new WebSocket(uri);
            ws.Connect();
        }
    }

    [System.Serializable]
    public class User {
        public string userName;
        public string userId;
    }
}

