﻿using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Shun_Card_System
{
    [RequireComponent(typeof(Collider2D))]
    public class BaseDraggableObjectRegion : MonoBehaviour, IMouseHoverable
    {
        public enum MiddleInsertionStyle
        {
            AlwaysBack,
            InsertInMiddle,
            Cannot,
            Swap,
        }
        [SerializeField] protected BaseDraggableObjectHolder DraggableObjectHolderPrefab;
        [SerializeField] protected Transform SpawnPlace;
        [SerializeField] protected Vector3 CardOffset = new Vector3(5f, 0 ,0);

        
        [SerializeField] protected List<BaseDraggableObjectHolder> _cardPlaceHolders = new();
        [SerializeField] protected int MaxCardHold;
        public MiddleInsertionStyle CardMiddleInsertionStyle = MiddleInsertionStyle.InsertInMiddle;
        protected BaseDraggableObjectHolder TemporaryBaseDraggableObjectHolder;
        public int CardHoldingCount { get; private set; } 
        
        
        [SerializeField]
        private bool _interactable;
        public bool IsHoverable { get => _interactable; protected set => _interactable = value;}
        public bool IsHovering { get; protected set; }

        #region INITIALIZE

        protected virtual void Awake()
        {
            InitializeCardPlaceHolder();
        }

        protected void InitializeCardPlaceHolder()
        {
            if (_cardPlaceHolders.Count != 0)
            {
                MaxCardHold = _cardPlaceHolders.Count;
                for (int i = 0; i < MaxCardHold; i++)
                {
                    _cardPlaceHolders[i].InitializeRegion(this, i);
                }
            }
            else
            {
                for (int i = 0; i < MaxCardHold; i++)
                {
                    var cardPlaceHolder = Instantiate(DraggableObjectHolderPrefab, SpawnPlace.position + i * CardOffset,
                        Quaternion.identity, SpawnPlace);
                    _cardPlaceHolders.Add(cardPlaceHolder);
                    cardPlaceHolder.InitializeRegion(this, i);
                }    
            }
            
        }
        
        #endregion

        #region OPERATION

        
        public List<BaseDraggableObject> GetAllCardGameObjects(bool getNull = false)
        {
            List<BaseDraggableObject> result = new();
            for (int i = 0; i < CardHoldingCount; i++)
            {
                if ((!getNull && _cardPlaceHolders[i].DraggableObject != null) || getNull) result.Add(_cardPlaceHolders[i].DraggableObject);
            }

            return result;
        }

        public void DestroyAllCardGameObject()
        {
            foreach (var cardHolder in _cardPlaceHolders)
            {
                if (cardHolder.DraggableObject == null) continue;
                Destroy(cardHolder.DraggableObject.gameObject);
                cardHolder.DraggableObject = null;
            }
            
            CardHoldingCount = 0;
        }

        protected BaseDraggableObjectHolder FindEmptyCardPlaceHolder()
        {
            if (CardHoldingCount >= MaxCardHold) return null;
            return _cardPlaceHolders[CardHoldingCount];
        }
        
        public BaseDraggableObjectHolder FindCardPlaceHolder(BaseDraggableObject baseDraggableObject)
        {
            foreach (var cardPlaceHolder in _cardPlaceHolders)
            {
                if (cardPlaceHolder.DraggableObject == baseDraggableObject) return cardPlaceHolder;
            }

            return null;
        }

        public bool AddCard(BaseDraggableObject draggableObject, BaseDraggableObjectHolder draggableObjectHolder = null)
        {
            if ( draggableObjectHolder == null || draggableObjectHolder.IndexInRegion >= CardHoldingCount)
            {
                return AddCardAtBack(draggableObject);
            }

            return CardMiddleInsertionStyle switch
            {
                MiddleInsertionStyle.AlwaysBack => AddCardAtBack(draggableObject),
                MiddleInsertionStyle.InsertInMiddle => AddCardAtMiddle(draggableObject, draggableObjectHolder.IndexInRegion),
                MiddleInsertionStyle.Cannot => false,
                _ => false
            };
        }

        private bool AddCardAtBack(BaseDraggableObject draggableObject)
        {
            if (CardHoldingCount >= MaxCardHold)
            {
                return false;
            }

            var index = CardHoldingCount;
            var cardPlaceHolder = _cardPlaceHolders[index];
            cardPlaceHolder.AttachCardGameObject(draggableObject);
            
            CardHoldingCount ++;
            
            OnSuccessfullyAddCard(draggableObject, cardPlaceHolder, index);
                
            return true;
        }
        
        private  bool AddCardAtMiddle(BaseDraggableObject draggableObject, int index)
        {
            if (CardHoldingCount >= MaxCardHold)
            {
                return false;
            }
            
            ShiftRight(index);

            var cardPlaceHolder = _cardPlaceHolders[index];
            cardPlaceHolder.AttachCardGameObject(draggableObject);
            
            CardHoldingCount++;
            
            OnSuccessfullyAddCard(draggableObject, cardPlaceHolder, index);
            
            return true;
        }
        
        
        protected virtual void ShiftRight(int startIndex)
        {
            for (int i = _cardPlaceHolders.Count - 1; i > startIndex; i--)
            {
                var card = _cardPlaceHolders[i - 1].DetachCardGameObject();
                
                if (card == null) continue;
                _cardPlaceHolders[i].AttachCardGameObject(card);
                
                //SmoothMove(card.transform, _cardPlaceHolders[i].transform.position);

            }
        }
        
        
        protected virtual void ShiftLeft(int startIndex)
        {
            for (int i = startIndex; i < _cardPlaceHolders.Count - 1; i++)
            {
                var card = _cardPlaceHolders[i + 1].DetachCardGameObject();
                
                if (card == null) continue;
                
                _cardPlaceHolders[i].AttachCardGameObject(card);
                
                
                //SmoothMove(card.transform, _cardPlaceHolders[i].transform.position);

            }
        }
        
        public virtual bool RemoveCard(BaseDraggableObject draggableObject)
        {

            for (int i = 0; i < _cardPlaceHolders.Count; i++)
            {
                if (_cardPlaceHolders[i].DraggableObject != draggableObject) continue;
                _cardPlaceHolders[i].DetachCardGameObject();
                
                ShiftLeft(i);
                CardHoldingCount--;
                
                OnSuccessfullyRemoveCard(draggableObject, _cardPlaceHolders[i], i);
                return true;
            }
            return false;
        }
        
        public virtual bool RemoveCard(BaseDraggableObject draggableObject,BaseDraggableObjectHolder draggableObjectHolder)
        {
            if (draggableObjectHolder.DraggableObject != draggableObject) return false;

            draggableObjectHolder.DetachCardGameObject();

            var index = _cardPlaceHolders.IndexOf(draggableObjectHolder);
            ShiftLeft(index);
            CardHoldingCount--;

            OnSuccessfullyRemoveCard(draggableObject, draggableObjectHolder, index);
            return true;
        }
        
        
        #endregion

        #region MOUSE_INPUT
        
        public virtual bool TryAddCard(BaseDraggableObject draggableObject, BaseDraggableObjectHolder draggableObjectHolder = null)
        {
            if (!IsHoverable) return false;
            return AddCard(draggableObject, draggableObjectHolder);
        }
        
        public virtual bool TakeOutTemporary(BaseDraggableObject draggableObject,BaseDraggableObjectHolder draggableObjectHolder)
        {
            if (!IsHoverable) return false;

            if (!RemoveCard(draggableObject, draggableObjectHolder)) return false;
            
            TemporaryBaseDraggableObjectHolder = draggableObjectHolder;
            return true;
        }
        
        public virtual void ReAddTemporary(BaseDraggableObject baseDraggableObject)
        {
            AddCard(baseDraggableObject, TemporaryBaseDraggableObjectHolder);
            
            TemporaryBaseDraggableObjectHolder = null;
        }

        public virtual void RemoveTemporary(BaseDraggableObject baseDraggableObject)
        {
            TemporaryBaseDraggableObjectHolder = null;
        }
        
        
        #endregion

        protected virtual void SmoothMove(Transform movingObject, Vector3 toPosition)
        {
            movingObject.position = toPosition;
        }

        protected virtual void OnSuccessfullyAddCard(BaseDraggableObject baseDraggableObject, BaseDraggableObjectHolder baseDraggableObjectHolder, int index)
        {
            
        }
        protected virtual void OnSuccessfullyRemoveCard(BaseDraggableObject baseDraggableObject, BaseDraggableObjectHolder baseDraggableObjectHolder, int index)
        {
            
        }

        public void Select()
        {
            throw new NotImplementedException();
        }

        public void Deselect()
        {
            throw new NotImplementedException();
        }

        public void StartHover()
        {
            IsHovering = true;
            
        }

        public void EndHover()
        {
            IsHovering = false;
            
        }

        public virtual void DisableInteractable()
        {
            
            if (!IsHoverable) return;
            IsHoverable = false;
            if(IsHovering) EndHover();
        }
        
        public virtual void EnableInteractable()
        {
            if (IsHoverable) return;
            IsHoverable = true;
        }
    }
}