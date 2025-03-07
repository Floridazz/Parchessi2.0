using System.Collections.Generic;
using Shun_Card_System;
using Shun_Utility;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Shun_Card_System
{
    public class BaseDraggableObjectMouseInput 
    {
        protected Vector3 MouseWorldPosition;
        protected RaycastHit2D[] MouseCastHits;
    
        [Header("Hover Objects")]
        protected List<IMouseHoverable> LastHoverMouseInteractableGameObjects = new();
        public bool IsHoveringCard => LastHoverMouseInteractableGameObjects.Count != 0;

        [Header("Drag Objects")]
        protected Vector3 CardOffset;
        protected BaseDraggableObject DraggingDraggable;
        protected BaseDraggableObjectRegion LastDraggableObjectRegion;
        protected BaseDraggableObjectHolder LastDraggableObjectHolder;
        protected BaseCardButton LastCardButton;

        public bool IsDraggingCard
        {
            get;
            private set;
        }

    
        public virtual void UpdateMouseInput()
        {
            UpdateMousePosition();
            CastMouse();
            if(!IsDraggingCard) UpdateHoverObject();
            
            if (Input.GetMouseButtonUp(0))
            {
                EndDragCard();
            }
            
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                StartDragCard();
            }

            if (Input.GetMouseButton(0))
            {
                DragCard();
            }

        }

        #region CAST

        protected void UpdateMousePosition()
        {
            Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            MouseWorldPosition = new Vector3(worldMousePosition.x, worldMousePosition.y, 0);
        }

        protected void CastMouse()
        {
            MouseCastHits = Physics2D.RaycastAll(MouseWorldPosition, Vector2.zero);
        }

        #endregion
    

        #region HOVER

        protected virtual void UpdateHoverObject()
        {
            var hoveringMouseInteractableGameObject = FindAllIMouseInteractableInMouseCast();

            var endHoverInteractableGameObjects = SetOperations.SetDifference(LastHoverMouseInteractableGameObjects, hoveringMouseInteractableGameObject);
            var startHoverInteractableGameObjects =  SetOperations.SetDifference(hoveringMouseInteractableGameObject, LastHoverMouseInteractableGameObjects);

            foreach (var interactable in endHoverInteractableGameObjects)
            {
                if (interactable.IsHovering) interactable.EndHover();
            }
            foreach (var interactable in startHoverInteractableGameObjects)
            {
                if (!interactable.IsHovering) interactable.StartHover();
            }

            LastHoverMouseInteractableGameObjects = hoveringMouseInteractableGameObject;
        }
    
    
        protected virtual IMouseHoverable FindFirstIMouseInteractableInMouseCast()
        {
            foreach (var hit in MouseCastHits)
            {
                var characterCardButton = hit.transform.gameObject.GetComponent<BaseCardButton>();
                if (characterCardButton != null && characterCardButton.IsHoverable)
                {
                    //Debug.Log("Mouse find "+ gameObject.name);
                    return characterCardButton;
                }
            
                var characterCardGameObject = hit.transform.gameObject.GetComponent<BaseDraggableObject>();
                if (characterCardGameObject != null && characterCardGameObject.IsDraggable)
                {
                    //Debug.Log("Mouse find "+ gameObject.name);
                    return characterCardGameObject;
                }
            }

            return null;
        }
    
        protected virtual List<IMouseHoverable> FindAllIMouseInteractableInMouseCast()
        {
            List<IMouseHoverable> mouseInteractableGameObjects = new(); 
            foreach (var hit in MouseCastHits)
            {
                var characterCardButton = hit.transform.gameObject.GetComponent<BaseCardButton>();
                if (characterCardButton != null && characterCardButton.IsHoverable)
                {
                    mouseInteractableGameObjects.Add(characterCardButton);
                }
            
                var characterCardGameObject = hit.transform.gameObject.GetComponent<BaseDraggableObject>();
                if (characterCardGameObject != null && characterCardGameObject.IsDraggable)
                {
                    //Debug.Log("Mouse find "+ gameObject.name);
                    mouseInteractableGameObjects.Add(characterCardGameObject);
                }
            }

            return mouseInteractableGameObjects;
        }
    
        #endregion
    
    
        protected TResult FindFirstInMouseCast<TResult>()
        {
            foreach (var hit in MouseCastHits)
            {
                var result = hit.transform.gameObject.GetComponent<TResult>();
                if (result != null)
                {
                    //Debug.Log("Mouse find "+ gameObject.name);
                    return result;
                }
            }

            //Debug.Log("Mouse cannot find "+ typeof(TResult));
            return default;
        }

    
        protected bool StartDragCard()
        {
            // Check for button first
            LastCardButton = FindFirstInMouseCast<BaseCardButton>();

            if (LastCardButton != null && LastCardButton.IsHoverable)
            {
                LastCardButton.Select();
                return true;
            } 

            // Check for card game object second
            DraggingDraggable = FindFirstInMouseCast<BaseDraggableObject>();

            if (DraggingDraggable == null || !DraggingDraggable.IsDraggable || !DetachCardToHolder())
            {
                DraggingDraggable = null;
                return false;
            }
            
            // Successfully detach card
            CardOffset = DraggingDraggable.transform.position - MouseWorldPosition;
            IsDraggingCard = true;

            DraggingDraggable.StartDrag();
    
            return true;
        
        }

        protected void DragCard()
        {
            if (!IsDraggingCard) return; 
        
            DraggingDraggable.transform.position = MouseWorldPosition + CardOffset;
        
        }

        protected void EndDragCard()
        {
            if (!IsDraggingCard) return;
        
            DraggingDraggable.EndDrag();
            AttachCardToHolder();

            DraggingDraggable = null;
            LastDraggableObjectHolder = null;
            LastDraggableObjectRegion = null;
            IsDraggingCard = false;

        }
    
        protected virtual bool DetachCardToHolder()
        {
            // Check the card region base on card game object or card holder, to TakeOutTemporary
            LastDraggableObjectRegion = FindFirstInMouseCast<BaseDraggableObjectRegion>();
            if (LastDraggableObjectRegion == null)
            {
                LastDraggableObjectHolder = FindFirstInMouseCast<BaseDraggableObjectHolder>();
                if (LastDraggableObjectHolder == null)
                {
                    return true;
                }

                LastDraggableObjectRegion = LastDraggableObjectHolder.DraggableObjectRegion;
            }
            else
            {
                LastDraggableObjectHolder = LastDraggableObjectRegion.FindCardPlaceHolder(DraggingDraggable);
            }

            // Having got the region and holder, take the card out temporary
            if (LastDraggableObjectRegion.TakeOutTemporary(DraggingDraggable, LastDraggableObjectHolder)) return true;
        
            LastDraggableObjectHolder = null;
            LastDraggableObjectRegion = null;

            return false;

        }

        protected void AttachCardToHolder()
        {
        
            var dropRegion = FindFirstInMouseCast<BaseDraggableObjectRegion>();
            var dropHolder = FindFirstInMouseCast<BaseDraggableObjectHolder>();
        
            if (dropHolder == null)
            {
                if (dropRegion != null && dropRegion != LastDraggableObjectRegion &&
                    dropRegion.TryAddCard(DraggingDraggable, dropHolder)) // Successfully add to the drop region
                {
                    if (LastDraggableObjectHolder != null) // remove the temporary in last region
                    {
                        LastDraggableObjectRegion.RemoveTemporary(DraggingDraggable);
                        return;
                    }
                }
            
                if (LastDraggableObjectRegion != null) // Unsuccessfully add to drop region or it is the same region
                    LastDraggableObjectRegion.ReAddTemporary(DraggingDraggable);
            }
            else
            {
                if (dropRegion == null) 
                    dropRegion = dropHolder.DraggableObjectRegion;
                
                if (dropRegion == null) // No region to drop anyway
                {
                    if(LastDraggableObjectRegion != null) LastDraggableObjectRegion.ReAddTemporary(DraggingDraggable);
                }

                if (dropRegion.CardMiddleInsertionStyle == BaseDraggableObjectRegion.MiddleInsertionStyle.Swap)
                {
                    var targetCard = dropHolder.DraggableObject;
                    if (targetCard != null && LastDraggableObjectRegion != null && dropRegion.TakeOutTemporary(targetCard, dropHolder))
                    {
                        LastDraggableObjectRegion.ReAddTemporary(targetCard);
                        dropRegion.ReAddTemporary(DraggingDraggable);
                        
                        return;
                    }
                    
                }
                
                if (!dropRegion.TryAddCard(DraggingDraggable, dropHolder))
                {
                    if(LastDraggableObjectRegion != null) LastDraggableObjectRegion.ReAddTemporary(DraggingDraggable);
                }
                
                if (LastDraggableObjectHolder != null)
                {
                    LastDraggableObjectRegion.RemoveTemporary(DraggingDraggable);
                }

            }

        }
    
    }
}
