using System.Collections.Generic;
using _SLIME.Gameplay.Slime;
using _SLIME.Gameplay.Slime.Scripts.new_scripts;
using Unity.VisualScripting;
using UnityEngine;

namespace Player
{
    public struct ConnectionsComponents
    {
        public EdgeCollider2D edgeColliderConnections;
    }
    public class SlimeConnections
    {
        private Dictionary<NewConnectingJoint, List<NewConnectingJoint>> _objectsConnections =
            new Dictionary<NewConnectingJoint, List<NewConnectingJoint>>();
        
        
        private readonly SlimeConfiguration _slimeConfig;
        private SlimeData _slimeData;
        private readonly SlimeConnectionPyshics _slimeConnectionPyshics;
        private readonly SlimeConnectionsVisuals _slimeConnectionVisuals;
        private readonly ConnectionsComponents _connectionsComponents;

        public SlimeConnections(SlimeConfiguration slimeConfiguration, SlimeData slimeData, ConnectionsComponents connectionsComponents)
        {
            _slimeConfig = slimeConfiguration;
            _slimeData = slimeData;
            _slimeConnectionPyshics = new SlimeConnectionPyshics(_slimeConfig, _slimeData, connectionsComponents);
            _slimeConnectionVisuals = new SlimeConnectionsVisuals(_slimeConfig, _slimeData, connectionsComponents);
            _connectionsComponents = connectionsComponents;
        }

        public void TryAddConnection(NewConnectingJoint connectorOne, NewConnectingJoint connectorTwo)
        {
            //TODO : return if there are MAX_CONNECTIONS already
            if (CheckIfConnected(connectorOne, connectorTwo) || CheckIfMaxedConnections(connectorOne)
                                                             || CheckIfMaxedConnections(connectorTwo)) return;
            _slimeConnectionPyshics.AddJoint(connectorOne, connectorTwo);
            _slimeConnectionVisuals.AddVisualLine(connectorOne, connectorTwo);
            _slimeConnectionPyshics.AddJoint(connectorTwo, connectorOne);
            _slimeConnectionVisuals.AddVisualLine(connectorTwo, connectorOne);
            AddConnectionToDict(connectorOne, connectorTwo);
            AddConnectionToDict(connectorTwo, connectorOne);
        }

        private void AddConnectionToDict(NewConnectingJoint source, NewConnectingJoint target)
        {
            if (!_objectsConnections.ContainsKey(source))
                _objectsConnections[source] = new List<NewConnectingJoint>();
            _objectsConnections[source].Add(target);
        }


        private bool CheckIfMaxedConnections(NewConnectingJoint connector)
        {
            return _objectsConnections.ContainsKey(connector)
                   && _objectsConnections[connector].Count >= connector.MaxConnections;
        }

        private bool CheckIfConnected(NewConnectingJoint connectorOne, NewConnectingJoint connectorTwo)
        {
            return _objectsConnections.ContainsKey(connectorOne) &&
                    _objectsConnections[connectorOne].Contains(connectorTwo);
        }


        public void Update()
        {
            UpdateConnections();
            UpdateSlimeData();
        }

        private void UpdateConnections()
        {
            List<(NewConnectingJoint, NewConnectingJoint)> toRemoveObjects
                = _slimeConnectionPyshics.CheckForBrokenConnections();
            foreach (var pair in toRemoveObjects)
            {
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
                SlimeEvents.SlimeConnected?.Invoke();
                _slimeData.Connected = true;
            }
            else if (_slimeData.Connected && counter == 0)
            {
                SlimeEvents.SlimeTears?.Invoke();
                _slimeData.Connected = false;
            }
            
            _slimeData.SpringJointCount = counter;
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
    }
}
    