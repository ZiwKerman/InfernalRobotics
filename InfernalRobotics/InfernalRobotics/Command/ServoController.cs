using System;
using InfernalRobotics.Control;
using InfernalRobotics.Control.Servo;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace InfernalRobotics.Command
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ServoControllerFlight : ServoController
    {
        public override string AddonName { get { return this.name; } }
    }

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class ServoControllerEditor : ServoController
    {
        public override string AddonName { get { return this.name; } }
    }

    public class ServoController : MonoBehaviour
    {
        public virtual String AddonName { get; set; }

        protected static bool UseElectricCharge = true;
        protected static ServoController ControllerInstance;

        public List<IServo> allServos;
        public List<ServoGroup> ServoGroups;

        private int partCounter;
        private int loadedVesselCounter = 0;

        public static ServoController Instance { get { return ControllerInstance; } }

        public static bool APIReady { get { return ControllerInstance != null && ControllerInstance.ServoGroups != null && ControllerInstance.ServoGroups.Count > 0; } }
        
        static ServoController()
        {
        }

        public static void ChangeServoGroup(ServoGroup removeFrom, ServoGroup addTo, IServo servo)
        {
            RemoveServoFromGroup(removeFrom, servo);
            AddServoToGroup(addTo, servo);
        }

        public static void RemoveServoFromGroup(ServoGroup removeFrom, IServo servo)
        {
            if (Instance == null || Instance.ServoGroups == null)
                return;

            var f = Instance.ServoGroups.Find(sg => sg._guid == removeFrom._guid);

            f.Servos.Remove(servo);

            servo.Group.RemoveGroup((ProtoGroup) removeFrom);
        }

        public static void AddServoToGroup(ServoGroup addTo, IServo servo)
        {
            if (Instance == null || Instance.ServoGroups == null)
                return;

            var t = Instance.ServoGroups.Find(sg => sg._guid == addTo._guid);

            t.Servos.Add(servo);

            servo.Group.AddGroup((ProtoGroup) addTo);
        }


        private void OnPartAttach(GameEvents.HostTargetAction<Part, Part> hostTarget)
        {
            
            var servos = hostTarget.host.GetChildServos();

            if (ServoGroups == null)
                ServoGroups = new List<ServoGroup>();

            foreach(var s in servos)
            {
                if (allServos.Contains(s))
                {
                    //we already know about this servo somehow, might be duplicate
                    //throw an error
                    Logger.Log("[ServoController] OnPartAttach, we already know about this servo: " + s.Name, Logger.Level.Debug);
                }
                else
                {
                    foreach (var g in s.Group.AllGroups)
                    {
                        var newSG = ServoGroups.Find(sg => sg._guid == g._guid);
                        if(newSG != null)
                        {
                            if(!newSG.Servos.Contains(s))
                                newSG.Servos.Add(s);
                        }
                        else
                        {
                            newSG = (ServoGroup) g; //cast ProtoGroup into ServoGroup
                            ServoGroups.Add(newSG);
                        }

                        allServos.Add(s);
                    }
                }
            }

            if (ServoGroups.Count == 0)
                ServoGroups = null;
            
            partCounter = EditorLogic.fetch.ship.parts.Count == 1 ? 0 : EditorLogic.fetch.ship.parts.Count;

            Logger.Log("[ServoController] OnPartAttach finished successfully", Logger.Level.Debug);
        }

        private void OnPartRemove(GameEvents.HostTargetAction<Part, Part> hostTarget)
        {
            Part part = hostTarget.target;

            if (ServoGroups == null)
                ServoGroups = new List<ServoGroup>();
            
            try
            {
                var servos = part.ToServos();
                foreach (var temp in servos)
                {
                    temp.Mechanism.Reset();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("[ServoController] OnPartRemove Error: " + ex, Logger.Level.Debug);
            }

            foreach (var p in part.GetChildServos())
            {
                allServos.Remove(p);
                foreach(var pg in p.Group.AllGroups)
                {
                    var toRemove = ServoGroups.Find(sg => sg._guid == pg._guid);
                    if (toRemove != null)
                        ServoGroups.Remove(toRemove);
                }
            }

            if (ServoGroups.Count == 0)
                ServoGroups = null;

            partCounter = EditorLogic.fetch.ship.parts.Count == 1 ? 0 : EditorLogic.fetch.ship.parts.Count;

            Logger.Log("[ServoController] OnPartRemove finished successfully", Logger.Level.Debug);
        }

        private void RebuildServoGroupsEditor(ShipConstruct ship = null)
        {
            if(ship==null)
                ship = EditorLogic.fetch.ship;

            ServoGroups = new List<ServoGroup>();
            allServos = new List<IServo>();

            foreach (Part p in ship.Parts)
            {
                foreach (var s in p.ToServos())
                {
                    if(allServos.Contains(s))
                    {
                        //we already know about this servo somehow
                    }
                    else
                    {
                        foreach (var g in s.Group.AllGroups)
                        {
                            var newSG = ServoGroups.Find(sg => sg._guid == g._guid);
                            if(newSG != null)
                            {
                                if(!newSG.Servos.Contains(s))
                                    newSG.Servos.Add(s);
                            }
                            else
                            {
                                newSG = (ServoGroup) g; //cast ProtoGroup into ServoGroup
                                ServoGroups.Add(newSG);
                            }

                            allServos.Add(s);
                        }

                    }
                }
            }

        }
       
        private void OnEditorShipModified(ShipConstruct ship)
        {
            if (EditorLogic.fetch.ship.parts.Count != partCounter)
            {
                RebuildServoGroupsEditor(ship);

                Gui.WindowManager.guiRebuildPending = true; //this should force an UI rebuild on first update

                if(Gui.IRBuildAid.IRBuildAidManager.Instance)
                    Gui.IRBuildAid.IRBuildAidManager.Reset();
                
            }
                
            partCounter = EditorLogic.fetch.ship.parts.Count == 1 ? 0 : EditorLogic.fetch.ship.parts.Count;
            Logger.Log("[ServoController] OnEditorShipModified finished successfully", Logger.Level.Debug);
        }

        private void OnEditorRestart()
        {
            ServoGroups = null;

            Gui.WindowManager.guiRebuildPending = true; //this should force an UI rebuild on first update

            if (Gui.IRBuildAid.IRBuildAidManager.Instance)
                Gui.IRBuildAid.IRBuildAidManager.Reset();

            Logger.Log ("OnEditorRestart called", Logger.Level.Debug);
        }

        private void OnEditorLoad(ShipConstruct s, KSP.UI.Screens.CraftBrowserDialog.LoadType t)
        {
            OnEditorShipModified (s);
            
            Logger.Log ("OnEditorLoad called", Logger.Level.Debug);
        }
        /// <summary>
        /// Rebuilds the servo groups. Only works in flight.
        /// </summary>
        private void RebuildServoGroupsFlight()
        {
            ServoGroups = new List<ServoGroup>();

            for(int i=0; i<FlightGlobals.Vessels.Count; i++)
            {
                var vessel = FlightGlobals.Vessels [i];

                if (!vessel.loaded)
                    continue;
                
            }

            if (ServoGroups.Count == 0)
                ServoGroups = null;

            Gui.WindowManager.guiRebuildPending = true; //this should force an UI rebuild on the next update

        }

        private void OnVesselChange(Vessel v)
        {
            Logger.Log(string.Format("[ServoController] vessel {0}", v.name));

            RebuildServoGroupsFlight ();

            foreach (var servo in v.ToServos())
            {
                servo.RawServo.SetupJoints();
            }

            Logger.Log("[ServoController] OnVesselChange finished successfully", Logger.Level.Debug);
        }

        private void OnVesselWasModified(Vessel v)
        {
            RebuildServoGroupsFlight ();
        }

        private void OnVesselLoaded (Vessel v)
        {
            Logger.Log("[ServoController] OnVesselLoaded, v=" + v.GetName(), Logger.Level.SuperVerbose);
            RebuildServoGroupsFlight ();
        }

        private void OnVesselUnloaded (Vessel v)
        {
            Logger.Log("[ServoController] OnVesselUnloaded, v=" + v.GetName(), Logger.Level.SuperVerbose);
            RebuildServoGroupsFlight ();
        }

        private void Awake()
        {
            Logger.Log("[ServoController] awake, AddonName = " + this.AddonName);

            GameScenes scene = HighLogic.LoadedScene;

            if (scene == GameScenes.FLIGHT)
            {
                GameEvents.onVesselChange.Add(OnVesselChange);
                GameEvents.onVesselWasModified.Add(OnVesselWasModified);
                GameEvents.onVesselLoaded.Add (OnVesselLoaded);
                GameEvents.onVesselDestroy.Add (OnVesselUnloaded);
                GameEvents.onVesselGoOnRails.Add (OnVesselUnloaded);
                ControllerInstance = this;
            }
            else if (scene == GameScenes.EDITOR)
            {
                GameEvents.onPartAttach.Add(OnPartAttach);
                GameEvents.onPartRemove.Add(OnPartRemove);
                GameEvents.onEditorShipModified.Add(OnEditorShipModified);
                GameEvents.onEditorLoad.Add(OnEditorLoad);
                GameEvents.onEditorRestart.Add(OnEditorRestart);
                ControllerInstance = this;
            }
            else
            {
                ControllerInstance = null;
            }

            Logger.Log("[ServoController] awake finished successfully, AddonName = " + this.AddonName, Logger.Level.Debug);
        }

        /// <summary>
        /// Sets the wheel auto-struting for the Vessel v. 
        /// In flight mode we need to set to false before moving 
        /// the joint and to true aferwards
        /// </summary>
        public static void SetWheelAutoStruts(bool value, Vessel v)
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            var activeVesselWheels = v.FindPartModulesImplementing<ModuleWheelBase>();
            foreach(var mwb in activeVesselWheels)
            {
                if (value)
                {
                    if(!mwb.autoStrut) //we only need to Cycle once
                        mwb.CycleWheelStrut();
                }
                else
                    mwb.ReleaseWheelStrut();

                mwb.autoStrut = value;

            }
        }


        private void FixedUpdate()
        {
            //because OnVesselDestroy and OnVesselGoOnRails seem to only work for active vessel I had to build this stupid workaround
            if(HighLogic.LoadedSceneIsFlight)
            {
                if(FlightGlobals.Vessels.Count(v => v.loaded) != loadedVesselCounter)
                {
                    RebuildServoGroupsFlight ();
                    loadedVesselCounter = FlightGlobals.Vessels.Count(v => v.loaded);
                }

                if (ServoGroups == null)
                    return;

                //check if all servos stopped running and enable the struts, otherwise disable wheel autostruts
                var anyActive = new Dictionary<Vessel, bool>();

                foreach(var g in ServoGroups)
                {
                    if (!anyActive.ContainsKey(g.Vessel))
                        anyActive.Add(g.Vessel, false);
                    
                    foreach(var s in g.Servos)
                    {
                        if (s.RawServo.Interpolator.Active)
                        {
                            anyActive[g.Vessel] = true;
                            break;
                        }
                    }
                }
                foreach(var pair in anyActive)
                {
                    SetWheelAutoStruts(!pair.Value, pair.Key);
                }
            }
        }

        private void OnDestroy()
        {
            Logger.Log("[ServoController] destroy", Logger.Level.Debug);

            GameEvents.onVesselChange.Remove(OnVesselChange);
            GameEvents.onPartAttach.Remove(OnPartAttach);
            GameEvents.onPartRemove.Remove(OnPartRemove);
            GameEvents.onVesselWasModified.Remove(OnVesselWasModified);
            GameEvents.onEditorShipModified.Remove(OnEditorShipModified);
            GameEvents.onEditorLoad.Remove(OnEditorLoad);
            GameEvents.onEditorRestart.Remove(OnEditorRestart);

            GameEvents.onVesselLoaded.Remove (OnVesselLoaded);
            GameEvents.onVesselDestroy.Remove (OnVesselUnloaded);
            GameEvents.onVesselGoOnRails.Remove (OnVesselUnloaded);
            Logger.Log("[ServoController] OnDestroy finished successfully", Logger.Level.Debug);
        }

        //TODO: move this to a separate file and extend if necessary

    }
}
