using System.Collections;
using _SLIME.BaseScripts;
using _SLIME.GameLoop;
using _SLIME.Projectiles;
using Unity.VisualScripting;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace _SLIME.Slime
{
    [RequireComponent(typeof(PlayerInput))]
    public class SlimeBrain: ProjectMonoBehavior
    {
        #region Serialized Fields
        [SerializeField] private SlimeConfiguration slimeConfiguration;
        
        [SerializeField] private GameObject slimeLeftSide; // Only for reference in the inspector - do not use in code!
        [SerializeField] private Transform slimeLeftSideAnchor; // Only for reference in the inspector - do not use in code!
        [SerializeField] private GameObject leftSideHitPoint;
        [SerializeField] private Renderer slimeLeftSideRenderer;
        
        [SerializeField] private GameObject slimeRightSide; // Only for reference in the inspector - do not use in code!
        [SerializeField] private Transform slimeRightSideAnchor; // Only for reference in the inspector - do not use in code!
        [SerializeField] private GameObject rightSideHitPoint;
        [SerializeField] private Renderer slimeRightSideRenderer;
        
        [SerializeField] private ConnectionsComponents conenctionsComponent;
        [SerializeField] private EdgeCollider2D edgeColliderConnections;
        [SerializeField] private TriggerSensor edgeColliderSensor;
        [SerializeField] private float controlSwitchDelay = 0.5f;
        [SerializeField] private float controlSwitchThreshold = 0.5f;
        [SerializeField] private float controlSwitchDuration = 0.5f;
        
        [Header("Slime Shooting")]
        [SerializeField] private Transform bossHitPoint;
        [SerializeField] private Transform bossHitPointEyeRight;
        [SerializeField] private Transform bossHitPointEyeLeft;
        [SerializeField] private BulletMonoPool bulletPool;
        
        [Header("Slime Powers")]
        [SerializeField] private SparkPowerDep sparkPowerDep;
        
        [Header("Feel Manager Settings")]
        [SerializeField] private ConrollerRumbleConfiguration controllerRumbleConfiguration;
        [SerializeField] private SlimeStretchCameraShakeConfiguration slimeStretchCameraShakeConfiguration;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float  shotTearConnectionDelay = .3f;

        [Header("Controll Switch Animation Sprites")]
        [SerializeField] private Renderer leftSlimeSoulSprite;
        [SerializeField] private Renderer rightSlimeSoulSprite;
        [SerializeField] private ControlledSfx slimeTearSFX;
        [SerializeField] private EventReference deadSlimeSFX;
        #endregion
        
        private SlimeData _slimeData;
        private SlimeFeelManager _feelManager;
        private SlimeSide _leftSide, _rightSide;
        private SlimePowers _slimePowers;
        private SlimeConnections _slimeConnections;
        
        private bool _isMoveLeftCancelled = true, _isMoveRightCancelled = true;
        private Coroutine _controlSwitchCoroutine;
        private bool _hasLoadedDeathScene = false;

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
            _slimePowers.OnEnable();
            _slimeConnections.OnEnable();
            
            SlimeEvents.SlimeTears += OnSlimeTears;
            SlimeEvents.SlimeGetHit += OnSlimeGetHit;
        }
        
        private void OnDisable()
        {
            _leftSide.OnDisable();
            _rightSide.OnDisable();
            _feelManager.OnDisable();
            _slimeConnections.OnDisable();
            _slimePowers.OnDisable();
            
            SlimeEvents.SlimeTears -= OnSlimeTears;
            SlimeEvents.SlimeGetHit -= OnSlimeGetHit;
        }

        private void Update()
        {
            UpdateSlimeControls();
            UpdateSlimeData();
            CheckDeath();
            _slimeConnections.Update();
            _leftSide.Update();
            _rightSide.Update();
            _feelManager.Update();
            _slimePowers.Update();
        }

        private void CheckDeath()
        {
            if (_hasLoadedDeathScene) return;
            
            if (_rightSide.IsDead && _leftSide.IsDead)
            {
                _hasLoadedDeathScene = true;
                if(SceneLoader.CurrentSceneType == SceneType.BossFinalBattleScene) SceneLoader.LoadScene(SceneType.BossFinalBattleScene);
                if(SceneLoader.CurrentSceneType == SceneType.StartScene) SceneLoader.LoadScene(SceneType.StartSceneAfterDeath);
                if(SceneLoader.CurrentSceneType == SceneType.StartSceneAfterDeath) SceneLoader.LoadScene(SceneType.StartSceneAfterDeath);
            }
        }

        private void FixedUpdate()
        {
            _leftSide.FixedUpdate();
            _rightSide.FixedUpdate();
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
                slimeRightSideAnchor,
                slimeConfiguration.MoveSpeed,
                slimeConfiguration.MaxHealth,
                rightSideHitPoint,
                _slimeData,
                slimeConfiguration.shootingSettings,
                new SlimeSideShootingReqComponents(slimeRightSideRenderer, bossHitPoint,bossHitPointEyeRight, bulletPool)
                ,deadSlimeSFX,
                rightSlimeSoulSprite
            ));
            _leftSide = new SlimeSide(new SlimeSide.SlimeSideFormat(
                slimeLeftSide,
                slimeLeftSideAnchor,
                slimeConfiguration.MoveSpeed,
                slimeConfiguration.MaxHealth,
                leftSideHitPoint,
                _slimeData,
                slimeConfiguration.shootingSettings,
                new SlimeSideShootingReqComponents(slimeLeftSideRenderer, bossHitPoint, bossHitPointEyeLeft, bulletPool),
                deadSlimeSFX,
                leftSlimeSoulSprite
            ));
            
            _slimeData.Initialize(_rightSide, _leftSide);
            
            _slimePowers = new SlimePowers(slimeConfiguration,new PowerComponents
            {
                connectionsTriggerSensor = edgeColliderSensor
            } , _slimeData, sparkPowerDep);
            _slimeConnections = new SlimeConnections(slimeConfiguration,_slimeData, conenctionsComponent );
            
            _feelManager = new SlimeFeelManager(this, controllerRumbleConfiguration,
                slimeStretchCameraShakeConfiguration, mainCamera, shotTearConnectionDelay, slimeTearSFX);
        }

        public void OnMoveRight(InputAction.CallbackContext context)
        {
            if (context.canceled != _isMoveRightCancelled) ;
            _isMoveRightCancelled = context.canceled;
            _rightSide.OnMove(context);
        }

        public void OnShootLeft(InputAction.CallbackContext context)
        {
            _leftSide.OnShoot(context);
        }
        
        public void OnShootRight(InputAction.CallbackContext context)
        {
            _rightSide.OnShoot(context);
        }

        public void OnMoveLeft(InputAction.CallbackContext context)
        {
            if (context.canceled != _isMoveLeftCancelled);
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
                    _controlSwitchCoroutine = StartCoroutine(ControlSwitchCoroutine(controlSwitchDelay));
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

        private void SwitchSlimeSides(bool overrideChecks)
        {
            if (!_rightSide.IsDead && !_leftSide.IsDead || overrideChecks);
            
            (_leftSide, _rightSide) = (_rightSide, _leftSide);
        }
        
        #region ConnectionFunctions

        public void TryAddConnection(ConnectingJoint connectorOne, ConnectingJoint connectorTwo)
        {
            _slimeConnections.TryAddConnection(connectorOne, connectorTwo);
        }
        public void TryAddConnectionAtStart(ConnectingJoint connectorOne, ConnectingJoint connectorTwo)
        {
            if (!slimeConfiguration.IsConnectedAtStart) return;
            _slimeConnections.TryAddConnection(connectorOne, connectorTwo);
        }

        #endregion

        private IEnumerator ControlSwitchCoroutine(float delay, bool overrideLifeChecks = false)
        {
            yield return new WaitForSeconds(delay);
            
            if (ShouldSwitchControls(overrideLifeChecks))
            {
                SwitchSlimeSides(overrideLifeChecks);
                StartCoroutine(AnimateControlSwitch(controlSwitchDuration));
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
        
        private IEnumerator AnimateControlSwitch(float duration)
        {
            StartCoroutine(_rightSide.AnimateSideSwitch(_leftSide.Transform, duration));
            StartCoroutine(_leftSide.AnimateSideSwitch(_rightSide.Transform, duration));
            
            yield return new WaitForSeconds(duration);
            
            (_leftSide.Animator.runtimeAnimatorController, _rightSide.Animator.runtimeAnimatorController) =
                (_rightSide.Animator.runtimeAnimatorController, _leftSide.Animator.runtimeAnimatorController);
            
            leftSlimeSoulSprite.gameObject.SetActive(false);
            rightSlimeSoulSprite.gameObject.SetActive(false);
        }
        
        private bool ShouldSwitchControls(bool overrideLifeChecks)
        {
            return IsSlimeSidesSwitched() && ShouldTrySwitchControls(overrideLifeChecks);
        }

        private bool IsSlimeSidesSwitched()
        {
            return _leftSide.Position.x > _rightSide.Position.x + controlSwitchThreshold;
        }

        private bool ShouldTrySwitchControls(bool overrideLifeChecks)
        {
            return overrideLifeChecks ||
                (_isMoveRightCancelled && _isMoveLeftCancelled && !_rightSide.IsDead && !_leftSide.IsDead);
        }

        private void OnSlimeGetHit()
        {
            if (_rightSide.IsDead && _leftSide.IsDead) return;
            StartCoroutine(ControlSwitchCoroutine(0.1f, true));
        }
    }
}