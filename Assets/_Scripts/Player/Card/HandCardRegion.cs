﻿
using DG.Tweening;
using Shun_Card_System;
using UnityEngine;


public class HandCardRegion : BaseDraggableObjectRegion
{
    private Vector3 _startPosition;
    private Vector3 _moveInteractableDestinationPosition;
    private Vector3 _middlePivotDestinationPosition;
    private Sequence _moveSequence;
    
    [Header("Enable/Disable Interactable Movement")]
    [SerializeField] private Vector3 _moveInteractableOffset = new Vector3(0,-2, 0);
    [SerializeField] private float _moveInteractableDuration = 0.25f;
    [SerializeField] private Ease _moveInteractableEase = Ease.OutCubic;
    
    [Header("Middle Align Movement")]
    [SerializeField] private float _middleAlignDuration = 0.125f;
    [SerializeField] private Ease _middleAlignEase = Ease.OutCubic;

    [Header("Audio")] 
    [SerializeField] private AudioClip _addCardSfx;
    [SerializeField] private AudioClip _dragCardSfx;

    protected override void Awake()
    {
        base.Awake();
        _startPosition = transform.localPosition;
    }

    protected override void OnSuccessfullyAddCard(BaseDraggableObject baseDraggableObject, BaseDraggableObjectHolder baseDraggableObjectHolder, int index)
    {
        
        MiddleAlign();
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(_addCardSfx);

    }

    protected override void OnSuccessfullyRemoveCard(BaseDraggableObject baseDraggableObject, BaseDraggableObjectHolder baseDraggableObjectHolder, int index)
    {
        //var characterCardGameObject = (BaseCharacterCardGameObject) baseCardGameObject;
        
        MiddleAlign();
    }

    private void MiddleAlign()
    {
        var maxOffset =  ( CardOffset * (MaxCardHold -1 ))/2;
        var currentOffset = (CardOffset * (CardHoldingCount - 1)) / 2;
        _middlePivotDestinationPosition = maxOffset - currentOffset ;
        
        
        LocalMoveToDestination(_middleAlignDuration, _middleAlignEase);
    }
    
    public override void EnableInteractable()
    {
        base.EnableInteractable();
        _moveInteractableDestinationPosition += _moveInteractableOffset;
        
        LocalMoveToDestination(_moveInteractableDuration, _moveInteractableEase);

        if (AudioManager.Instance != null)  AudioManager.Instance.PlaySFX(_dragCardSfx);
    }

    public override void DisableInteractable()
    {
        base.DisableInteractable();
        _moveInteractableDestinationPosition -= _moveInteractableOffset;

        LocalMoveToDestination(_moveInteractableDuration, _moveInteractableEase);
        
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(_dragCardSfx);
    }

    private void LocalMoveToDestination(float duration, Ease ease)
    {
        _moveSequence.Kill();
        _moveSequence = DOTween.Sequence();
        _moveSequence.Append(transform.DOLocalMove(GetMovePosition(), duration).SetEase(ease));
        _moveSequence.Play();
    }

    private Vector3 GetMovePosition()
    {
        return _startPosition + _moveInteractableDestinationPosition + _middlePivotDestinationPosition;
    }
}
