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
using OpenMetaverse.Assets;
using OpenMetaverse.Rendering;
using OpenMetaverse.StructuredData;
using OpenSim.Framework;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace MazeModule
{
    [Extension(Path = "/OpenSim/RegionModules", NodeName = "RegionModule", Id = "MazeModule")]

    public class MazeModule : INonSharedRegionModule
    {

        private static readonly ILog m_log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IConfig m_config = null;
        private bool m_enabled = true;
        private Scene m_scene = null;

        private IScriptModuleComms m_comms;
        private IWorldComm m_worldComm;
        private UUID landOwnerUUID = UUID.Zero;

        private int commChannel = -5;
        #region IRegionModule Members

        private UUID startPoint = UUID.Zero;
        private UUID endPoint = UUID.Zero;

        private string mazeControllerName = "MazeController";
        private UUID mazeControllerID = UUID.Zero;

        public bool isOwner(UUID avatarID)
        {
            return avatarID == landOwnerUUID;
        }
        public string Name
        {
            get
            { //return this.GetType().Name; 
                return "Maze module";
            }
        }

        public void Initialise(IConfigSource config)
        {
            m_log.WarnFormat("[MazeMod] start configuration");

            try
            {
                // if ((m_config = config.Configs["MazeMod"]) != null)
                //     m_enabled = m_config.GetBoolean("Enabled", m_enabled);
                m_enabled = true;
            }
            catch (Exception e)
            {
                m_log.ErrorFormat("[MazeMod] initialization error: {0}", e.Message);
                return;
            }

            m_log.ErrorFormat("[MazeMod] module {0} enabled", (m_enabled ? "is" : "is not"));
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
                    m_log.WarnFormat("[MazeMod] ScriptModuleComms interface not defined");
                    m_enabled = false;
                    return;
                }

                m_comms.RegisterScriptInvocation(this, "generateMaze2D");
                m_comms.RegisterScriptInvocation(this, "generateMaze3D");
                m_comms.RegisterScriptInvocation(this, "getStartPoint");
                m_comms.RegisterScriptInvocation(this, "getEndPoint");
                m_comms.RegisterScriptInvocation(this, "generateTestCube");

                // Register some constants as well
                // m_comms.RegisterConstant("ModConstantInt1", 25);
                // m_comms.RegisterConstant("ModConstantFloat1", 25.000f);
                // m_comms.RegisterConstant("ModConstantString1", "abcdefg");
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

        #endregion

        #region ScriptInvocationInteface

        public void generateMaze2D(UUID hostID, UUID scriptID, int size)
        {
            Maze2D maze = new Maze2D(size, size);
            Vector3 pos = m_scene.GetSceneObjectPart(hostID).AbsolutePosition;
            Console.WriteLine("Generating:\n");
            maze.printMaze();
            int[,] binaryMaze = new BinaryMaze2D(maze.getCells()).getCells();
            UUID[,] UUIDs = new UUID[size * 2 + 1, size * 2 + 1];
            for (int y = 0; y < size * 2 + 1; y++)
            {
                for (int x = 0; x < size * 2 + 1; x++)
                {
                    if (binaryMaze[x, y] == 0)
                    {
                        UUIDs[x, y] = generateCube(hostID, scriptID, pos + new Vector3(2 * x, 2 * y, 0));
                    }
                }
            }

            for (int x = 0; x < size * 2 + 1; x++)
            {
                if (binaryMaze[x, 0] == 0)
                {
                    startPoint = UUIDs[x, 0];
                }
            }
            for (int x = 0; x < size * 2 + 1; x++)
            {
                if (binaryMaze[x, size * 2] == 0)
                {
                    endPoint = UUIDs[x, size * 2];
                }
            }
            LoadEndPointObject(endPoint);
            createBall(startPoint);
            m_log.WarnFormat("[MazeMod] Generating maze: " + size.ToString());

        }

        public UUID generateCube(UUID hostID, UUID scriptID, Vector3 pos)
        {
            try
            {
                SceneObjectPart obj = m_scene.GetSceneObjectPart(hostID);
                OpenMetaverse.UUID owner = obj.OwnerID;
                SceneObjectPart part = new SceneObjectPart(hostID, PrimitiveBaseShape.CreateBox(), pos, Quaternion.Identity, Vector3.Zero);
                // Get the texture entry of the cube
                Console.Write("1");

                Primitive.TextureEntry textureEntry = part.Shape.Textures;
                Primitive.TextureEntryFace face = textureEntry.CreateFace(0);

                textureEntry.DefaultTexture.TextureID = new OpenMetaverse.UUID("89556747-24cb-43ed-920b-47caed15465f");
                face.TextureID = new OpenMetaverse.UUID("00000000-0000-0000-0000-000000000000");
                textureEntry.FaceTextures[0] = face;

                SceneObjectGroup group = new SceneObjectGroup(part);
                group.Name = "Test Cube";
                part.Scale = new Vector3(2, 2, 1);
                group.OwnerID = owner;
                group.RootPart.OwnerID = owner;
                group.RootPart.CreatorID = owner;
                group.RootPart.LastOwnerID = owner;
                group.RootPart.GroupID = UUID.Zero;
                group.GroupID = UUID.Zero;
                group.LastOwnerID = hostID;
                m_scene.AddNewSceneObject(group, false);
                part.UpdateTextureEntry(textureEntry);


                return part.UUID;
            }

            catch (Exception e)
            {
                m_log.WarnFormat("[MazeMod] Error generating cube: " + e.Message);
                return UUID.Zero;
            }
        }


        private void findController()
        {
            List<SceneObjectGroup> sceneObjects = m_scene.GetSceneObjectGroups();
            foreach (SceneObjectGroup sc in sceneObjects)
            {
                if (mazeControllerName.Equals(sc.Name))
                {
                    mazeControllerID = sc.UUID;
                }
            }
        }
        private UUID getController()
        {
            if (mazeControllerID == UUID.Zero) findController();
            return mazeControllerID;
        }
        public UUID getStartPoint(UUID hostID, UUID scriptID)
        {
            return startPoint;
        }
        public UUID getEndPoint(UUID hostID, UUID scriptID)
        {
            return endPoint;
        }
        private void LoadEndPointObject(UUID objectUUID)
        {
            try
            {
                SceneObjectPart srcObject = m_scene.GetSceneObjectPart(getController());
                TaskInventoryItem item = srcObject.Inventory.GetInventoryItem("Endpoint");
                SceneObjectPart targetObject = m_scene.GetSceneObjectPart(objectUUID);
                targetObject.ScriptAccessPin = 123;
                m_scene.RezScriptFromPrim(item.ItemID, srcObject, objectUUID, 123, 1, 0);
            }
            catch (Exception e)
            {
                m_log.WarnFormat("[MazeMod] Error loading endpoint object: " + e.Message);
            }
        }

        private void createBall(UUID startPoint){
            try
            {
                SceneObjectPart controller = m_scene.GetSceneObjectPart(getController());
                TaskInventoryItem item = controller.Inventory.GetInventoryItem("Ball");
                SceneObjectPart startPointObj = m_scene.GetSceneObjectPart(startPoint);
                m_scene.RezObject(controller,item,startPointObj.AbsolutePosition, null, Vector3.Zero,0, false, false);
                
            }
            catch (Exception e)
            {
                m_log.WarnFormat("[MazeMod] Error loading ball: " + e.Message);
            }
        }

        public int getArenaCommChannel(UUID hostID, UUID scriptID)
        {
            return commChannel;
        }

        public void reset(UUID hostID, UUID scriptID)
        {

        }

        public void generateMaze3D(UUID hostID, UUID scriptID, int size)
        {
            // Maze3D maze = new Maze3D(size, size, size);
            // Vector3 pos = m_scene.GetSceneObjectPart(hostID).AbsolutePosition;
            // Console.WriteLine("Generating:\n");
            // maze.printMaze();
            // int[,,] binaryMaze = maze.getMaze();
            // for (int z = 0; z < size; z++)
            // {
            //     for (int y = 0; y < size * 2 + 1; y++)
            //     {
            //         for (int x = 0; x < size * 2 + 1; x++)
            //         {
            //             if (binaryMaze[x, y, z] == 1)
            //             {
            //                 generateCube(hostID, scriptID, pos + new Vector3(8 * x, 8 * y, 8 * z));
            //             }
            //         }
            //     }
            // }
            m_log.WarnFormat("[MazeMod] Generating maze: " + size.ToString());
        }
        #endregion
    }
}