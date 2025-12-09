using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

namespace _SLIME.Gameplay.Slime.Scripts.new_scripts
{
    public class SlimeConnectionPyshics
    {
        private readonly SlimeConfiguration _slimeConfig;

        private readonly Dictionary<SpringJoint2D, (NewConnectingJoint, NewConnectingJoint)> _joints =
            new Dictionary<SpringJoint2D, (NewConnectingJoint, NewConnectingJoint)>();
        public SlimeConnectionPyshics(SlimeConfiguration slimeConfiguration)
        {
            _slimeConfig = slimeConfiguration;
        }

        public void AddJoint(NewConnectingJoint source, NewConnectingJoint target)
        {
            SpringJoint2D joint = source.AddComponent<SpringJoint2D>();
            Rigidbody2D targetBody = target.GetComponent<Rigidbody2D>();
            joint.connectedBody = targetBody;
            joint.distance = 0f;
            joint.autoConfigureDistance = false;
            joint.frequency = _slimeConfig.ConnectionFrequency;
            joint.breakForce = _slimeConfig.MaxStretch;
            joint.dampingRatio = _slimeConfig.ConnectionDampingRatio;
            _joints[joint] = (source, target);
        }
        
        
        public List<(NewConnectingJoint,NewConnectingJoint)> CheckForBrokenConnections()
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
    }
}