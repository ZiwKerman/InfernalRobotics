using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.EventSystems;
using System;
using InfernalRobotics.Command;

namespace InfernalRobotics.Gui
{
    public class ServoDropHandler : MonoBehaviour, IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
            var dropedObject = eventData.pointerDrag;
            Debug.Log("Servo OnDrop: " + dropedObject.name);

            var dragHandler = dropedObject.GetComponent<ServoDragHandler>();
            
            if(dragHandler == null)
            {
                Logger.Log("[ServoDropHandler]: dropped object missing ServoDragHandler", Logger.Level.Debug);
                return;
            }
        }

        public void onServoDrop(ServoDragHandler dragHandler)
        {
            //dragged item might be a copy

            var servoUIControls = dragHandler.draggedItem;
            var newGroupUIControls = dragHandler.dropZone.parent.gameObject;

            ServoGroup newGroup = null;
            foreach(var pair in WindowManager._servoGroupUIControls)
            {
                if(pair.Value == newGroupUIControls)
                {
                    newGroup = pair.Key;
                    break;
                }

            }

            if(newGroup == null || dragHandler.originalGroup == null || dragHandler.servo == null)
            {
                //error
                Logger.Log("[ServoDropHandler]: Null checks failed. " 
                    + (newGroup == null ? "newGroup is null." : "newGroup.name = " + newGroup.name)
                    + (dragHandler.originalGroup == null ? "originalGroup is null." : "originalGroup.name = " + dragHandler.originalGroup.name)
                    + (dragHandler.servo == null ? "servo is null." : "servo.name = " + dragHandler.servo.Name) , Logger.Level.Debug);
                return;
            }

            if(newGroup == dragHandler.originalGroup)
            {
                //nothing changed
                return;
            }

            if(dragHandler.isCopy)
            {
                ServoController.AddServoToGroup(newGroup, dragHandler.servo);
            }
            else
            {
                ServoController.ChangeServoGroup(dragHandler.originalGroup, newGroup, dragHandler.servo);
            }

        }

    }

}