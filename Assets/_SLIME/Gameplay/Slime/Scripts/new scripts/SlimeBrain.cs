using System.Collections;
using _SLIME.Gameplay.Slime.Scripts.SlimeComponents;
using Audio;
using Player;
using Player.new_scripts;
using UnityEngine;
using UnityEngine.InputSystem;
    
namespace _SLIME.Gameplay.Slime.Scripts.new_scripts
{
    [RequireComponent(typeof(PlayerInput))]
    public class SlimeBrain: ProjectMonoBehavior
    {
        [SerializeField] private SlimeConfiguration slimeConfiguration;
        [SerializeField] private GameObject slimeLeftSide; // Only for reference in the inspector - do not use in code!
        [SerializeField] private GameObject slimeRightSide; // Only for reference in the inspector - do not use in code!
        [SerializeField] private float controlSwitchDelay = 0.5f;
        [SerializeField] private float controlSwitchThreshold = 0.5f;
        
        [Header("Feel Manager Settings")]
        [SerializeField] private ConrollerRumbleConfiguration controllerRumbleConfiguration;
        [SerializeField] private SlimeStretchCameraShakeConfiguration slimeStretchCameraShakeConfiguration;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float  shotTearConnectionDelay = .3f;
        
        private SlimeData _slimeData;
        private SlimeFeelManager _feelManager;
        private SlimeSide _leftSide, _rightSide;
        private SlimePowers _slimePowers;
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
        }
        
        private void OnDisable()
        {
            _leftSide.OnDisable();
            _rightSide.OnDisable();
            _feelManager.OnDisable();
        }

        private void Update()
        {
            UpdateSlimeControls();
            UpdateSlimeData();
            _slimeConnections.Update();
            _leftSide.Update();
            _rightSide.Update();
            
            _feelManager.Update();
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
            _slimeData = new SlimeData();
            
            _rightSide = new SlimeSide(new SlimeSide.SlimeSideFormat(
                slimeRightSide,
                slimeConfiguration.MoveSpeed,
                slimeConfiguration.MaxHealth
            ));
            
            _leftSide = new SlimeSide(new SlimeSide.SlimeSideFormat(
                slimeLeftSide,
                slimeConfiguration.MoveSpeed,
                slimeConfiguration.MaxHealth
            ));
            
            _slimePowers = new SlimePowers();
            _slimeConnections = new SlimeConnections(slimeConfiguration, _slimeData);
            
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
            _slimeData.MaxStretch = slimeConfiguration.MaxStretch;
        }

        private void UpdateSlimeData()
        {
            _slimeData.Distance = Vector3.Distance(_rightSide.Position, _leftSide.Position);
        }

        private void OnTearFinished() // Called by the FeelManager via Invoke
        {
            _feelManager.OnTearFinished();
        }
    }
}