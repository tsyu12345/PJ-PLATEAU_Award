using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DeviceManager;
using Newtonsoft.Json;


#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour {
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;

		public float splintStrengthThreshold = 1.5f;

		public GameObject destination; // 目的地
		public float maxSpeed = 5.0f; // 最大速度

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

		private DeviceInputManager deviceInputManager;
		private NavMeshAgent agent; // ナビメッシュエージェント
		private readonly Queue<Action> _mainThreadActions = new Queue<Action>();


#if ENABLE_INPUT_SYSTEM

		/*
		public void OnMove(string json) {
			InputData data = JsonConvert.DeserializeObject<InputData>(json);
			Vector2 newMoveDirection = new Vector2(0f, -data.strength); //NOTE: -を付けないと逆（後方）になる
			if(data.strength >= splintStrengthThreshold) {
				SprintInput(true);
			} else {
				SprintInput(false);
			}
			MoveInput(newMoveDirection);
		}
		*/
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value) {
			SprintInput(value.isPressed);
		}
#endif

		void Start() {
            deviceInputManager = GetComponent<DeviceInputManager>();
            agent = GetComponent<NavMeshAgent>();

            // エージェントの目的地を設定
            if (destination != null) {
                agent.SetDestination(destination.transform.position);
            }

        }

        void Update() {
            // エージェントが目的地に向かって移動している場合のみ、キャラクターの向きを調整
            if (!agent.pathPending && agent.remainingDistance > agent.stoppingDistance) {
                Vector3 direction = agent.desiredVelocity.normalized;
                if (direction != Vector3.zero) {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5.0f);
                }
            }
        }


		public void MoveInput(Vector2 newMoveDirection) {
			Debug.Log("MoveInput: " + newMoveDirection);
			move = newMoveDirection;
		}

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}

}