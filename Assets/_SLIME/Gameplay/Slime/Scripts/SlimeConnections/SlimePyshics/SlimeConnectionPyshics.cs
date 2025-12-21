using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace _SLIME.Slime
{
    public class SlimeConnectionPyshics
    {
        private readonly SlimeConfiguration _slimeConfig;
        private readonly SlimeData _slimeData;

        private static readonly Dictionary<SpringJoint2D, (ConnectingJoint, ConnectingJoint)> _joints =
            new Dictionary<SpringJoint2D, (ConnectingJoint, ConnectingJoint)>();
        
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

        public void AddJoint(ConnectingJoint source, ConnectingJoint target)
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

        public static void ChangeJointsAttributes(float frequency, float breakForce)
        {
            Debug.Log("change joints attributes " + frequency + " to " + breakForce);
            foreach (var j in _joints.Keys)
            {
                j.breakForce = breakForce;
                j.frequency = frequency;
            }
        }
        
        


        public List<(ConnectingJoint, ConnectingJoint)> CheckForBrokenConnections()
        {
            List<SpringJoint2D> jToRemove = new List<SpringJoint2D>();
            List<(ConnectingJoint, ConnectingJoint)> jToRemoveObjects =
                new List<(ConnectingJoint, ConnectingJoint)>();
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

        /// <summary>
        /// Force-breaks all existing connections immediately and returns the connection pairs that were removed.
        /// This is useful when other systems (e.g., visuals) need to know exactly which connections were torn.
        /// </summary>
        public List<(ConnectingJoint, ConnectingJoint)> TearAllConnections()
        {
            var removed = new List<(ConnectingJoint, ConnectingJoint)>();

            if (_joints.Count == 0)
                return removed;

            // Capture the connection data for visuals BEFORE we clear/destroy.
            removed.AddRange(_joints.Values);

            // Copy keys to avoid modifying the dictionary while iterating.
            var jointsToDestroy = new List<SpringJoint2D>(_joints.Keys);

            foreach (var joint in jointsToDestroy)
            {
                if (joint)
                {
                    // Destroying the joint component breaks the connection.
                    Object.Destroy(joint);
                }
            }

            _joints.Clear();

            // If the rest of the system uses this flag to drive visuals/colliders, update it.
            _slimeData.Connected = false;

            return removed;
        }
    }
}