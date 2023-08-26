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

        private Vector3 START_OFFSET = new Vector3(-20, 3, -3);
        private IScriptModuleComms m_comms;
        private IWorldComm m_worldComm;
        private UUID landOwnerUUID = UUID.Zero;

        private int commChannel = -5;
        #region IRegionModule Members

        private UUID startPoint = UUID.Zero;
        private int[] startPointPos = new int[2] { 0, 0 };
        private UUID endPoint = UUID.Zero;
        private int[] endPointPos = new int[2] { 0, 0 };
        private UUID avatarPlayer = UUID.Zero;
        private string mazeControllerName = "MazeController";
        private SceneObjectPart mazeController = null;

        private UUID[,] mazeObjUUIDs = null;

        private UUID[,] mazeObstacleUUIDs = null;

        private UUID[,] mazePowerupsUUIDs = null;

        private UUID[,] landmarkUUIDs = null;

        private UUID[,] placedPathUUIDs = null;

        private UUID floorUUID = UUID.Zero;
        private Random random = new Random();

        private List<Player> players = new List<Player>();

        private const int objScale = 2;

        private Dictionary<string, Timer> timerDictionary = new Dictionary<string, Timer>();

        private MazeSolver mazeSolver = null;
        private PowerUpModule PowerUpModule = new PowerUpModule();

        private ObstacleModule ObstacleModule = new ObstacleModule();

        private LandmarkModule LandmarkModule = null;
        private AttachmentModule AttachmentModule = null;
        private CleanerModule CleanerModule = null;
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
            PowerUpModule.AddPowerUp(
                new PowerUp(
                    "Shield",
                    15000,
                    delegate (Player player, object[] data)
                    {
                        TaskInventoryItem textureItem = getController().Inventory.GetInventoryItem("Shield_texture");
                        Primitive.TextureEntry texture = new Primitive.TextureEntry(textureItem.AssetID);
                        Primitive.TextureEntryFace face = texture.CreateFace(0);
                        face.Glow = 0.2f;
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
                    delegate (Player player, object[] data)
                    {
                        Primitive.TextureEntry texture = new Primitive.TextureEntry(UUID.Parse("5748decc-f629-461c-9a36-a35a221fe21f"));
                        Primitive.TextureEntryFace face = texture.CreateFace(0);
                        face.Glow = 0.04f;
                        face.RGBA = new Color4(0, 0.455f, 0.906f, 1f);
                        face.Fullbright = false;
                        SceneObjectGroup playerObj = m_scene.GetSceneObjectGroup(player.getUUID());
                        if (playerObj == null)
                        {
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
                    }
                )
            );
            PowerUpModule.AddPowerUp(
                new PowerUp(
                    "Random",
                    15000,
                    delegate (Player player, object[] data)
                    {

                    },
                    delegate (Player player, object[] data)
                    {

                    }
                )
            );
            PowerUpModule.AddPowerUp(
                new PowerUp(
                    "Build",
                    100,
                    delegate (Player player, object[] data)
                    {
                        //buildPath(player, (string)data[0], Convert.ToInt32(data[1]));
                        buildPath(player, (string)data[0], 1);

                    },
                    delegate (Player player, object[] data)
                    {

                    }
                )
            );
            PowerUpModule.AddPowerUp(
                new PowerUp(
                    "Jump",
                    100,
                    delegate (Player player, object[] data)
                    {
                        SceneObjectGroup playerObj = m_scene.GetSceneObjectGroup(player.getUUID());
                        UUID nextLandmark = LandmarkModule.getNextLandmark(LandmarkModule.getPlayerLandmark(player.getUUID()));
                        SceneObjectPart nextLandmarkInstance = m_scene.GetSceneObjectPart(nextLandmark);
                        //playerObj.RootPart.ParentGroup.MoveToTarget(nextLandmarkInstance.AbsolutePosition - new Vector3(0, 0, nextLandmarkInstance.Scale.Z * 2), 0.5f);
                        playerObj.TeleportObject(player.getUUID(), nextLandmarkInstance.AbsolutePosition + new Vector3(0, 0, 3), Quaternion.Identity, 1);
                        playerObj.RootPart.SetVelocity(new Vector3(0, 0, 0), true);
                        player.AddToPath(LandmarkModule.getLandmark(nextLandmark).getStartPoint());

                    },
                    delegate (Player player, object[] data)
                    {

                    }
                )
            );
        }

        private void initObstacles()
        {
            ObstacleModule.AddObstacle(
                new Obstacle(
                    "Obstacle1",
                    delegate (Player player)
                    {
                        if (player.hasPowerUp("Shield")) { return; }
                        SceneObjectGroup start = m_scene.GetSceneObjectGroup(startPoint);
                        teleportToStart(player.getUUID());
                    },
                    (int[] pos) =>
                    {
                        List<int[]> path = mazeSolver.getPath();
                        bool inPath = false;
                        foreach (int[] array in path)
                        {
                            if (array[0] == pos[0] && array[1] == pos[1]) inPath = true;
                        }
                        mazeSolver.printPath();
                        if (!inPath) return true;

                        bool found = false;
                        uint obstacleNum = 0;
                        uint shieldNum = 0;
                        for (int i = path.Count - 1; i >= 0; i--)
                        {
                            int[] array = path[i];
                            if (mazeObstacleUUIDs[array[0], array[1]] != UUID.Zero && ObstacleModule.GetObstacle(mazeObstacleUUIDs[array[0], array[1]]).Name == "Obstacle1") obstacleNum++;
                            if (mazePowerupsUUIDs[array[0], array[1]] != UUID.Zero && PowerUpModule.getPowerUp(mazePowerupsUUIDs[array[0], array[1]]).Name == "Shield") shieldNum++;
                            if (array[0] == pos[0] && array[1] == pos[1])
                            {
                                if (obstacleNum > shieldNum) return false;
                                found = true;
                                continue;
                            }
                            if (!found) continue;
                            if (mazeObstacleUUIDs[array[0], array[1]] != UUID.Zero && ObstacleModule.GetObstacle(mazeObstacleUUIDs[array[0], array[1]]).Name == "Obstacle1") return false;
                            if (mazePowerupsUUIDs[array[0], array[1]] != UUID.Zero && PowerUpModule.getPowerUp(mazePowerupsUUIDs[array[0], array[1]]).Name == "Shield") return true;
                        }
                        return false;
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
                AttachmentModule = new AttachmentModule(m_scene);
                LandmarkModule = new LandmarkModule(m_scene);
                CleanerModule = new CleanerModule(m_scene);
                landOwnerUUID = scene.RegionInfo.EstateSettings.EstateOwner;
                if (m_comms == null)
                {
                    m_log.WarnFormat("[MazeMod] ScriptModuleComms interface not defined");
                    m_enabled = false;
                    return;
                }

                m_comms.RegisterScriptInvocation(this, "generateMaze");
                m_comms.RegisterScriptInvocation(this, "getStartPoint");
                m_comms.RegisterScriptInvocation(this, "getEndPoint");
                m_comms.RegisterScriptInvocation(this, "generateTestCube");
                m_comms.RegisterScriptInvocation(this, "resetMaze");
                m_comms.RegisterScriptInvocation(this, "obstacleCollision");
                m_comms.RegisterScriptInvocation(this, "floorCollision");
                m_comms.RegisterScriptInvocation(this, "powerUpCollision");
                m_comms.RegisterScriptInvocation(this, "landMarkCollision");
                m_comms.RegisterScriptInvocation(this, "endPointCollision");
                m_comms.RegisterScriptInvocation(this, "movePlayer");
                m_comms.RegisterScriptInvocation(this, "getPowerUps");
                m_comms.RegisterScriptInvocation(this, "mazeHasStarted");
                m_comms.RegisterScriptInvocation(this, "getAvatar");


                m_comms.RegisterScriptInvocation(this, "move_fwd");
                m_comms.RegisterScriptInvocation(this, "move_back");
                m_comms.RegisterScriptInvocation(this, "move_left");
                m_comms.RegisterScriptInvocation(this, "move_right");

                m_comms.RegisterScriptInvocation(this, "activate_powerup");


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

        private void teleportToStart(UUID player)
        {
            try
            {
                Player p = getPlayer(player);
                if (p == null) return;
                UUID checkPoint = LandmarkModule.getPlayerLandmark(p.getUUID());
                Landmark landmark = LandmarkModule.getLandmark(checkPoint);
                SceneObjectPart checkPointInstance = m_scene.GetSceneObjectPart(checkPoint);
                if (checkPointInstance == null) return;
                p.AddToPath(landmark.getStartPoint());

                List<string> toDeleteTimers = new List<string> { "move", "powerup" };
                foreach (KeyValuePair<string, Timer> timer in timerDictionary)
                {
                    if (timer.Key.Contains(p.getUUID().ToString()) && toDeleteTimers.Find(name => timer.Key.Contains(name)) != null)
                    {
                        timer.Value.Dispose();
                        timerDictionary.Remove(timer.Key);
                    }
                }

                SceneObjectGroup playerObj = m_scene.GetSceneObjectGroup(player);
                playerObj.TeleportObject(playerObj.UUID, checkPointInstance.AbsolutePosition + new Vector3(0, 0, objScale), Quaternion.Identity, 1);
                playerObj.RootPart.SetVelocity(new Vector3(0, 0, 0), true);
                //playerObj.RootPart.ParentGroup.MoveToTarget(checkPointInstance.AbsolutePosition - new Vector3(0, 0, checkPointInstance.Scale.Z * 2), 0.5f);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        #endregion

        #region ScriptInvocationInteface

        public void generateMaze(UUID hostID, UUID scriptID, int size, Vector3 start, UUID avatar)
        {
            try
            {
                avatarPlayer = avatar;
                Maze2D maze = new Maze2D(size, size);
                maze.printMaze();
                int[,] binaryMaze = new BinaryMaze2D(maze.getCells()).getCells();
                mazeSolver = new MazeSolver(binaryMaze);
                mazeSolver.Solve();
                LandmarkCreator creator = new LandmarkCreator(mazeSolver.getPath());
                generatePath(binaryMaze, size, start);

                LoadEndPointObject(endPoint);
                createBall(startPoint);
                createLandmarks(creator.getLandmarks(), mazeObjUUIDs);
                createPowerUps(mazeObjUUIDs, creator.getPointsOfInterest(), mazeSolver.getPath());
                createObstacles(mazeObjUUIDs);

                placedPathUUIDs = new UUID[size * 2 + 1, size * 2 + 1];
                CleanerModule.AddPlacedPath(placedPathUUIDs);
                createFloor(256, new Vector3(128, 128, getController().AbsolutePosition.Z - getController().Scale.Z - 2f));
                m_log.WarnFormat("[MazeMod] Generating maze: " + size.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void generateLevel(UUID hostID, UUID scriptID, int size, Vector3 start)
        {
            Maze2D maze = new Maze2D(size, size, defStartX: endPointPos[0] / 2);
            maze.printMaze();
            int[,] binaryMaze = new BinaryMaze2D(maze.getCells()).getCells();
            mazeSolver = new MazeSolver(binaryMaze);
            mazeSolver.Solve();
            LandmarkCreator creator = new LandmarkCreator(mazeSolver.getPath());
            generatePath(binaryMaze, size, start);

            LoadEndPointObject(endPoint);
            createLandmarks(creator.getLandmarks(), mazeObjUUIDs);
            createPowerUps(mazeObjUUIDs, creator.getPointsOfInterest(), mazeSolver.getPath());
            createObstacles(mazeObjUUIDs);
            placedPathUUIDs = new UUID[size * 2 + 1, size * 2 + 1];
            CleanerModule.AddPlacedPath(placedPathUUIDs);
            m_log.WarnFormat("[MazeMod] Generating maze: " + size.ToString());
        }


        private void generatePath(in int[,] binaryMaze, int size, Vector3 pos)
        {
            mazeObjUUIDs = new UUID[size * 2 + 1, size * 2 + 1];
            UUID hostID = getController().UUID;


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
                    startPointPos = new int[2] { x, 0 };
                }
            }
            for (int x = 0; x < size * 2 + 1; x++)
            {
                if (binaryMaze[x, size * 2] == 0)
                {
                    endPoint = mazeObjUUIDs[x, size * 2];
                    endPointPos = new int[2] { x, size * 2 };
                }
            }
            CleanerModule.AddPath(mazeObjUUIDs);
        }

        public UUID generateCube(UUID hostID, Vector3 pos)
        {
            try
            {
                SceneObjectPart obj = m_scene.GetSceneObjectPart(hostID);
                OpenMetaverse.UUID owner = obj.OwnerID;
                SceneObjectPart part = new SceneObjectPart(hostID, PrimitiveBaseShape.CreateBox(), pos, Quaternion.Identity, Vector3.Zero);
                TaskInventoryItem textureItem = getController().Inventory.GetInventoryItem("path_texture");
                Primitive.TextureEntry texture = new Primitive.TextureEntry(textureItem.AssetID);
                Primitive.TextureEntryFace face = texture.CreateFace(0);


                face.RGBA = new Color4(0.8f, 0.61f, 0.024f, 1f);
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
                    mazeController = sc.RootPart;
                }
            }
        }
        private SceneObjectPart getController()
        {
            if (mazeController == null) findController();
            return mazeController;
        }
        public UUID getStartPoint(UUID hostID, UUID scriptID)
        {
            return startPoint;
        }
        public UUID getEndPoint(UUID hostID, UUID scriptID)
        {
            return endPoint;
        }

        public int mazeHasStarted(UUID hostID, UUID scriptID)
        {
            return (startPoint != UUID.Zero) ? 1 : 0;
        }

        public UUID getAvatar(UUID hostID, UUID scriptID)
        {
            return avatarPlayer;
        }



        private void LoadEndPointObject(UUID objectUUID)
        {
            try
            {
                SceneObjectPart srcObject = getController();
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
                SceneObjectPart controller = getController();
                TaskInventoryItem item = controller.Inventory.GetInventoryItem("Ball");
                SceneObjectPart startPointObj = m_scene.GetSceneObjectPart(startPoint);
                List<SceneObjectGroup> newBall = m_scene.RezObject(controller, item, startPointObj.AbsolutePosition, null, Vector3.Zero, 0, false, false);
                mazeBallUUID = newBall[0].UUID;
                newBall[0].ResumeScripts();
                Player newPlayer = new Player(newBall[0].UUID, "player" + players.Count.ToString(), startPointPos);
                players.Add(newPlayer);

                //Create axes
                createAxisBall(newBall[0]);
                createAxisAttachment(avatarPlayer);

                //Add commands script
                TaskInventoryItem commands = controller.Inventory.GetInventoryItem("Commands");
                newBall[0].RootPart.ScriptAccessPin = 123;
                m_scene.RezScriptFromPrim(commands.ItemID, controller, mazeBallUUID, 123, 1, 0);

                TaskInventoryItem commandsScript = newBall[0].RootPart.Inventory.GetInventoryItem("Commands");
                commandsScript.OwnerID = avatarPlayer;
                newBall[0].InvalidateEffectivePerms();
                newBall[0].RootPart.ScheduleFullUpdate();


                //ScenePresence avatarObj = m_scene.GetScenePresence(avatarPlayer);
                //m_scene.Permissions.GenerateClientFlags(newBall[0].RootPart, avatarObj);
                //newBall[0].UpdatePermissions(avatarPlayer, 2, 0, 0, 0);
                //if (m_scene.TryGetClient(avatarPlayer, out IClientAPI client)) newBall[0].SendPropertiesToClient(client);
                // avatarObj.SendFullUpdateToClient(avatarObj.ControllingClient);

            }
            catch (Exception e)
            {
                m_log.WarnFormat("[MazeMod] Error loading ball: " + e.Message);
            }
        }

        private void createAxisBall(SceneObjectGroup ball)
        {
            SceneObjectPart controller = getController();
            if (ball == null || controller == null) return;
            TaskInventoryItem item = controller.Inventory.GetInventoryItem("Axis");
            if (item == null) return;
            List<SceneObjectGroup> newBallAxis = m_scene.RezObject(controller, item, ball.AbsolutePosition + new Vector3(0, 0, 3), null, Vector3.Zero, 0, false, false);
            newBallAxis[0].ResumeScripts();

            CleanerModule.AddItem(newBallAxis[0].UUID);
            string timerId = "ballAxisTimer" + ball.UUID.ToString();
            Timer ballAxisTimer = new Timer(delegate (object state)
            {
                SceneObjectGroup ax = m_scene.GetSceneObjectGroup(newBallAxis[0].UUID);
                if (ax == null)
                {
                    timerDictionary[timerId].Dispose();
                    timerDictionary.Remove(timerId);
                    return;
                }
                // Rotate opposite of cameraRotation

                ax.TeleportObject(ax.UUID, ball.AbsolutePosition + new Vector3(0, 0, 3), Quaternion.Identity, 1);
            }, null, 0, 80);
            timerDictionary.Add(timerId, ballAxisTimer);
        }

        private void createAxisAttachment(UUID avatar)
        {
            SceneObjectPart controller = getController();
            ScenePresence player = m_scene.GetScenePresence(avatar);
            if (player == null || controller == null) return;
            TaskInventoryItem item = controller.Inventory.GetInventoryItem("HUDAxis");
            if (item == null) return;
            List<SceneObjectGroup> newAttachment = m_scene.RezObject(controller, item, player.AbsolutePosition, null, Vector3.Zero, 0, false, false);
            newAttachment[0].ResumeScripts();
            newAttachment[0].SetOwnerId(player.UUID);
            AttachmentModule.attachToPlayer(player, newAttachment[0].RootPart, new Vector3(0f, -0.2f, 0.15f));

            string timerId = "axisRotationTimer" + player.UUID.ToString();
            Timer axisRotationTimer = new Timer(delegate (object state)
            {
                SceneObjectGroup attachment = m_scene.GetSceneObjectGroup(newAttachment[0].UUID);
                if (attachment == null)
                {
                    timerDictionary[timerId].Dispose();
                    timerDictionary.Remove(timerId);
                    return;
                }
                Quaternion cameraRotation = player.CameraRotation;
                // Rotate opposite of cameraRotation

                attachment.RootPart.UpdateRotation(Quaternion.Inverse(cameraRotation));
            }, null, 0, 100);
            timerDictionary.Add(timerId, axisRotationTimer);
        }

        private void createObstacles(UUID[,] map)
        {
            try
            {
                //ObstacleModule.AddAction("Obstacle1", )
                mazeObstacleUUIDs = new UUID[map.GetLength(0), map.GetLength(1)];
                for (int y = 2; y < map.GetLength(1) - 1; y++)
                {
                    for (int x = 1; x < map.GetLength(0); x++)
                    {
                        if (map[x, y] != UUID.Zero)
                        {
                            if (landmarkUUIDs[x, y] != UUID.Zero || mazePowerupsUUIDs[x, y] != UUID.Zero || mazeObstacleUUIDs[x - 1, y] != UUID.Zero || mazeObstacleUUIDs[x, y - 1] != UUID.Zero || random.Next(0, 20) != 0) continue;
                            SceneObjectPart controller = getController();
                            Obstacle randomObstacle = ObstacleModule.GetRandomObstacle();
                            if (!ObstacleModule.CanPlaceObstacle(randomObstacle.Name, new int[2] { x, y })) continue;
                            m_log.WarnFormat("[MazeMod] Creating obstacle: " + randomObstacle.Name);
                            TaskInventoryItem obstacleInvItem = controller.Inventory.GetInventoryItem(randomObstacle.Name);
                            SceneObjectPart spawnPoint = m_scene.GetSceneObjectPart(map[x, y]);
                            List<SceneObjectGroup> newObstacle = m_scene.RezObject(controller, obstacleInvItem, spawnPoint.AbsolutePosition + new Vector3(0, 0, spawnPoint.Scale.Z * 1.15f), null, Vector3.Zero, 0, false, false);
                            mazeObstacleUUIDs[x, y] = newObstacle[0].UUID;
                            newObstacle[0].ResumeScripts();
                            ObstacleModule.AddObject(newObstacle[0].UUID, randomObstacle.Name);
                        }
                    }
                }
                CleanerModule.AddObstacles(mazeObstacleUUIDs);
            }
            catch (Exception e)
            {
                m_log.WarnFormat("[MazeMod] Error loading ball: " + e.Message);
            }
        }

        private void createPowerUps(UUID[,] map, List<int[]> pointsOfInterest, List<int[]> path)
        {
            SceneObjectPart controller = getController();
            PowerUpModule.RegisterSpecialPowerUp("Jump");
            try
            {
                mazePowerupsUUIDs = new UUID[map.GetLength(0), map.GetLength(1)];
                for (int y = 2; y < map.GetLength(1) - 1; y++)
                {
                    for (int x = 1; x < map.GetLength(0); x++)
                    {
                        if (map[x, y] != UUID.Zero)
                        {
                            bool isPointOfInterest = false;
                            bool isInPath = false;
                            foreach (int[] array in pointsOfInterest)
                            {
                                if (array[0] == x && array[1] == y)
                                {
                                    isPointOfInterest = true;
                                    break;
                                }
                            }

                            foreach (int[] array in path)
                            {
                                if (array[0] == x && array[1] == y)
                                {
                                    isInPath = true;
                                    break;
                                }
                            }

                            if (!isPointOfInterest)
                            {
                                if (landmarkUUIDs[x, y] != UUID.Zero || isInPath || (mazePowerupsUUIDs[x - 1, y] != UUID.Zero || mazePowerupsUUIDs[x, y - 1] != UUID.Zero || random.Next(0, 20) != 0)) continue;
                            }

                            PowerUp randomPowerUp = PowerUpModule.GetRandomPowerUp();
                            TaskInventoryItem powerup = controller.Inventory.GetInventoryItem(randomPowerUp.Name);
                            SceneObjectPart spawnPoint = m_scene.GetSceneObjectPart(map[x, y]);
                            Vector3 spawnPos = spawnPoint.AbsolutePosition + new Vector3(0, 0, spawnPoint.Scale.Z * 1.15f);
                            List<SceneObjectGroup> newPowerup = m_scene.RezObject(controller, powerup, spawnPos, null, Vector3.Zero, 0, false, false);
                            mazePowerupsUUIDs[x, y] = newPowerup[0].UUID;
                            newPowerup[0].ResumeScripts();
                            float angle = (float)(random.NextDouble() * 2 * Math.PI);
                            newPowerup[0].RootPart.UpdateRotation(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, angle));
                            PowerUpModule.AddObject(newPowerup[0].UUID, randomPowerUp.Name);
                        }
                    }
                }
                PowerUp endPowerup = PowerUpModule.getPowerUp("Jump");
                TaskInventoryItem endPowerupInvItem = controller.Inventory.GetInventoryItem(endPowerup.Name);
                SceneObjectPart endPointInstance = m_scene.GetSceneObjectPart(endPoint);
                Vector3 endPos = endPointInstance.AbsolutePosition + new Vector3(0, 0, endPointInstance.Scale.Z * 1.15f);
                List<SceneObjectGroup> endPowerupInstance = m_scene.RezObject(controller, endPowerupInvItem, endPos, null, Vector3.Zero, 0, false, false);
                mazePowerupsUUIDs[endPointPos[0], endPointPos[1]] = endPowerupInstance[0].UUID;
                endPowerupInstance[0].ResumeScripts();
                PowerUpModule.AddObject(endPowerupInstance[0].UUID, endPowerup.Name);

                CleanerModule.AddPowerups(mazePowerupsUUIDs);


            }
            catch (Exception e)
            {
                m_log.WarnFormat("[MazeMod] Error loading powerups: " + e.Message);
            }
        }

        public void createLandmarks(List<Landmark> landmarks, UUID[,] map)
        {
            try
            {
                landmarkUUIDs = new UUID[map.GetLength(0), map.GetLength(1)];
                foreach (Landmark landmark in landmarks)
                {
                    SceneObjectPart controller = getController();
                    TaskInventoryItem item = controller.Inventory.GetInventoryItem("Flag");
                    SceneObjectPart spawnPoint = m_scene.GetSceneObjectPart(mazeObjUUIDs[landmark.getStartPoint()[0], landmark.getStartPoint()[1]]);
                    Vector3 spawnPos = spawnPoint.AbsolutePosition + new Vector3(0, 0, spawnPoint.Scale.Z);
                    List<SceneObjectGroup> newLandmark = m_scene.RezObject(controller, item, spawnPos, null, Vector3.Zero, 0, false, false);
                    newLandmark[0].ResumeScripts();
                    LandmarkModule.addLandmark(newLandmark[0].UUID, landmark);
                    landmarkUUIDs[landmark.getStartPoint()[0], landmark.getStartPoint()[1]] = newLandmark[0].UUID;
                }
                CleanerModule.AddLandmarks(landmarkUUIDs);
            }
            catch (Exception e)
            {
                m_log.WarnFormat("[MazeMod] Error creating landmark: " + e.Message);
            }
        }

        private void createFloor(int size, Vector3 startPoint)
        {
            SceneObjectPart controller = getController();
            TaskInventoryItem floor = controller.Inventory.GetInventoryItem("Floor");
            List<SceneObjectGroup> newFloor = m_scene.RezObject(controller, floor, startPoint, null, Vector3.Zero, 0, false, false);
            newFloor[0].GroupResize(new Vector3(size, size, 2));
            newFloor[0].ResumeScripts();
            floorUUID = newFloor[0].UUID;
        }

        private void deleteMaze()
        {
            try
            {
                CleanerModule.reset();
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

        private void buildPath(Player p, string direction, int length)
        {
            if (p == null) return;
            if (p.GetLastPos()[0] < 0 || p.GetLastPos()[0] > mazeObjUUIDs.GetLength(0)) return;
            if (p.GetLastPos()[1] < 0 || p.GetLastPos()[1] > mazeObjUUIDs.GetLength(1)) return;
            int[] pos = new int[2] { p.GetLastPos()[0], p.GetLastPos()[1] };
            for (int i = 0; i < length; i++)
            {
                if (direction == "forward") pos[1] += 1;
                else if (direction == "back") pos[1] -= 1;
                else if (direction == "right") pos[0] += 1;
                else if (direction == "left") pos[0] -= 1;
                Vector3 instancePos = m_scene.GetSceneObjectPart(mazeObjUUIDs[p.GetLastPos()[0], p.GetLastPos()[1]]).AbsolutePosition;
                if (pos[0] - p.GetLastPos()[0] != 0) instancePos.X += objScale * (pos[0] - p.GetLastPos()[0]);
                else if (pos[1] - p.GetLastPos()[1] != 0) instancePos.Y += objScale * (pos[1] - p.GetLastPos()[1]);
                placedPathUUIDs[pos[0], pos[1]] = generateCube(p.getUUID(), instancePos);
            }
        }

        public void obstacleCollision(UUID hostID, UUID scriptID, UUID player)
        {
            m_log.WarnFormat("[MazeMod] Ball collided with obstacle");
            Player p = getPlayer(player);
            if (p == null) return;
            ObstacleModule.OnCollision(hostID, getPlayer(player));
        }

        public int powerUpCollision(UUID hostID, UUID scriptID, UUID player)
        {
            m_log.WarnFormat("[MazeMod] Ball collided with powerup");
            Player p = getPlayer(player);
            if (p == null) return 0;
            PowerUp powerUp = PowerUpModule.AddPowerUp(hostID, p);
            AttachmentModule.attachToPlayer(p, powerUp.Name, avatarPlayer);
            return 1;
        }

        public int landMarkCollision(UUID hostID, UUID scriptID, UUID player)
        {
            try
            {
                m_log.WarnFormat("[MazeMod] Ball collided with powerup");
                Player p = getPlayer(player);
                if (p == null) return 0;
                LandmarkModule.addPlayerToLandmark(p.getUUID(), hostID);
                return 1;
            }
            catch (Exception e)
            {
                m_log.WarnFormat("[MazeMod] Error adding player to landmark: " + e.Message);
                return 0;
            }
        }

        public void activate_powerup(UUID hostID, UUID scriptID, string powerup, object[] args)
        {
            try
            {
                Player p = getPlayer(hostID);
                if (p == null) return;
                PowerUp activatedPowerup = p.ActivatePowerUp(powerup, args);
                if (activatedPowerup == null) return;
                AttachmentModule.removeAttachment(p, powerup);
                string timerId = p.getUUID().ToString() + "powerup" + powerup;

                if (timerDictionary.ContainsKey(timerId))
                {
                    Timer timerToRemove = timerDictionary[timerId];
                    timerToRemove.Dispose();
                    timerDictionary.Remove(timerId);

                }

                Timer timer = new Timer((object state) =>
                {
                    p.RemovePowerUp(powerup);
                    if (timerDictionary.ContainsKey(timerId))
                    {
                        Timer timerToRemove = timerDictionary[timerId];
                        timerToRemove.Dispose();
                        timerDictionary.Remove(timerId);
                    }
                }, null, PowerUpModule.getPowerUp(powerup).Duration, Timeout.Infinite);
                timerDictionary.Add(timerId, timer);
            }
            catch (Exception e)
            {
                m_log.WarnFormat("[MazeMod] Error consuming powerup: " + e.Message);
            }
        }
        public void floorCollision(UUID hostID, UUID scriptID, UUID player)
        {
            m_log.WarnFormat("[MazeMod] Ball collided with obstacle");
            teleportToStart(player);
        }

        public int endPointCollision(UUID hostID, UUID scriptID, UUID player)
        {
            m_log.WarnFormat("[MazeMod] Ball collided with endpoint");
            Player p = getPlayer(player);
            if (p == null) return 0;
            // SceneObjectGroup playerObj = m_scene.GetSceneObjectGroup(player);
            // playerObj.ScriptSetPhantomStatus(true);
            // playerObj.ScriptSetPhysicsStatus(false);
            // Primitive.TextureEntry texture = playerObj.RootPart.Shape.Textures;
            // Primitive.TextureEntryFace face = texture.CreateFace(0);
            // face.Glow = 0.3f;
            // face.RGBA = new Color4(0, 0.5f, 1f, 0.3f);
            // texture.FaceTextures[0] = face;
            // playerObj.TeleportObject(player, playerObj.AbsolutePosition + new Vector3(0, 0, 3), Quaternion.Identity, 1);
            // playerObj.RootPart.UpdateTextureEntry(texture);
            SceneObjectPart endPointObj = m_scene.GetSceneObjectPart(endPoint);
            if (endPointObj == null) return 0;
            SceneObjectPart startPointObj = getController();
            if (startPointObj == null) return 0;

            Vector3 newStartPointPos = endPointObj.AbsolutePosition + new Vector3(0, 0, 4);
            newStartPointPos.Y = endPointObj.AbsolutePosition.Y + objScale;
            newStartPointPos.X = startPointObj.AbsolutePosition.X + START_OFFSET.X;
            generateLevel(UUID.Zero, UUID.Zero, Convert.ToInt32(Math.Round(mazeObjUUIDs.GetLength(0) * 0.75f)), newStartPointPos);
            return 1;

        }

        public void movePlayer(UUID hostID, UUID scriptID, Vector3 dist)
        {
            try
            {
                Console.WriteLine("Called movePlayer with " + dist.ToString());
                if (dist.X == 0 && dist.Y == 0 && dist.Z == 0) return;
                Player p = getPlayer(hostID);
                if (p == null) return;
                int[] newMove = new int[2] { p.GetLastPos()[0] + (int)dist.X, p.GetLastPos()[1] + (int)dist.Y };
                p.AddToPath(newMove);

                SceneObjectPart playerObj = m_scene.GetSceneObjectPart(hostID);
                Vector3 target = playerObj.AbsolutePosition + new Vector3(dist.X * objScale, dist.Y * objScale, 0);
                float speed = objScale * 1.05f;
                Vector3 directionVector = new Vector3(dist.X != 0 ? Math.Sign(dist.X) : 0, dist.Y != 0 ? Math.Sign(dist.Y) : 0, 0);
                Vector3 moveVector = directionVector * speed;
                int MOVE_CHECK_INTERVAL = 50;
                float MOVE_CHECK_THRESHOLD = 0.15f;

                playerObj.SetVelocity(moveVector, false);
                playerObj.UpdateAngularVelocity(directionVector * PI);

                Console.WriteLine("Moving player toooooooooooooo " + target.ToString());
                string timerId = p.getUUID().ToString() + "move";
                Timer timer = new Timer((object state) =>
                {
                    Console.WriteLine("Checking player position");
                    if (Vector3.Distance(playerObj.AbsolutePosition, target) < MOVE_CHECK_THRESHOLD)
                    {
                        playerObj.SetVelocity(Vector3.Zero, false);
                        playerObj.ParentGroup.UpdateGroupPosition(target);

                        if (timerDictionary.ContainsKey(timerId))
                        {
                            Timer timerToRemove = timerDictionary[timerId];
                            timerToRemove.Dispose();
                            timerDictionary.Remove(timerId);
                            Console.WriteLine(timerDictionary.Count.ToString());
                        }
                    }
                    else if (playerObj.Velocity == Vector3.Zero)
                    {
                        Console.WriteLine("Setting velocity: " + moveVector.ToString());
                        playerObj.SetVelocity(moveVector, false);
                    }
                }, null, MOVE_CHECK_INTERVAL, MOVE_CHECK_INTERVAL);

                timerDictionary.Add(timerId, timer);


                m_log.WarnFormat("[MazeMod] Player moved, last pos " + p.GetLastPos()[0] + ", " + p.GetLastPos()[1]);
            }
            catch (Exception e)
            {
                m_log.WarnFormat("[MazeMod] Error moving player: " + e.Message);
            }
        }

        public object[] getPowerUps(UUID hostID, UUID scriptID, UUID playerID)
        {
            Player p = getPlayer(playerID);
            if (p == null) return new object[0];
            object[] powerUps = p.GetInventory();
            return powerUps;
        }
        public int getArenaCommChannel(UUID hostID, UUID scriptID)
        {
            return commChannel;
        }

        public void resetMaze(UUID hostID, UUID scriptID)
        {
            deleteMaze();
            foreach (Player p in players)
            {
                p.Reset();
                AttachmentModule.Reset();
            }
            foreach (Timer timer in timerDictionary.Values)
            {

                timer.Dispose();
            }
            timerDictionary.Clear();
            startPoint = UUID.Zero;
            endPoint = UUID.Zero;
        }





        public void move_fwd(UUID hostID, UUID scriptID, int dist)
        {
            movePlayer(hostID, scriptID, new Vector3(0, dist, 0));
        }

        public void move_back(UUID hostID, UUID scriptID, int dist)
        {
            movePlayer(hostID, scriptID, new Vector3(0, -dist, 0));
        }

        public void move_left(UUID hostID, UUID scriptID, int dist)
        {
            movePlayer(hostID, scriptID, new Vector3(-dist, 0, 0));
        }

        public void move_right(UUID hostID, UUID scriptID, int dist)
        {
            movePlayer(hostID, scriptID, new Vector3(dist, 0, 0));
        }

        #endregion
    }
}