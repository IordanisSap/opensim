
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
using System.Linq;


public class Obstacle
{
    public delegate void Callback(Player player);
    public Callback OnCollisionCallback { get; set; }

    public delegate bool PlaceCondition(int[] pos);

    public PlaceCondition PlaceConditionCallback { get; set; }

    public PlaceCondition OnPlaceCallback { get; set; }
    public string Name { get; set; }
    public Obstacle(string name, Callback onCollision, PlaceCondition placeCondition, PlaceCondition placeCallback = null)
    {
        this.Name = name;
        this.OnCollisionCallback = onCollision;
        this.PlaceConditionCallback = placeCondition;
        this.OnPlaceCallback = placeCallback;
    }
    public void OnCollision(Player player)
    {
        // Invoke the CollisionCallback delegate
        if (OnCollisionCallback != null)
        {
            OnCollisionCallback(player);
        }
    }

    public void onPlace(int[] data)
    {
        if (OnPlaceCallback != null)
        {
            OnPlaceCallback(data);
        }
    }
}


public class ObstacleModule
{
    private Dictionary<UUID, string> objectMap;
    private Dictionary<string, Obstacle> actionMap;

    private Random random = new Random();

    public ObstacleModule()
    {
        objectMap = new Dictionary<UUID, string>();
        actionMap = new Dictionary<string, Obstacle>();
    }

    public Obstacle GetRandomObstacle()
    {
        Console.WriteLine("actionMap.Count: " + actionMap.Count);
        if (actionMap.Count == 0)
        {
            return null;
        }
        int randomIndex = random.Next(0, actionMap.Count);
        Obstacle randomObstacle = actionMap.ElementAt(random.Next(0, actionMap.Count)).Value;
        return randomObstacle;
    }

    public bool CanPlaceObstacle(string obstacleName, int[] pos)
    {
        if (!actionMap.TryGetValue(obstacleName, out Obstacle obstacle)) return false;
        return obstacle.PlaceConditionCallback(pos);
    }
        
    public void AddObstacle(Obstacle obstacle)
    {
        actionMap.Add(obstacle.Name, obstacle);
    }
    public void AddObject(UUID uuid, string obstacleName)
    {
        objectMap.Add(uuid, obstacleName);
    }

    public void OnCollision(UUID obstacleUUID, Player player)
    {
        if (!objectMap.TryGetValue(obstacleUUID, out string obstacleType)) return;
        Obstacle obstacle = actionMap[obstacleType];
        obstacle.OnCollision(player);
    }
    public Obstacle GetObstacle(string obstacleName)
    {
        if (!actionMap.TryGetValue(obstacleName, out Obstacle obstacle)) return null;
        return obstacle;
    }
    
    public Obstacle GetObstacle(UUID obstacleUUID)
    {
        if (!objectMap.TryGetValue(obstacleUUID, out string obstacleName)) return null;
        return GetObstacle(obstacleName);
    }
}