

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
using System.Linq;


public class CleanerModule
{

    private List<UUID[,]> pathUUIDs = null;

    private List<UUID[,]> obstacleUUIDs = null;

    private List<UUID[,]> powerupUUIDs = null;

    private List<UUID[,]> landmarkUUIDs = null;

    private List<UUID[,]> placedPathUUIDs = null;

    private List<UUID> items = null;
    private Scene m_scene;

    public CleanerModule(Scene m_scene)
    {
        pathUUIDs = new List<UUID[,]>();
        obstacleUUIDs = new List<UUID[,]>();
        powerupUUIDs = new List<UUID[,]>();
        landmarkUUIDs = new List<UUID[,]>();
        placedPathUUIDs = new List<UUID[,]>();
        items = new List<UUID>();
        this.m_scene = m_scene;
    }

    public void AddPath(UUID[,] pathUUIDs)
    {
        this.pathUUIDs.Add(pathUUIDs);
    }

    public void AddObstacles(UUID[,] obstacleUUIDs)
    {
        this.obstacleUUIDs.Add(obstacleUUIDs);
    }

    public void AddPowerups(UUID[,] powerupUUIDs)
    {
        this.powerupUUIDs.Add(powerupUUIDs);
    }

    public void AddLandmarks(UUID[,] landmarkUUIDs)
    {
        this.landmarkUUIDs.Add(landmarkUUIDs);
    }

    public void AddPlacedPath(UUID[,] placedPathUUID)
    {
        this.placedPathUUIDs.Add(placedPathUUID);
    }

    public void AddItem(UUID item)
    {
        items.Add(item);
    }

    public void reset()
    {
        for (int level = 0; level < pathUUIDs.Count; level++)
        {
            for (int y = 0; y < pathUUIDs[level].GetLength(1); y++)
            {
                for (int x = 0; x < pathUUIDs[level].GetLength(0); x++)
                {
                    if (pathUUIDs[level][x, y] != null)
                    {
                        SceneObjectGroup obj = m_scene.GetSceneObjectGroup(pathUUIDs[level][x, y]);
                        if (obj != null) m_scene.DeleteSceneObject(obj, false);

                        SceneObjectGroup obj2 = m_scene.GetSceneObjectGroup(obstacleUUIDs[level][x, y]);
                        if (obj2 != null) m_scene.DeleteSceneObject(obj2, false);

                        SceneObjectGroup obj3 = m_scene.GetSceneObjectGroup(powerupUUIDs[level][x, y]);
                        if (obj3 != null) m_scene.DeleteSceneObject(obj3, false);

                        SceneObjectGroup obj4 = m_scene.GetSceneObjectGroup(landmarkUUIDs[level][x, y]);
                        if (obj4 != null) m_scene.DeleteSceneObject(obj4, false);

                        SceneObjectGroup obj5 = m_scene.GetSceneObjectGroup(placedPathUUIDs[level][x, y]);
                        if (obj5 != null) m_scene.DeleteSceneObject(obj5, false);
                    }
                }
            }
        }
        foreach (UUID item in items)
        {
            SceneObjectGroup obj = m_scene.GetSceneObjectGroup(item);
            if (obj != null) m_scene.DeleteSceneObject(obj, false);
        }
        pathUUIDs.Clear();
        obstacleUUIDs.Clear();
        powerupUUIDs.Clear();
        landmarkUUIDs.Clear();
        placedPathUUIDs.Clear();
    }

}