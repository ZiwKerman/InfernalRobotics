using InfernalRobotics.Module;
using InfernalRobotics.Command;
using System.Collections.Generic;

namespace InfernalRobotics.Control.Servo
{
    internal class ControlGroup : IControlGroup
    {
        private readonly ModuleIRServo rawServo;

        public ControlGroup(ModuleIRServo rawServo)
        {
            this.rawServo = rawServo;
        }

        public void AddGroup(ProtoGroup pg)
        {
            if(rawServo.servoGroups.Find(g => g._guid == pg._guid) != null)
            {
                //maybe we should update servo's stored PG with passed PG
                return;
            }
            else
            {
                rawServo.servoGroups.Add(pg);
            }
        }

        public void RemoveGroup(ProtoGroup pg)
        {
            var toRemove = rawServo.servoGroups.Find(g => g._guid == pg._guid);
            if(toRemove != null)
            {
                rawServo.servoGroups.Remove(toRemove);
            }
            else
            {
                //not found
            }
        }

        public List<ProtoGroup> AllGroups
        {
            get
            {
                return rawServo.servoGroups;
            }
        }
    }
}