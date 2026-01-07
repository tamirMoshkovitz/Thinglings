using System;
using _SLIME.BaseScripts;
using UnityEngine;

namespace _SLIME.Slime
{
    public enum ConnectorState
    {
        FirstSlime,
        SecondSlime,
        OutsidePoint,
    }
    
    public class ConnectingJoint: ProjectMonoBehavior
    {
        [SerializeField] private ConnectorState state;
        [SerializeField] private SlimeBrain brain;
        [SerializeField] private int maxConnections;
        [SerializeField] private ConnectingJoint matchingConnection;
        [SerializeField] private Transform top;
        [SerializeField] private Transform mid;
        [SerializeField] private Transform bottom;
        [SerializeField] private ConnectingJoint leftJoint;
        [SerializeField] private ConnectingJoint rightJoint;
        private static float _cooldownTime = .5f; 
        private static bool _canConnect = true;
        private float _deathCooldownTimer;
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

        private void OnEnable()
        {
            SlimeEvents.SlimeGetHit += OnSlimeGetHit;
        }

        private void Update()
        {
            if (_deathCooldownTimer >= _cooldownTime)
            {
                _deathCooldownTimer = 0;
                _canConnect = true;
            }
            else if (_canConnect == false)
            {
                _deathCooldownTimer += Time.deltaTime;
            }
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (!_canConnect) return;
            
            var otherJoint = other.GetComponent<ConnectingJoint>();
            if(otherJoint == null) return;
            AddConnection(otherJoint,this);
            AddConnection(otherJoint.leftJoint,this.rightJoint);
            AddConnection(otherJoint.rightJoint,this.leftJoint);
        }

        private void AddConnection(ConnectingJoint otherJoint, ConnectingJoint connectingJoint)
        {
            if (connectingJoint.GetInstanceID() < otherJoint.GetInstanceID()
                                   && connectingJoint.state != otherJoint.state)
            {
                brain.TryAddConnection(otherJoint, connectingJoint);
            }
        }

        private void OnSlimeGetHit()
        {
            _canConnect = false;
        }
    }
}