using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FateGames
{
    public class CameraFollow : MonoBehaviour
    {
        public static CameraFollow Instance = null;
        public Transform Target = null;
        public Vector3 Offset = Vector3.zero;
        public float Speed = 1;
        public bool PhysicsFollow = false;
        private bool fallCamera = false;
        [SerializeField] private bool freezeX = false;
        [SerializeField] private bool freezeY = false;
        [SerializeField] private bool freezeZ = false;

        private void Awake()
        {
            if (!Instance)
                Instance = this;
            else
            {
                DestroyImmediate(gameObject);
                return;
            }
        }

        private void LateUpdate()
        {
            if (!PhysicsFollow && Target)
                Follow();
        }
        private void FixedUpdate()
        {
            if (PhysicsFollow && Target)
                Follow();
            if (fallCamera)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(Target.transform.position - transform.position), Time.fixedDeltaTime * 5);
            }
        }

        public void SwitchFallCamera()
        {
            fallCamera = true;
            freezeX = true;
            freezeY = true;
            freezeZ = true;
        }

        private void Follow()
        {
            Vector3 pos = Target.position + Offset;
            if (freezeX)
                pos.x = transform.position.x;
            if (freezeY)
                pos.y = transform.position.y;
            if (freezeZ)
                pos.z = transform.position.z;
            transform.position = Vector3.Lerp(transform.position, pos, Speed * Time.deltaTime);
        }
    }

}
