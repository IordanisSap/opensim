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
        const float PI = 3.1415f;
        private IScriptModuleComms m_comms;
        private IWorldComm m_worldComm;
        private UUID landOwnerUUID = UUID.Zero;

        private int commChannel = -5;
        #region IRegionModule Members

        private UUID startPoint = UUID.Zero;
        private UUID endPoint = UUID.Zero;

        private string mazeControllerName = "MazeController";
        private UUID mazeControllerID = UUID.Zero;

        private UUID[,] mazeObjUUIDs = null;

        private UUID[,] mazeObstacleUUIDs = null;

        private UUID[,] mazePowerupsUUIDs = null;

        private UUID floorUUID = UUID.Zero;
        private Random random = new Random();

        private List<Player> players = new List<Player>();

        private const int objScale = 2;

        private Dictionary<string, Timer> timerDictionary = new Dictionary<string, Timer>();

        private PowerUpManager powerUpManager = new PowerUpManager();

        private ObstacleManager obstacleManager = new ObstacleManager();
        private UUID mazeBallUUID = UUID.Zero;
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

        private void initPowerUps()
        {
            powerUpManager.AddPowerUp(
                new PowerUp(
                    "Shield",
                    15000,
                    delegate (Player player)
                    {
                        Console.WriteLine("Shield activated");
                        TaskInventoryItem textureItem = m_scene.GetSceneObjectPart(getController()).Inventory.GetInventoryItem("Shield_texture");
                        Primitive.TextureEntry texture = new Primitive.TextureEntry(textureItem.AssetID);
                        Primitive.TextureEntryFace face = texture.CreateFace(0);
                        face.Glow = 0.3f;
                        face.RGBA = new Color4(0, 0.5f, 1f, 1f);
                        face.Fullbright = true;
                        SceneObjectGroup playerObj = m_scene.GetSceneObjectGroup(player.getUUID());
                        playerObj.RootPart.Shape.Textures.FaceTextures[0] = face;
                        Primitive.TextureAnimation pTexAnim = new Primitive.TextureAnimation
                        {
                            Flags = Primitive.TextureAnimMode.ANIM_ON | Primitive.TextureAnimMode.LOOP | Primitive.TextureAnimMode.SMOOTH | Primitive.TextureAnimMode.ROTATE,
                            Face = 255,
                            Length = 2 * PI,
                            Rate = 2 * PI,
                            SizeX = 1,
                            SizeY = 1,
                            Start = 0
                        };
                        playerObj.RootPart.AddTextureAnimation(pTexAnim);
                        playerObj.RootPart.UpdateTextureEntry(texture);
                        playerObj.RootPart.SendFullUpdateToAllClients();
                        playerObj.RootPart.ParentGroup.HasGroupChanged = true;
                    },
                    delegate (Player player)
                    {
                        Primitive.TextureEntry texture = new Primitive.TextureEntry(UUID.Parse("5748decc-f629-461c-9a36-a35a221fe21f"));
                        Primitive.TextureEntryFace face = texture.CreateFace(0);
                        face.Glow = 0.03f;
                        face.RGBA = new Color4(0, 0.5f, 1f, 1f);
                        face.Fullbright = true;
                        SceneObjectGroup playerObj = m_scene.GetSceneObjectGroup(player.getUUID());
                        if (playerObj == null)
                        {
                            Console.WriteLine("Player object not found");
                            return;
                        }
                        playerObj.RootPart.Shape.Textures.FaceTextures[0] = face;

                        Primitive.TextureAnimation pTexAnim = new Primitive.TextureAnimation
                        {
                            Flags = Primitive.TextureAnimMode.ANIM_OFF,
                            Face = 255,
                            Length = 2 * PI,
                            Rate = 2 * PI,
                            SizeX = 1,
                            SizeY = 1,
                            Start = 0
                        };
                        playerObj.RootPart.AddTextureAnimation(pTexAnim);
                        playerObj.RootPart.UpdateTextureEntry(texture);
                        playerObj.RootPart.SendFullUpdateToAllClients();
                        playerObj.RootPart.ParentGroup.HasGroupChanged = true;
                        Console.WriteLine("Shield deactivated");
                    }
                )
            );

        }

        private void initObstacles()
        {
            obstacleManager.AddObstacle(
                new Obstacle(
                    "Spikes1",
                    delegate (Player player)
                    {
                        if (player.hasPowerUp("Shield")){return;}
                        SceneObjectGroup start = m_scene.GetSceneObjectGroup(startPoint);
                        SceneObjectGroup sc = m_scene.GetSceneObjectGroup(player.getUUID());
                        sc.TeleportObject(sc.UUID, start.AbsolutePosition, Quaternion.Identity, 1);
                    }
                )
            );
        }
        public void Initialise(IConfigSource config)
        {
            m_log.WarnFormat("[MazeMod] start configuration");

            try
            {
                // if ((m_config = config.Configs["MazeMod"]) != null)
                //     m_enabled = m_config.GetBoolean("Enabled", m_enabled);
                initPowerUps();
                initObstacles();
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
                m_comms.RegisterScriptInvocation(this, "resetMaze");
                m_comms.RegisterScriptInvocation(this, "obstacleCollision");
                m_comms.RegisterScriptInvocation(this, "floorCollision");
                m_comms.RegisterScriptInvocation(this, "powerUpCollision");
                m_comms.RegisterScriptInvocation(this, "endPointCollision");

                

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
            generatePath(maze, size);
            LoadEndPointObject(endPoint);
            createBall(startPoint);
            createObstacles(mazeObjUUIDs);
            createPowerUps(mazeObjUUIDs, mazeObstacleUUIDs);
            createFloor(size * 2 + 1, pos - Vector3.UnitZ * 5);
            m_log.WarnFormat("[MazeMod] Generating maze: " + size.ToString());

        }

        private void generatePath(in Maze2D maze, int size)
        {
            int[,] binaryMaze = new BinaryMaze2D(maze.getCells()).getCells();
            mazeObjUUIDs = new UUID[size * 2 + 1, size * 2 + 1];
            UUID hostID = getController();
            Vector3 pos = m_scene.GetSceneObjectPart(hostID).AbsolutePosition;


            for (int y = 0; y < size * 2 + 1; y++)
            {
                for (int x = 0; x < size * 2 + 1; x++)
                {
                    if (binaryMaze[x, y] == 0)
                    {
                        mazeObjUUIDs[x, y] = generateCube(hostID, pos + new Vector3(2 * x, 2 * y, 0));
                    }
                    else
                    {
                        mazeObjUUIDs[x, y] = UUID.Zero;
                    }
                }
            }

            for (int x = 0; x < size * 2 + 1; x++)
            {
                if (binaryMaze[x, 0] == 0)
                {
                    startPoint = mazeObjUUIDs[x, 0];
                }
            }
            for (int x = 0; x < size * 2 + 1; x++)
            {
                if (binaryMaze[x, size * 2] == 0)
                {
                    endPoint = mazeObjUUIDs[x, size * 2];
                }
            }
        }

        public UUID generateCube(UUID hostID, Vector3 pos)
        {
            try
            {
                SceneObjectPart obj = m_scene.GetSceneObjectPart(hostID);
                OpenMetaverse.UUID owner = obj.OwnerID;
                SceneObjectPart part = new SceneObjectPart(hostID, PrimitiveBaseShape.CreateBox(), pos, Quaternion.Identity, Vector3.Zero);
                // Get the texture entry of the cube
                TaskInventoryItem textureItem = m_scene.GetSceneObjectPart(getController()).Inventory.GetInventoryItem("Path_texture");
                Primitive.TextureEntry texture = new Primitive.TextureEntry(textureItem.AssetID);
                Primitive.TextureEntryFace face = texture.CreateFace(0);
                part.Shape.Textures.FaceTextures[0] = face;

                SceneObjectGroup group = new SceneObjectGroup(part);
                group.Name = "Path_Cube";
                part.Scale = new Vector3(objScale, objScale, 1);
                group.OwnerID = owner;
                group.RootPart.OwnerID = owner;
                group.RootPart.CreatorID = owner;
                group.RootPart.LastOwnerID = owner;
                group.RootPart.GroupID = UUID.Zero;
                group.GroupID = UUID.Zero;
                group.LastOwnerID = hostID;
                m_scene.AddNewSceneObject(group, false);
                part.UpdateTextureEntry(texture);


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

        private void createBall(UUID startPoint)
        {
            try
            {
                SceneObjectPart controller = m_scene.GetSceneObjectPart(getController());
                TaskInventoryItem item = controller.Inventory.GetInventoryItem("Ball");
                SceneObjectPart startPointObj = m_scene.GetSceneObjectPart(startPoint);
                List<SceneObjectGroup> newBall = m_scene.RezObject(controller, item, startPointObj.AbsolutePosition, null, Vector3.Zero, 0, false, false);
                mazeBallUUID = newBall[0].UUID;
                newBall[0].ResumeScripts();
                players.Add(new Player(newBall[0].UUID, "player" + players.Count.ToString()));



            }
            catch (Exception e)
            {
                m_log.WarnFormat("[MazeMod] Error loading ball: " + e.Message);
            }
        }

        private void createObstacles(UUID[,] map)
        {
            try
            {
                //obstacleManager.AddAction("Spikes1", )
                mazeObstacleUUIDs = new UUID[map.GetLength(0), map.GetLength(1)];
                for (int y = 2; y < map.GetLength(1) - 1; y++)
                {
                    for (int x = 1; x < map.GetLength(0); x++)
                    {
                        if (map[x, y] != UUID.Zero)
                        {
                            if (mazeObstacleUUIDs[x - 1, y] != UUID.Zero || mazeObstacleUUIDs[x, y - 1] != UUID.Zero || random.Next(0, 20) != 0) continue;
                            SceneObjectPart controller = m_scene.GetSceneObjectPart(getController());
                            Obstacle randomObstacle = obstacleManager.GetRandomObstacle();
                            m_log.WarnFormat("[MazeMod] Creating obstacle: " + randomObstacle.Name);
                            TaskInventoryItem obstacleInvItem = controller.Inventory.GetInventoryItem(randomObstacle.Name);
                            SceneObjectPart spawnPoint = m_scene.GetSceneObjectPart(map[x, y]);
                            List<SceneObjectGroup> newObstacle = m_scene.RezObject(controller, obstacleInvItem, spawnPoint.AbsolutePosition + new Vector3(0, 0, spawnPoint.Scale.Z * 1.15f), null, Vector3.Zero, 0, false, false);
                            mazeObstacleUUIDs[x, y] = newObstacle[0].UUID;
                            newObstacle[0].ResumeScripts();
                            obstacleManager.AddObject(newObstacle[0].UUID, randomObstacle.Name);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                m_log.WarnFormat("[MazeMod] Error loading ball: " + e.Message);
            }
        }

        private void createPowerUps(UUID[,] map, UUID[,] obstacles)
        {
            try
            {
                mazePowerupsUUIDs = new UUID[map.GetLength(0), map.GetLength(1)];
                for (int y = 2; y < map.GetLength(1) - 1; y++)
                {
                    for (int x = 1; x < map.GetLength(0); x++)
                    {
                        if (map[x, y] != UUID.Zero)
                        {
                            if (mazePowerupsUUIDs[x - 1, y] != UUID.Zero || mazePowerupsUUIDs[x, y - 1] != UUID.Zero || random.Next(0, 20) != 0) continue;
                            SceneObjectPart controller = m_scene.GetSceneObjectPart(getController());

                            PowerUp randomPowerUp = powerUpManager.GetRandomPowerUp();
                            TaskInventoryItem powerup = controller.Inventory.GetInventoryItem(randomPowerUp.Name);
                            SceneObjectPart spawnPoint = m_scene.GetSceneObjectPart(map[x, y]);
                            SceneObjectPart obstaclePoint = m_scene.GetSceneObjectPart(obstacles[x, y]);
                            Vector3 spawnPos = spawnPoint.AbsolutePosition + new Vector3(0, 0, spawnPoint.Scale.Z * 1.15f);
                            if (obstaclePoint != null) spawnPos += new Vector3(0, 0, obstaclePoint.Scale.Z * 2.2f);
                            List<SceneObjectGroup> newPowerup = m_scene.RezObject(controller, powerup, spawnPos, null, Vector3.Zero, 0, false, false);
                            mazePowerupsUUIDs[x, y] = newPowerup[0].UUID;
                            newPowerup[0].ResumeScripts();
                            powerUpManager.AddObject(newPowerup[0].UUID, randomPowerUp.Name);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                m_log.WarnFormat("[MazeMod] Error loading ball: " + e.Message);
            }
        }


        private void createFloor(int size, Vector3 startPoint)
        {
            SceneObjectPart controller = m_scene.GetSceneObjectPart(getController());
            TaskInventoryItem floor = controller.Inventory.GetInventoryItem("Floor");
            List<SceneObjectGroup> newFloor = m_scene.RezObject(controller, floor, startPoint + new Vector3(objScale * size / 2, objScale * size / 2, 0), null, Vector3.Zero, 0, false, false);
            newFloor[0].GroupResize(new Vector3(objScale * size * 2, objScale * size * 2, 1));
            newFloor[0].ResumeScripts();
            floorUUID = newFloor[0].UUID;
        }

        private void deleteMaze()
        {
            try
            {
                m_log.WarnFormat("[MazeMod] Deleting maze2");
                if (mazeObjUUIDs == null) return;
                m_log.WarnFormat("[MazeMod] Deleting maze");
                for (int y = 0; y < mazeObjUUIDs.GetLength(1); y++)
                {
                    for (int x = 0; x < mazeObjUUIDs.GetLength(0); x++)
                    {
                        if (mazeObjUUIDs[x, y] != null)
                        {
                            SceneObjectGroup obj = m_scene.GetSceneObjectGroup(mazeObjUUIDs[x, y]);
                            if (obj != null) m_scene.DeleteSceneObject(obj, false);

                            SceneObjectGroup obj2 = m_scene.GetSceneObjectGroup(mazeObstacleUUIDs[x, y]);
                            if (obj2 != null) m_scene.DeleteSceneObject(obj2, false);

                            SceneObjectGroup obj3 = m_scene.GetSceneObjectGroup(mazePowerupsUUIDs[x, y]);
                            if (obj3 != null) m_scene.DeleteSceneObject(obj3, false);
                        }
                    }
                }
                SceneObjectGroup ball = m_scene.GetSceneObjectGroup(mazeBallUUID);
                if (ball != null) m_scene.DeleteSceneObject(ball, false);
                SceneObjectGroup floor = m_scene.GetSceneObjectGroup(floorUUID);
                if (floor != null) m_scene.DeleteSceneObject(floor, false);
            }
            catch (Exception e)
            {
                m_log.WarnFormat("[MazeMod] Error deleting maze: " + e.Message);
            }
        }

        private Player getPlayer(UUID player)
        {
            return players.Find(obj => obj.getUUID() == player);
        }
        public void obstacleCollision(UUID hostID, UUID scriptID, UUID player)
        {
            m_log.WarnFormat("[MazeMod] Ball collided with obstacle");
            Player p = getPlayer(player);
            if (p == null) return;
            obstacleManager.OnCollision(hostID, getPlayer(player));
        }

        public void powerUpCollision(UUID hostID, UUID scriptID, UUID player)
        {
            m_log.WarnFormat("[MazeMod] Ball collided with powerup");
            Player p = getPlayer(player);
            string timerId = p.getUUID().ToString() + hostID.ToString();
            powerUpManager.ActivatePowerUp(hostID, p);
            if (timerDictionary.ContainsKey(timerId))
            {
                Timer timerToRemove = timerDictionary[timerId];
                timerToRemove.Dispose();
                timerDictionary.Remove(timerId);
            }
            Timer timer = new Timer((object state) =>
            {
                Console.WriteLine("timerrr"); powerUpManager.DeactivatePowerUp(hostID, p);
                if (timerDictionary.ContainsKey(timerId))
                {
                    Timer timerToRemove = timerDictionary[timerId];
                    timerToRemove.Dispose();
                    timerDictionary.Remove(timerId);
                }
            }, null, powerUpManager.getPowerUp(hostID).Duration, Timeout.Infinite);
            timerDictionary.Add(timerId, timer);
        }

        public void floorCollision(UUID hostID, UUID scriptID, UUID player)
        {
            m_log.WarnFormat("[MazeMod] Ball collided with obstacle");
            SceneObjectGroup playerObj = m_scene.GetSceneObjectGroup(player);
            SceneObjectPart startPointObj = m_scene.GetSceneObjectPart(startPoint);
            playerObj.TeleportObject(player, startPointObj.AbsolutePosition, Quaternion.Identity, 1);
        }

        public int endPointCollision(UUID hostID, UUID scriptID, UUID player)
        {
            m_log.WarnFormat("[MazeMod] Ball collided with endpoint");
            Player p = getPlayer(player);
            if (p == null) return 0;
            SceneObjectGroup playerObj = m_scene.GetSceneObjectGroup(player);
            playerObj.ScriptSetPhantomStatus(true);
            playerObj.ScriptSetPhysicsStatus(false);
            Primitive.TextureEntry texture = playerObj.RootPart.Shape.Textures;
            Primitive.TextureEntryFace face = texture.CreateFace(0);
            face.Glow = 0.3f;
            face.RGBA = new Color4(0, 0.5f, 1f, 0.3f);
            texture.FaceTextures[0] = face;
            playerObj.TeleportObject(player,playerObj.AbsolutePosition + new Vector3(0, 0, 3), Quaternion.Identity, 1);
            playerObj.RootPart.UpdateTextureEntry(texture);
            return 1;

        }

        public int getArenaCommChannel(UUID hostID, UUID scriptID)
        {
            return commChannel;
        }

        public void resetMaze(UUID hostID, UUID scriptID)
        {
            deleteMaze();
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