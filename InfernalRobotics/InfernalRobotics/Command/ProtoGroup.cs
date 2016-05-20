using System;
using InfernalRobotics.Module;
using InfernalRobotics.Control;
using System.Collections.Generic;
using System.Linq;

namespace InfernalRobotics.Command
{
    /// <summary>
    /// Proto group meant to be serialized in each ModuleIRServo.
    /// Our ControlGroup will be derived from ProtoGroup
    /// </summary>
    public class ProtoGroup
    {
        internal Guid _guid;
        public string groupID {
            get { return _guid.ToString();}
        }

        internal string name = "New Group";

        internal float speedMultipler = 1.0f;
        internal string forwardKey = "";
        internal string reverseKey = "";

        public bool Expanded = false;

        public ProtoGroup()
        {
            _guid = Guid.NewGuid();

        }

        public ProtoGroup(string guid)
        {
            _guid = guid != "" ? new Guid(guid) : Guid.NewGuid();

        }

        public ConfigNode Serialize()
        {
            var retVal = new ConfigNode("ProtoGroup");

            retVal.AddValue("groupID", groupID);
            retVal.AddValue("name", name);
            retVal.AddValue("speedMultiplier", speedMultipler);
            retVal.AddValue("forwardKey", forwardKey);
            retVal.AddValue("reverseKey", reverseKey);
            retVal.AddValue("Expanded", Expanded);

            return retVal;
        }

        public static ProtoGroup LoadFromNode(ConfigNode node)
        {
            var guid = node.GetValue("groupID");

            var retVal = new ProtoGroup(guid);

            retVal.name = node.GetValue("name");
            retVal.forwardKey = node.GetValue("forwardKey");
            retVal.reverseKey = node.GetValue("reverseKey");

            node.TryGetValue("speedMultiplier", ref retVal.speedMultipler);
            node.TryGetValue("Expanded", ref retVal.Expanded);

            return retVal;
        }
    }
}

