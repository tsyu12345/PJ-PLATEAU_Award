using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class PUN2Manager : MonoBehaviourPunCallbacks {
    
    void Start(){
        // PhotonServerSettingsの設定内容を使ってマスターサーバーへ接続する
        PhotonNetwork.ConnectUsingSettings();
    }

    /// <summary>
    /// マスターサーバーへの接続が成功した時に呼ばれるコールバック
    /// </summary>
    public override void OnConnectedToMaster() {
        // "Room"という名前のルームに参加する（ルームが存在しなければ作成して参加する）
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions(), TypedLobby.Default);
        Debug.Log("[Photon]マスターサーバーへの接続に成功しました");
    }

    /// <summary>
    /// ゲームサーバーへの接続が成功した時に呼ばれるコールバック
    /// </summary>
    public override void OnJoinedRoom() {
        Debug.Log("[Photon]ルームへの参加に成功しました");
        //Fieldオブジェクト中のStart（Respownタグ）を持つオブジェクトを探す
        var field = GameObject.FindWithTag("Respawn");
        //Fieldオブジェクトの中にあるStart（Respownタグ）を持つオブジェクトの位置にプレイヤーを生成する
        var startPosition = field.transform.position;
        //プレイヤーを生成する（y軸だけ少し浮かせる）
        var position = new Vector3(startPosition.x, startPosition.y + 1.0f, startPosition.z);
        PhotonNetwork.Instantiate("Avatar", position, Quaternion.identity);
    }

    /// <summary>
    /// 他プレイヤーがルームへ参加した時に呼ばれるコールバック
    /// </summary>
    /// <param name="newPlayer"></param>
    public override void OnPlayerEnteredRoom(Player newPlayer) {
        Debug.Log($"{newPlayer.NickName}が参加しました");
    }

    /// <summary>
    /// 他プレイヤーがルームから退出した時に呼ばれるコールバック
    /// </summary>
    /// <param name="otherPlayer"></param>
    public override void OnPlayerLeftRoom(Player otherPlayer) {
        Debug.Log($"{otherPlayer.NickName}が退出しました");
    }
}
