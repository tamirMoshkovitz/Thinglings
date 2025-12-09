using System;
using _SLIME.Gameplay.Slime.Scripts.new_scripts;
using Audio;
using UnityEngine;

namespace _SLIME.Gameplay.Slime
{
    public enum ConnectorState
    {
        FirstSlime,
        SecondSlime,
        OutsidePoint,
    }
    
    

    
    public class NewConnectingJoint: ProjectMonoBehavior
    {
        [SerializeField] private ConnectorState state;
        [SerializeField] private SlimeBrain brain;
        [SerializeField] private int maxConnections;
        [SerializeField] private NewConnectingJoint matchingConnection;
        [SerializeField] private Transform top;
        [SerializeField] private Transform mid;
        [SerializeField] private Transform bottom;
        public int MaxConnections => maxConnections;
        public Transform Top => top;
        public Transform Mid => mid;
        public Transform Bottom => bottom;
        public ConnectorState State => state;
        private void Start()
        {
            if(matchingConnection != null && 
               GetInstanceID() < matchingConnection.GetInstanceID()) brain.TryAddConnectionAtStart(matchingConnection, this);
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            var otherJoint = other.GetComponent<NewConnectingJoint>();
            if (otherJoint != null && GetInstanceID() < otherJoint.GetInstanceID()
                && state != otherJoint.state)
            {
                brain.TryAddConnection(otherJoint, this);
            }
        }
    }
}