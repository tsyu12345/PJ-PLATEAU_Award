using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCamera : MonoBehaviour {

    private Transform player;
    [Header("Camera Settings")]
    public float distance = 0.0f; // カメラを後ろにどれだけ配置するか

    void Start() {
        //PUN2より、Playerがスポーンしたイベントで初期化する
        PlayerController.OnLoad += () => {
            player = GameObject.FindWithTag("Player").transform;
            transform.position = new Vector3(player.transform.position.x, 30, player.transform.position.z - distance);
            transform.LookAt(player.transform);
        };
    }
    void Update() {
        // プレイヤーのx,z座標に基づいてカメラの位置を更新
        if(player == null) { return; }
        Vector3 newPosition = transform.position;
        newPosition.x = player.position.x;
        newPosition.z = player.position.z;

        // カメラの位置を設定
        transform.position = newPosition;
        // 常にプレイヤーの方を向く
        transform.LookAt(player);
    }
}
