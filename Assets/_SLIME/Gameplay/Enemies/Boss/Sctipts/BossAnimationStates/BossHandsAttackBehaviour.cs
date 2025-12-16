using UnityEngine;
using DG.Tweening;

public class BossHandsAttackBehaviour : BossBaseBehaviour
{
    [Header("Smash Settings")]
    public int smashesToPreform = 3;
    
    private Sequence _smashSequence;
    private Transform _activeHand;
    private Transform _activeTarget;
    private int _smashCounter;
    private Animator _animator;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        
        _animator = animator;
        _smashCounter = 0;

        StartNextSmash();
    }

    private void StartNextSmash()
    {
        if (_smashCounter >= smashesToPreform)
        {
            _animator.SetTrigger("Hide");
            return;
        }

        bool isLeft = Random.value > 0.5f;
        if (isLeft)
        {
            _activeHand = data.leftHand;
            _activeTarget = data.playerLeftSlime;
        }
        else
        {
            _activeHand = data.rightHand;
            _activeTarget = data.playerRightSlime;
        }

        _activeHand.DOKill();

        CreateSmashSequence();
    }

    private void CreateSmashSequence()
    {
        _smashSequence = DOTween.Sequence();
        
        _smashSequence.Append(
            _activeHand.DOMoveY(data.prepareHeight, data.riseDuration)
            .SetEase(Ease.OutBack)
        );
        
        _smashSequence.Append(
            DOVirtual.Float(0, 1, data.trackDuration, (v) => {})
            .OnUpdate(TrackTargetFrame)
        );

        _smashSequence.Append(
            _activeHand.DOShakePosition(0.3f, 0.5f, 20, 90)
        );

        _smashSequence.Append(
            _activeHand.DOMoveY(data.floorHeight, data.smashDuration)
            .SetEase(Ease.InExpo)
        );
        
        _smashSequence.AppendCallback(() => {
            if (data.mainCamera) data.mainCamera.DOShakePosition(0.3f, 0.5f, 20);
        });
        
        _smashSequence.AppendInterval(0.5f);
        
        _smashSequence.Append(
            _activeHand.DOMoveY(data.hoverHeight, data.recoverDuration)
            .SetEase(Ease.InOutQuad)
        );
        
        _smashSequence.OnComplete(() => 
        {
            _smashCounter++;
            StartNextSmash();
        });
    }

    private void TrackTargetFrame()
    {
        if (_activeTarget == null || _activeHand == null) return;

        Vector3 pos = _activeHand.position;
        float targetX = _activeTarget.position.x;
        
        pos.x = Mathf.Lerp(pos.x, targetX, Time.deltaTime * data.trackSpeed);
        pos.y = data.prepareHeight; 
        
        _activeHand.position = pos;
    }
    

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_smashSequence != null && _smashSequence.IsActive())
        {
            _smashSequence.Kill();
        }
    }
}