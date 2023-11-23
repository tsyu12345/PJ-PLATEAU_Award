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

        private readonly Queue<Action> _mainThreadActions = new Queue<Action>();
        public delegate void CleanedDataHandler(string data);
        public event CleanedDataHandler OnCleanedDataReceived;
        private WebSocket ws;

        void Start() {

            GetUserProfile();
            var endpoint = $"/store/strength/{userId}-{clientType}";
            Connect(endpoint);
            
            ws.OnOpen += (sender, ev) => {

            };

            ws.OnMessage += (sender, ev) => {
                //データの整形
                string cleanedData = ev.Data.Trim('"');
                cleanedData = cleanedData.Replace("\\\"", "\"");
                Debug.Log("Cleaned Data: " + cleanedData);
                OnCleanedDataReceived?.Invoke(cleanedData);
            };

            ws.OnError += (sender, e) => {
                Debug.Log("WebSocket Error Message: " + e.Message);
            };

            ws.OnClose += (sender, e) => {
                Debug.Log("WebSocket Close");
            };

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

        void OnApplicationQuit() {
            ws.Close();
            Debug.Log("WebSocket Connection Closed");
        }

    }

    [System.Serializable]
    public class User {
        public string userName;
        public string userId;
    }
}

