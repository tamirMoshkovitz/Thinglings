using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace _SLIME.Slime
{
    public struct ConnectionsComponents
    {
        public EdgeCollider2D edgeColliderConnections;
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
        }

        public void OnDisable()
        {
            SlimeEvents.SlimeConnected -= OnSlimeConnected;
            SlimeEvents.SlimeGetHit -= OnSlimeGotHit;
        }

        public void Update()
        {
            UpdateConnections();
            UpdateSlimeData();
        }

        public void FixedUpdate()
        {
            _slimeConnectionVisuals.FixedUpdate();
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
            _slimeConnectionVisuals.AddVisualLine(connectorOne, connectorTwo);
            _slimeConnectionPyshics.AddJoint(connectorTwo, connectorOne);
            _slimeConnectionVisuals.AddVisualLine(connectorTwo, connectorOne);
            AddConnectionToDict(connectorOne, connectorTwo);
            AddConnectionToDict(connectorTwo, connectorOne);
            UpdateConnectionOfSlime(connectorOne, connectorTwo);
        }

        private void UpdateConnectionOfSlime(ConnectingJoint connectorOne, ConnectingJoint connectorTwo, int numOfConnections = 1)
        {
            if ((connectorOne.State == ConnectorState.FirstSlime && connectorTwo.State == ConnectorState.SecondSlime) ||
                (connectorTwo.State == ConnectorState.SecondSlime && connectorOne.State == ConnectorState.FirstSlime))
                _numOfSlimeConnections += numOfConnections;
        }

        private bool CheckSlimeMaxConnections(ConnectingJoint connectorOne, ConnectingJoint connectorTwo)
        {
            if ((connectorOne.State == ConnectorState.FirstSlime && connectorTwo.State == ConnectorState.SecondSlime) ||
                (connectorTwo.State == ConnectorState.SecondSlime && connectorOne.State == ConnectorState.FirstSlime))
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
            
            if (_slimeDied)
            {
                toRemoveObjects = _slimeConnectionPyshics.TearAllConnections();
                _slimeDied = false;
            }
            else
            {
                toRemoveObjects = _slimeConnectionPyshics.CheckForBrokenConnections();
            }

            foreach (var pair in toRemoveObjects)
            {
                UpdateConnectionOfSlime(pair.Item1, pair.Item2,-1);
                _objectsConnections[pair.Item1].Remove(pair.Item2);
                _slimeConnectionVisuals.RemoveSegment(pair.Item1, pair.Item2);
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

        private void OnSlimeGotHit(GameObject slime)
        {
            _slimeDied = true;
        }

        private void OnSlimeConnected()
        {
            _slimeDied = false;
        }
    }
}
    