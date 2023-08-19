
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
    public Callback CollisionCallback { get; set; }

    public delegate bool PlaceCondition(int[] pos);

    public PlaceCondition PlaceConditionCallback { get; set; }
    public string Name { get; set; }
    public Obstacle(string name, Callback onCollision, PlaceCondition placeCondition)
    {
        this.Name = name;
        this.CollisionCallback = onCollision;
        this.PlaceConditionCallback = placeCondition;
    }
    public void OnCollision(Player player)
    {
        // Invoke the CollisionCallback delegate
        if (CollisionCallback != null)
        {
            CollisionCallback(player);
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
}