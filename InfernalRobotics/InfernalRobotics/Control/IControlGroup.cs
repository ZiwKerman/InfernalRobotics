using System.Collections.Generic;
using InfernalRobotics.Command;

namespace InfernalRobotics.Control
{
    public interface IControlGroup    
    {
        List<ProtoGroup> AllGroups { get; }

        void AddGroup(ProtoGroup pg);

        void RemoveGroup(ProtoGroup pg);

    }
}