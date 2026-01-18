using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace _SLIME.Slime
{
    [Serializable]
    public struct ConnectionsComponents
    {
        public EdgeCollider2D EdgeColliderConnections;
        public Transform TransformSlime1LinePoint;
        public Transform TransformSlime2LinePoint;
        public GameObject Spark;
    }
    
    
    
    public class SlimeConnections
    {
        private Dictionary<ConnectingJoint, List<ConnectingJoint>> _objectsConnections =
            new Dictionary<ConnectingJoint, List<ConnectingJoint>>();
        
        
        private readonly SlimeConfiguration _slimeConfig;
        private SlimeData _slimeData;
        private readonly SlimeConnectionPyshics _slimeConnectionPyshics;
        private readonly SlimeConnectionsVisuals _slimeConnectionVisuals;
        private readonly ConnectionsComponents _connectionsComponents;
        private bool _slimeDied;
        private int _numOfSlimeConnections;
        private float _currentStretchTimer = 0f; 
        private bool _shouldTearAllConnections = false;
        private bool _tearByIcicle = false;

        public SlimeConnections(SlimeConfiguration slimeConfiguration, SlimeData slimeData, ConnectionsComponents connectionsComponents)
        {
            _slimeConfig = slimeConfiguration;
            _slimeData = slimeData;
            _slimeConnectionPyshics = new SlimeConnectionPyshics(_slimeConfig, _slimeData, connectionsComponents);
            _slimeConnectionVisuals = new SlimeConnectionsVisuals(_slimeConfig, _slimeData, connectionsComponents);
            _connectionsComponents = connectionsComponents;
        }

        public void OnEnable()
        {
            SlimeEvents.SlimeConnected += OnSlimeConnected;
            SlimeEvents.SlimeGetHit += OnSlimeGotHit;
            SlimeEvents.SlimeTears += OnSlimeTears;
            SlimeEvents.SlimeConnectionGotHitByIcicle += OnSlimeConnectionGotHitByIcicle;
        }

        private void OnSlimeConnectionGotHitByIcicle()
        {
            _tearByIcicle = true;
        }


        public void OnDisable()
        {
            SlimeEvents.SlimeConnected -= OnSlimeConnected;
            SlimeEvents.SlimeGetHit -= OnSlimeGotHit;
            SlimeEvents.SlimeTears -= OnSlimeTears;
            SlimeEvents.SlimeConnectionGotHitByIcicle -= OnSlimeConnectionGotHitByIcicle;
        }

        public void Update()
        {
            CheckTimeAtMaxStretch();
            UpdateConnections();
            UpdateSlimeData();
        }

        private void CheckTimeAtMaxStretch()
        {
        
            float stretchPercentThreshold = _slimeConfig.MaxStretchPercentThreshold / 100f;
            float currentStretchRatio = _slimeData.StretchRatio;
            
            if (currentStretchRatio >= stretchPercentThreshold)
            {

                _currentStretchTimer += Time.deltaTime;
    
                if (_currentStretchTimer >= _slimeConfig.MaxStretchTimeThreshold)
                {
                    _shouldTearAllConnections = true;
                }
            }
            else
            {
                _currentStretchTimer = 0f;
                _shouldTearAllConnections = false;
            }
        }
        

        public void LateUpdate()
        {
            _slimeConnectionVisuals.LateUpdate();
            _slimeConnectionPyshics.LateUpdate();
        }

        public void TryAddConnection(ConnectingJoint connectorOne, ConnectingJoint connectorTwo)
        {
            //TODO : return if there are MAX_CONNECTIONS already
            if (CheckIfConnected(connectorOne, connectorTwo) || CheckIfMaxedConnections(connectorOne)
                                                             || CheckIfMaxedConnections(connectorTwo) ||
                                                             CheckSlimeMaxConnections(connectorOne,connectorTwo)) return;
            _slimeConnectionPyshics.AddJoint(connectorOne, connectorTwo);
            _slimeConnectionPyshics.AddJoint(connectorTwo, connectorOne);
            AddConnectionToDict(connectorOne, connectorTwo);
            AddConnectionToDict(connectorTwo, connectorOne);
            UpdateConnectionOfSlime(connectorOne, connectorTwo);
        }

        private void UpdateConnectionOfSlime(ConnectingJoint connectorOne, ConnectingJoint connectorTwo, int numOfConnections = 1)
        {
            if ((connectorOne.State == ConnectorState.FirstSlime && connectorTwo.State == ConnectorState.SecondSlime) ||
                (connectorOne.State == ConnectorState.SecondSlime && connectorTwo.State == ConnectorState.FirstSlime))
                _numOfSlimeConnections = Mathf.Max(_numOfSlimeConnections + numOfConnections,0);
        }

        private bool CheckSlimeMaxConnections(ConnectingJoint connectorOne, ConnectingJoint connectorTwo)
        {
            if ((connectorOne.State == ConnectorState.FirstSlime && connectorTwo.State == ConnectorState.SecondSlime) ||
                (connectorOne.State == ConnectorState.SecondSlime && connectorTwo.State == ConnectorState.FirstSlime))
                return _numOfSlimeConnections  >= _slimeConfig.MaxConnectionsOfSlime;
            
            return false;
        }

        private void AddConnectionToDict(ConnectingJoint source, ConnectingJoint target)
        {
            if (!_objectsConnections.ContainsKey(source))
                _objectsConnections[source] = new List<ConnectingJoint>();
            _objectsConnections[source].Add(target);
        }


        private bool CheckIfMaxedConnections(ConnectingJoint connector)
        {
            return _objectsConnections.ContainsKey(connector)
                   && _objectsConnections[connector].Count >= connector.MaxConnections;
        }

        private bool CheckIfConnected(ConnectingJoint connectorOne, ConnectingJoint connectorTwo)
        {
            return _objectsConnections.ContainsKey(connectorOne) &&
                    _objectsConnections[connectorOne].Contains(connectorTwo);
        }


        private void UpdateConnections()
        {
            List<(ConnectingJoint, ConnectingJoint)> toRemoveObjects;
            
            if (_slimeDied || _tearByIcicle)
            {   
                toRemoveObjects = _slimeConnectionPyshics.TearAllConnections();
                _slimeDied = false;
                _tearByIcicle = false;
            }
            else if (_shouldTearAllConnections)
            {
                toRemoveObjects = _slimeConnectionPyshics.TearAllConnections();
                _shouldTearAllConnections = false;
                _currentStretchTimer = 0f; 
            }
            else
            {
                toRemoveObjects = _slimeConnectionPyshics.CheckForBrokenConnections();
                if(toRemoveObjects.Count > 0) toRemoveObjects.AddRange(_slimeConnectionPyshics.TearAllConnections());
            }

            foreach (var pair in toRemoveObjects)
            {
                if(pair.Item1 == null || pair.Item2 == null) continue;
                _objectsConnections[pair.Item1].Remove(pair.Item2);
                UpdateConnectionOfSlime(pair.Item1, pair.Item2,-1);
            }
        }

        private void UpdateSlimeData()
        {
            int counter = 0;
            foreach (var e in _objectsConnections)
            {
                counter += e.Value.Count;
            }
            
            if (!_slimeData.Connected && counter > 0)
            {
                _slimeData.Connected = true;
            }
            else if (_slimeData.Connected && counter == 0)
            {
                _slimeData.Connected = false;
            }
            
            _slimeData.SpringJointCount = counter;
        }

        private void OnSlimeGotHit()
        {
            _slimeDied = true;
        }

        private void OnSlimeConnected()
        {
            _slimeConnectionVisuals.AddVisualLine(_connectionsComponents.TransformSlime1LinePoint,
                _connectionsComponents.TransformSlime2LinePoint);
            _slimeDied = false;
        }
        private void OnSlimeTears()
        {
            _slimeConnectionVisuals.RemoveSegment();
        }
    }
}
    
