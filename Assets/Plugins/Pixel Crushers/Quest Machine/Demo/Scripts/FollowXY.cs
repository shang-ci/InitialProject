// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.QuestMachine.Demo
{

    /// <summary>
    /// Makes the GameObject follow another GameObject on the XY plane.
    /// </summary>
    public class FollowXY : MonoBehaviour
    {
        public Transform followTarget;


        private void Start()
        {
            // 在游戏开始时，如果发现没有设置跟随目标
            if (followTarget == null)
            {
                // 就自动去场景里寻找标签为 "Player" 的游戏对象
                GameObject player = GameObject.FindWithTag("Player");

                // 如果找到了，就把它设置为跟随目标
                if (player != null)
                {
                    followTarget = player.transform;
                    Debug.Log("Camera found the Player to follow.");
                }
                else
                {
                    Debug.LogError("Camera could not find an object with tag 'Player'!");
                }
            }
        }


        private void Update()
        {
            transform.position = new Vector3(followTarget.position.x, followTarget.position.y, transform.position.z);
        }

    }

}