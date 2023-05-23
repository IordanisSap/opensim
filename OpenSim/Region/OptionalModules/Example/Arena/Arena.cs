using Mono.Addins;

using System;
using System.Reflection;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;
using log4net;
using Nini.Config;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using OpenSim.Framework;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ArenaModule
{
    [Extension(Path = "/OpenSim/RegionModules", NodeName = "RegionModule", Id = "ArenaModule")]

    public class ArenaModule : INonSharedRegionModule
    {

        Dictionary<UUID, UUID> m_Players_NPCs = new Dictionary<UUID, UUID>();
        private UUID controller = UUID.Zero;
        private string controllerName = "ArenaController";
        private struct StartPoint
        {
            public SceneObjectPart self;
            public Scene scene;
            static int LSLText = 10;

            private const int MAX_AVATAR_NUM = 10;
            private List<UUID> avatarUUIDs;
            public List<UUID> npcUUIDs;
            public StartPoint(SceneObjectPart sc)
            {
                this.self = sc;
                this.scene = sc.ParentGroup.Scene;
                avatarUUIDs = new List<UUID>();
                npcUUIDs = new List<UUID>();

            }
            public UUID getUUID()
            {
                return self.UUID;
            }

            public string getName()
            {
                return self.Name;
            }
            public Vector3 getPos()
            {
                return self.AbsolutePosition;
            }
            public void addAvatar(UUID avatar)
            {
                avatarUUIDs.Add(avatar);
                m_log.WarnFormat("[ArenaMod] avatar {0} added to point {1}, avatars: {2}", scene.GetScenePresence(avatar).Name, getName(), avatarUUIDs.Count);
            }
            public List<UUID> getAvatars()
            {
                return avatarUUIDs;
            }
            public bool canAddNPC()
            {
                if (avatarUUIDs.Count > npcUUIDs.Count)
                {
                    return true;
                }
                m_log.WarnFormat("[ArenaMod] NPCs are full {0} - {1}", avatarUUIDs.Count, npcUUIDs.Count);
                return false;
            }
            public void addNPC(UUID npc)
            {
                if (!canAddNPC()) return;
                for (int i = 0; i < avatarUUIDs.Count; i++)
                {
                    npcUUIDs.Add(npc);
                    m_log.WarnFormat("[ArenaMod] NPC {0} added to point {1}", npc, getName());
                }
                printAvatars();
            }
            public List<UUID> getNPCs()
            {
                return npcUUIDs;
            }
            public void printAvatars()
            {
                foreach (UUID avatarUUID in avatarUUIDs)
                {
                    if (scene.GetScenePresence(avatarUUID) == null)
                    {
                        m_log.WarnFormat("[ArenaMod] avatar {0} in point {1} has been deleted", avatarUUID, getName());
                        continue;
                    }
                    m_log.WarnFormat("[ArenaMod] avatar {0} with UUID {1} in point {2}", scene.GetScenePresence(avatarUUID).Name, avatarUUID, getName());
                }
            }
            public void start()
            {
                foreach (UUID avatarUUID in avatarUUIDs)
                {
                    ScenePresence avatar = scene.GetScenePresence(avatarUUID);
                    if (avatar == null)
                    {
                        m_log.WarnFormat("[ArenaMod] avatar {0} in point {1} has been deleted", avatarUUID, getName());
                        continue;
                    }
                    m_log.WarnFormat("[ArenaMod] avatar {0} teleported to point {1}", avatar.Name, getName());
                    //scene.RequestLocalTeleport(scene.GetScenePresence(avatarUUID), getPos(),  new Vector3(0,0,0), new Vector3(0,0,0), 0);
                    avatar.RotateToLookAt(new Vector3(128, 128, avatar.AbsolutePosition.Z));
                    avatar.Teleport(getPos());
                }
            }
            public void reset()
            {
                avatarUUIDs.Clear();
                npcUUIDs.Clear();
            }
        }

        private static readonly ILog m_log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IConfig m_config = null;
        private bool m_enabled = true;
        private Scene m_scene = null;

        private IScriptModuleComms m_comms;

        private IWorldComm m_worldComm;

        private Dictionary<string, int> m_ArenaModePlayerNum = new Dictionary<string, int>();

        private string currentMode = "PVE";

        private bool arenaReady = false;
        private bool started = false;
        private UUID landOwnerUUID = UUID.Zero;


        private string[] startPointNames = new string[] { "StartPoint1", "StartPoint2" };

        private const int maxStartPoints = 2;
        private List<StartPoint> avatarStartPoints;

        private int commChannel = -5;
        #region IRegionModule Members

        public bool isOwner(UUID avatarID)
        {
            return avatarID == landOwnerUUID;
        }
        public string Name
        {
            get
            { //return this.GetType().Name; 
                return "Arena module";
            }
        }

        public void Initialise(IConfigSource config)
        {
            avatarStartPoints = new List<StartPoint>();
            m_ArenaModePlayerNum.Add("PVP", 2);
            m_ArenaModePlayerNum.Add("PVE", 2);
            m_ArenaModePlayerNum.Add("2v2", 4);

            m_log.WarnFormat("[ArenaMod] start configuration");

            try
            {
                // if ((m_config = config.Configs["ArenaMod"]) != null)
                //     m_enabled = m_config.GetBoolean("Enabled", m_enabled);
                m_enabled = true;
            }
            catch (Exception e)
            {
                m_log.ErrorFormat("[ArenaMod] initialization error: {0}", e.Message);
                return;
            }

            m_log.ErrorFormat("[ArenaMod] module {0} enabled", (m_enabled ? "is" : "is not"));
        }

        public void PostInitialise()
        {
            if (m_enabled) { }
        }

        public void Close() { }
        public void AddRegion(Scene scene) { }
        public void RemoveRegion(Scene scene) { }

        public void RegionLoaded(Scene scene)
        {
            if (m_enabled)
            {
                m_scene = scene;
                m_comms = m_scene.RequestModuleInterface<IScriptModuleComms>();
                m_worldComm = m_scene.RequestModuleInterface<IWorldComm>();
                landOwnerUUID = scene.RegionInfo.EstateSettings.EstateOwner;
                if (m_comms == null)
                {
                    m_log.WarnFormat("[ArenaMod] ScriptModuleComms interface not defined");
                    m_enabled = false;

                    return;
                }

                m_comms.RegisterScriptInvocation(this, "start");
                m_comms.RegisterScriptInvocation(this, "setup");
                m_comms.RegisterScriptInvocation(this, "setPlayerReady");
                m_comms.RegisterScriptInvocation(this, "setMode");
                m_comms.RegisterScriptInvocation(this, "getMode");
                m_comms.RegisterScriptInvocation(this, "getModePlayerNum");
                m_comms.RegisterScriptInvocation(this, "getControllableObject");
                m_comms.RegisterScriptInvocation(this, "getController");
                m_comms.RegisterScriptInvocation(this, "getEnemy");
                m_comms.RegisterScriptInvocation(this, "getArenaCommChannel");
                m_comms.RegisterScriptInvocation(this, "commandCharacter");
                m_comms.RegisterScriptInvocation(this, "setNPC");
                m_comms.RegisterScriptInvocation(this, "canSetNPC");
                m_comms.RegisterScriptInvocation(this, "completeSetup");
                m_comms.RegisterScriptInvocation(this, "getPlayerNPC");
                m_comms.RegisterScriptInvocation(this, "reset");




                // Register some constants as well
                m_comms.RegisterConstant("ModConstantInt1", 25);
                m_comms.RegisterConstant("ModConstantFloat1", 25.000f);
                m_comms.RegisterConstant("ModConstantString1", "abcdefg");
            }
        }

        public Type ReplaceableInterface
        {
            get { return null; }
        }

        private void teleportToStart(UUID avatarID, Vector3 pos)
        {
            ScenePresence avatar = m_scene.GetScenePresence(avatarID);
            avatar.Teleport(pos);
        }

        private void print()
        {
            m_log.WarnFormat("----PRINT START----");
            foreach (StartPoint sp in avatarStartPoints)
            {
                sp.printAvatars();
            }
        }

        #endregion

        #region ScriptInvocationInteface

        public void setup(UUID hostID, UUID scriptID)
        {
            if (arenaReady)
            {
                m_log.WarnFormat("[ArenaMod] arena already setup");
                return;
            }
            List<SceneObjectGroup> sceneObjects = m_scene.GetSceneObjectGroups();
            foreach (SceneObjectGroup sc in sceneObjects)
            {
                foreach (string startPointName in startPointNames)
                {
                    if (startPointName.Equals(sc.Name))
                    {
                        SceneObjectPart obj = m_scene.GetSceneObjectPart(sc.UUID);
                        if (obj.OwnerID != landOwnerUUID)
                        {
                            m_log.WarnFormat("[ArenaMod] start point {0} is not owned by the land owner", sc.Name);
                            continue;
                        }
                        StartPoint sp = new StartPoint(obj);
                        avatarStartPoints.Add(sp);
                    }
                }
                if (controller == UUID.Zero && sc.Name.Equals(controllerName))
                {
                    controller = sc.UUID;
                }
            }
            arenaReady = true;
        }

        public int hasStarted()
        {
            return started ? 1 : 0;
        }
        public void setPlayerReady(UUID hostID, UUID scriptID, UUID playerUUID)
        {
            if (!arenaReady)
            {
                m_log.WarnFormat("setPlayerReady can only be invoked after setup");
                return;
            }

            if (started)
            {
                m_log.WarnFormat("setPlayerReady can only be invoked before start");
                return;
            }

            SceneObjectPart startObject = m_scene.GetSceneObjectPart(hostID);
            if (startObject.OwnerID != landOwnerUUID)
            {
                m_log.WarnFormat("setPlayerReady can only be invoked by the land owner ({0} is not the land owner)", startObject.OwnerID);
            }

            m_log.WarnFormat("setPlayerReady invoked by {0}", m_scene.GetScenePresence(playerUUID).Name);
            for (int i = 0; i < avatarStartPoints.Count; i++)
            {

                if (avatarStartPoints[i].getAvatars().IndexOf(playerUUID) != -1)
                {
                    m_log.WarnFormat("Player {0} is already registered", m_scene.GetScenePresence(playerUUID).Name);
                    return;
                }

                if (avatarStartPoints[i].getAvatars().Count < (m_ArenaModePlayerNum[currentMode] / maxStartPoints))
                {
                    m_log.WarnFormat("Trying Start point {0}, Count={1}", avatarStartPoints[i].getName(), avatarStartPoints[i].getAvatars().Count);
                    avatarStartPoints[i].printAvatars();
                    m_log.WarnFormat("Trying to add to {0}", avatarStartPoints[i].getName());
                    avatarStartPoints[i].addAvatar(playerUUID);
                    m_log.WarnFormat("Added to {0}", avatarStartPoints[i].getName());

                    return;
                }
                else
                {
                    m_log.WarnFormat("Start point {0} is full", avatarStartPoints[i].getName());
                }
            }
            m_log.ErrorFormat("Player number reached max capacity or no start points available {0}", avatarStartPoints.Count);
        }

        public void setNPC(UUID hostID, UUID scriptID, UUID npcID)
        {
            m_log.WarnFormat("1");
            UUID owner = m_scene.GetSceneObjectPart(hostID).OwnerID;
            m_log.WarnFormat("2");

            foreach (StartPoint startPoint in avatarStartPoints)
            {
                m_log.WarnFormat("3");

                if (startPoint.getUUID().Equals(hostID))
                {
                    m_log.WarnFormat("4");

                    startPoint.addNPC(npcID);
                }
                m_log.WarnFormat("5");

            }
        }

        public int start(UUID hostID, UUID scriptID)
        {
            if (!arenaReady)
            {
                m_log.WarnFormat("start can only be invoked after setup");
                return 1;
            }
            if (started)
            {
                m_log.WarnFormat("start can only be invoked once");
                return 1;
            }
            int readyAvatars = 0;
            foreach (StartPoint startPoint in avatarStartPoints)
            {
                readyAvatars += startPoint.getAvatars().Count;
                m_log.WarnFormat("start point {0} has {1} avatars", startPoint.getName(), startPoint.getAvatars().Count);
                startPoint.printAvatars();
            }
            if (readyAvatars < m_ArenaModePlayerNum[currentMode])
            {
                m_log.WarnFormat("not enough players for mode: {0}, number of current players is: {1}", currentMode, readyAvatars);
                return 1;
            }
            started = true;
            string globalMsg = "";
            foreach (StartPoint startPoint in avatarStartPoints)
            {
                startPoint.start();
                string msg = "";
                foreach (UUID avatar in startPoint.getAvatars())
                {
                    msg += avatar.ToString() + "+";
                    globalMsg += avatar.ToString() + "+";
                }
                m_log.WarnFormat("Sending message to {0} with {1}", startPoint.getName(), msg);
                msg = msg.Substring(0, msg.Length - 1);
                m_worldComm.DeliverMessageTo(startPoint.getUUID(),commChannel, new Vector3(0,0,0), "ArenaMod", UUID.Zero, msg );
                m_log.WarnFormat("Sent message with name {0}", startPoint.getName() + "ArenaMod");
            }
         m_worldComm.DeliverMessage(ChatTypeEnum.Region, commChannel, "ArenaMod", UUID.Zero, globalMsg);
            return 0;
        }

        public int setMode(UUID hostID, UUID scriptID, string mode)
        {
            if (started)
            {
                return 1;
            }
            m_ArenaModePlayerNum.TryGetValue(mode, out int value);
            if (value == 0)
            {
                return 1;
            }
            currentMode = mode;
            return 0;
        }

        public string getMode(UUID hostID, UUID scriptID)
        {
            return currentMode;
        }
        public int getModePlayerNum(UUID hostID, UUID scriptID)
        {
            m_ArenaModePlayerNum.TryGetValue(currentMode, out int value);
            return value;
        }
        public int canSetNPC(UUID hostID, UUID scriptID)
        {
            foreach (StartPoint startPoint in avatarStartPoints)
            {
                if (hostID.Equals(startPoint.getUUID()))
                {
                    return startPoint.canAddNPC() ? 1 : 0;
                }
            }
            m_log.WarnFormat("Start point {0} not found", hostID);
            return 0;
        }

        public UUID getControllableObject(UUID hostID, UUID scriptID)
        {
            UUID owner = m_scene.GetSceneObjectPart(hostID).OwnerID;
            foreach (StartPoint startPoint in avatarStartPoints)
            {
                for (int i = 0; i < startPoint.getAvatars().Count; i++)
                {
                    if (startPoint.getAvatars()[i] == owner)
                    {
                        return startPoint.getNPCs()[i];
                    }
                }
            }
            return UUID.Zero;
        }

        public UUID getEnemy(UUID hostID, UUID scriptID)
        {
            UUID owner = m_scene.GetSceneObjectPart(hostID).OwnerID;
            foreach (StartPoint startPoint in avatarStartPoints)
            {
                bool flag = false;
                for (int i = 0; i < startPoint.getAvatars().Count; i++)
                {
                    if (startPoint.getAvatars()[i] == owner)
                    {
                        flag = true;
                    }
                }
                if (!flag) return startPoint.getNPCs()[0];
            }
            return UUID.Zero;
        }
        public UUID commandCharacter(UUID hostID, UUID scriptID, string command)
        {
            UUID owner = m_scene.GetSceneObjectPart(hostID).OwnerID;
            foreach (StartPoint startPoint in avatarStartPoints)
            {
                foreach (UUID avatar in startPoint.getAvatars())
                {
                    if (avatar == owner)
                    {
                        m_worldComm.DeliverMessageTo(startPoint.getUUID(), commChannel, new Vector3(0, 0, 0), "controller", UUID.Zero, command);
                        return startPoint.getUUID();
                    }
                }
            }
            return UUID.Zero;
        }

        public int getArenaCommChannel(UUID hostID, UUID scriptID)
        {
            return commChannel;
        }

        public void completeSetup(UUID hostID, UUID scriptID)
        {
            if (!arenaReady || !started)
            {
                m_log.WarnFormat("Arena not ready or started");
                return;
            }
            foreach (StartPoint startPoint in avatarStartPoints)
            {
                if (startPoint.getUUID().Equals(hostID))
                {
                    for (int i = 0; i < startPoint.getAvatars().Count; i++)
                    {
                        if (m_Players_NPCs.Count >= m_ArenaModePlayerNum[currentMode])
                        {
                            m_log.WarnFormat("Too many players in arena");
                            return;
                        }
                        m_Players_NPCs.Add(startPoint.getAvatars()[i], startPoint.getNPCs()[i]);
                        m_log.WarnFormat("Adding Dict {0} to {1}", startPoint.getAvatars()[i], startPoint.getNPCs()[i]);
                    }
                }
            }
        }

        public int isPlayerNPC(UUID hostID, UUID scriptID, UUID playerID, UUID npcID)
        {
            if (m_Players_NPCs.ContainsKey(playerID) && m_Players_NPCs[playerID].Equals(npcID))
            {
                return 1;
            }
            return 0;
        }

        public UUID getPlayerNPC(UUID hostID, UUID scriptID, UUID playerID)
        {
            if (m_Players_NPCs.ContainsKey(playerID))
            {
                return m_Players_NPCs[playerID];
            }
            return UUID.Zero;
        }

        public UUID getController(UUID hostID, UUID scriptID)
        {
            return controller;
        }

        public void reset(UUID hostID, UUID scriptID)
        {
            foreach (StartPoint startPoint in avatarStartPoints)
            {
                startPoint.reset();
            }
            m_Players_NPCs.Clear();
            started = false;
            currentMode = "PVE";
            if (arenaReady) m_scene.GetSceneObjectGroup(controller).TriggerScriptChangedEvent(Changed.REGION_RESTART);
        }
        #endregion
    }
}