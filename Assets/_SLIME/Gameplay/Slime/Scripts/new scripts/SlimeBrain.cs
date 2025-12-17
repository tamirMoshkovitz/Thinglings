using System.Collections;
using _SLIME.Gameplay.Slime.Scripts.SlimeComponents;
using _SLIME.Gameplay.Slime.SlimePowers;
using Audio;
using Player;
using Player.new_scripts;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

namespace _SLIME.Gameplay.Slime.Scripts.new_scripts
{
    [RequireComponent(typeof(PlayerInput))]
    public class SlimeBrain: ProjectMonoBehavior
    {
        #region Serialized Fields
        [SerializeField] private SlimeConfiguration slimeConfiguration;
        
        [SerializeField] private GameObject slimeLeftSide; // Only for reference in the inspector - do not use in code!
        [SerializeField] private Transform slimeLeftSideAnchor; // Only for reference in the inspector - do not use in code!
        [SerializeField] private GameObject leftSideHitPoint;
        
        [SerializeField] private GameObject slimeRightSide; // Only for reference in the inspector - do not use in code!
        [SerializeField] private Transform slimeRightSideAnchor; // Only for reference in the inspector - do not use in code!
        [SerializeField] private GameObject rightSideHitPoint;
        
        [SerializeField] private EdgeCollider2D edgeColliderConnections;
        [SerializeField] private TriggerSensor edgeColliderSensor;
        [SerializeField] private float controlSwitchDelay = 0.5f;
        [SerializeField] private float controlSwitchThreshold = 0.5f;
        
        [Header("Feel Manager Settings")]
        [SerializeField] private ConrollerRumbleConfiguration controllerRumbleConfiguration;
        [SerializeField] private SlimeStretchCameraShakeConfiguration slimeStretchCameraShakeConfiguration;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float  shotTearConnectionDelay = .3f;
        #endregion
        
        private SlimeData _slimeData;
        private SlimeFeelManager _feelManager;
        private SlimeSide _leftSide, _rightSide;
        private Slime.SlimePowers.SlimePowers _slimePowers;
        private SlimeConnections _slimeConnections;
        
        private bool _isMoveLeftCancelled = true, _isMoveRightCancelled = true;
        private Coroutine _controlSwitchCoroutine;
        
        public SlimeData Data => _slimeData;

        private void Awake()
        {
            InitializeFields();
            InitializeSlimeData();
        }
        
        private void OnEnable()
        {
            _leftSide.OnEnable();
            _rightSide.OnEnable();
            _feelManager.OnEnable();
            _slimeConnections.OnEnable();
            
            SlimeEvents.SlimeTears += OnSlimeTears;
        }
        
        private void OnDisable()
        {
            _leftSide.OnDisable();
            _rightSide.OnDisable();
            _feelManager.OnDisable();
            _slimeConnections.OnDisable();
            
            SlimeEvents.SlimeTears -= OnSlimeTears;
        }

        private void Update()
        {
            UpdateSlimeControls();
            UpdateSlimeData();
            _slimeConnections.Update();
            _leftSide.Update();
            _rightSide.Update();
            _feelManager.Update();
            _slimePowers.Update();
        }

        private void FixedUpdate()
        {
            _leftSide.FixedUpdate();
            _rightSide.FixedUpdate();
            _slimeConnections.FixedUpdate();
        }

        private void LateUpdate()
        {
            _slimeConnections.LateUpdate();
        }

        private void OnDestroy()
        {
            _feelManager.OnDestroy();
        }

        private void InitializeFields()
        {
            _rightSide = new SlimeSide(new SlimeSide.SlimeSideFormat(
                slimeRightSide,
                slimeRightSideAnchor,
                slimeConfiguration.MoveSpeed,
                slimeConfiguration.MaxHealth,
                rightSideHitPoint
            ));
            
            _leftSide = new SlimeSide(new SlimeSide.SlimeSideFormat(
                slimeLeftSide,
                slimeLeftSideAnchor,
                slimeConfiguration.MoveSpeed,
                slimeConfiguration.MaxHealth,
                leftSideHitPoint
            ));
            
            _slimeData = new SlimeData(_rightSide, _leftSide);
            
            _slimePowers = new Slime.SlimePowers.SlimePowers(slimeConfiguration,new PowerComponents
            {
                connectionsTriggerSensor = edgeColliderSensor
            } , _slimeData);
            _slimeConnections = new SlimeConnections(slimeConfiguration,_slimeData, new ConnectionsComponents
            {
                edgeColliderConnections = edgeColliderConnections,
            } );
            
            _feelManager = new SlimeFeelManager(this, controllerRumbleConfiguration,
                slimeStretchCameraShakeConfiguration, mainCamera, shotTearConnectionDelay);
        }

        public void OnMoveRight(InputAction.CallbackContext context)
        {
            _isMoveRightCancelled = context.canceled;
            _rightSide.OnMove(context);
        }

        public void OnMoveLeft(InputAction.CallbackContext context)
        {
            _isMoveLeftCancelled = context.canceled;
            _leftSide.OnMove(context);
        }

        private void UpdateSlimeControls()
        {
            bool bothCancelled = _isMoveRightCancelled && _isMoveLeftCancelled;

            if (bothCancelled)
            {
                if (_controlSwitchCoroutine == null)
                {
                    _controlSwitchCoroutine = StartCoroutine(ControlSwitchCoroutine());
                }
            }
            else
            {
                if (_controlSwitchCoroutine != null)
                {
                    StopCoroutine(_controlSwitchCoroutine);
                    _controlSwitchCoroutine = null;
                }
            }
        }

        private void SwitchSlimeSides()
        {
            (_leftSide, _rightSide) = (_rightSide, _leftSide);
        }
        
        #region ConnectionFunctions

        public void TryAddConnection(NewConnectingJoint connectorOne, NewConnectingJoint connectorTwo)
        {
            _slimeConnections.TryAddConnection(connectorOne, connectorTwo);
        }
        public void TryAddConnectionAtStart(NewConnectingJoint connectorOne, NewConnectingJoint connectorTwo)
        {
            if (!slimeConfiguration.IsConnectedAtStart) return;
            _slimeConnections.TryAddConnection(connectorOne, connectorTwo);
        }

        #endregion

        private IEnumerator ControlSwitchCoroutine()
        {
            yield return new WaitForSeconds(controlSwitchDelay);

            if (_isMoveRightCancelled && _isMoveLeftCancelled && _leftSide.Position.x > _rightSide.Position.x + controlSwitchThreshold)
            {
                SwitchSlimeSides();
            }

            _controlSwitchCoroutine = null;
        }


        private void InitializeSlimeData()
        {
            _slimeData.Connected = slimeConfiguration.IsConnectedAtStart;
        }

        private void OnTearFinished() // Called by the FeelManager via Invoke
        {
            _feelManager.OnTearFinished();
        }
        
        private void OnSlimeTears()
        {
            _feelManager.OnSlimeTears();
        }
        
        private void UpdateSlimeData()
        {
            _slimeData.TopLineConnectionPositionLeft = _leftSide.TopPosition.position;
            _slimeData.TopLineConnectionPositionRight = _rightSide.TopPosition.position;
        }
    }
}