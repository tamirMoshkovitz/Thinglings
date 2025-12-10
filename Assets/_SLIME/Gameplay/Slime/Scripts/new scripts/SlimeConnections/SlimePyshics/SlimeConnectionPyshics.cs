using System.Collections.Generic;
using JetBrains.Annotations;
using Player;
using Player;
using Unity.VisualScripting;
using UnityEngine;

namespace _SLIME.Gameplay.Slime.Scripts.new_scripts
{
    public class SlimeConnectionPyshics
    {
        private readonly SlimeConfiguration _slimeConfig;
        private readonly SlimeData _slimeData;

        private readonly Dictionary<SpringJoint2D, (NewConnectingJoint, NewConnectingJoint)> _joints =
            new Dictionary<SpringJoint2D, (NewConnectingJoint, NewConnectingJoint)>();
        
        private readonly ConnectionsComponents _connectionsComponents;
        
        public SlimeConnectionPyshics(SlimeConfiguration slimeConfiguration, SlimeData slimeData,
            ConnectionsComponents connectionsComponents)
        {
            _connectionsComponents = connectionsComponents;
            _slimeConfig = slimeConfiguration;

            _slimeData = slimeData;
            _slimeData.SpringBreakForce = _slimeConfig.BreakForce;
            _slimeData.SpringFrequency = _slimeConfig.ConnectionFrequency;
        }

        public void AddJoint(NewConnectingJoint source, NewConnectingJoint target)
        {
            SpringJoint2D joint = source.AddComponent<SpringJoint2D>();
            Rigidbody2D targetBody = target.GetComponent<Rigidbody2D>();
            joint.connectedBody = targetBody;
            joint.distance = 0f;
            joint.autoConfigureDistance = false;
            joint.frequency = _slimeConfig.ConnectionFrequency;
            joint.breakForce = _slimeConfig.BreakForce;
            joint.dampingRatio = _slimeConfig.ConnectionDampingRatio;
            _joints[joint] = (source, target);
        }


        public List<(NewConnectingJoint, NewConnectingJoint)> CheckForBrokenConnections()
        {
            List<SpringJoint2D> jToRemove = new List<SpringJoint2D>();
            List<(NewConnectingJoint, NewConnectingJoint)> jToRemoveObjects =
                new List<(NewConnectingJoint, NewConnectingJoint)>();
            foreach (var e in _joints)
            {
                SpringJoint2D j = e.Key;
                if (!j || !j.connectedBody)
                {
                    jToRemove.Add(j);
                    jToRemoveObjects.Add(e.Value);
                }
            }

            foreach (var j in jToRemove)
            {
                _joints.Remove(j);
            }

            return jToRemoveObjects;
        }

        public void LateUpdate()
        {
            UpdateConectionsCollider();
        }

        private void UpdateConectionsCollider()
        {
            _connectionsComponents.edgeColliderConnections.enabled = _slimeData.Connected;
            Vector2 worldPosLeft = _slimeData.TopLineConnectionPositionLeft;
            Vector2 worldPosRight = _slimeData.TopLineConnectionPositionRight;
            Transform colliderTransform = _connectionsComponents.edgeColliderConnections.transform;

            Vector2 localPosLeft = colliderTransform.InverseTransformPoint(worldPosLeft);
            Vector2 localPosRight = colliderTransform.InverseTransformPoint(worldPosRight);
            
            Vector2[] newPoints = new Vector2[]
            {
                localPosLeft,
                localPosRight
            };
            
            _connectionsComponents.edgeColliderConnections.points = newPoints;
        }
    }
}