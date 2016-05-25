using System;
using InfernalRobotics.Module;
using InfernalRobotics.Control;
using System.Collections.Generic;
using System.Linq;

namespace InfernalRobotics.Command
{
    public class ServoGroup : ProtoGroup
    {
        private readonly List<IServo> servos;
        private readonly Vessel vessel;

        private bool stale;
        private float totalElectricChargeRequirement;

        public ServoGroup(IServo s) : base()
        {
            servos = new List<IServo>();
            servos.Add(s);
            vessel = s.RawServo.vessel;
        }

        public ServoGroup() : base()
        {
            
        }

        public ServoGroup (ProtoGroup pg)
        {
            this._guid = pg._guid;
            this.name = pg.name;
            this.forwardKey = pg.forwardKey;
            this.reverseKey = pg.reverseKey;
            this.Expanded = pg.Expanded;
            this.speedMultipler = pg.speedMultipler;

            this.servos = new List<IServo>();

        }

        public IList<IServo> Servos
        {
            get { return servos; }
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                //we also need to update protogroups on all siblings
                UpdateProtoSiblings();
            }
        }

        public string ForwardKey
        {
            get { return forwardKey; }
            set
            {
                forwardKey = value;
                //we also need to update protogroups on all siblings
                UpdateProtoSiblings();
            }
        }

        public string ReverseKey
        {
            get { return reverseKey; }
            set
            {
                reverseKey = value;
                //we also need to update protogroups on all siblings
                UpdateProtoSiblings();
            }
        }

        public float Speed
        {
            get { return speedMultipler; }
            set
            {
                speedMultipler = value;
                //we also need to update protogroups on all siblings
                UpdateProtoSiblings();
            }
        }

        public void UpdateProtoSiblings()
        {
            //we also need to update protogroups on all siblings
            if(servos != null)
            {
                foreach(var s in servos)
                {
                    var pg = s.Group.AllGroups.Find(g => g._guid == this._guid);
                    if (pg != null)
                    {
                        pg.name = this.name;
                        pg.forwardKey = this.forwardKey;
                        pg.reverseKey = this.reverseKey;
                        pg.speedMultipler = this.speedMultipler;
                        pg.Expanded = this.Expanded;
                    }
                        
                }
            }
        }

        public Vessel Vessel
        {
            get { return vessel; }
        }

        public bool MovingPositive = false;
        public bool MovingNegative = false;

        public void MoveRight()
        {
            if (Servos.Any())
            {
                foreach (var servo in Servos)
                {
                    servo.Motor.MoveRight();
                }
            }
        }

        public void MoveLeft()
        {
            if (Servos.Any())
            {
                foreach (var servo in Servos)
                {
                    servo.Motor.MoveLeft();
                }
            }
        }

        public void MoveCenter()
        {
            if (Servos.Any())
            {
                foreach (var servo in Servos)
                {
                    servo.Motor.MoveCenter();
                }
            }
        }

        public void MoveNextPreset()
        {
            if (Servos.Any())
            {
                foreach (var servo in Servos)
                {
                    servo.Preset.MoveNext();
                }
            }
        }

        public void MovePrevPreset()
        {
            if (Servos.Any())
            {
                foreach (var servo in Servos)
                {
                    servo.Preset.MovePrev();
                }
            }
        }

        public void Stop()
        {
            if (Servos.Any())
            {
                foreach (var servo in Servos)
                {
                    servo.Motor.Stop();
                }
            }
        }

        public float TotalElectricChargeRequirement
        {
            get
            {
                if (stale) Freshen();
                return totalElectricChargeRequirement;
            }
        }

        private void Freshen()
        {
            if (Servos == null) return;

            float chargeRequired = Servos.Where (s => s.Mechanism.IsFreeMoving == false).Sum (s => s.ElectricChargeRequired);
            totalElectricChargeRequirement = chargeRequired;
            stale = false;
        }

    }
}
