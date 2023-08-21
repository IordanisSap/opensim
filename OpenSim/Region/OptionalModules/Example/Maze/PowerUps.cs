

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

public class PowerUp
{
    public delegate void Callback(Player player, object[] data = null);
    public Callback ActivateCallback { get; set; }
    public Callback DeactivateCallback { get; set; }

    public string Name { get; set; }

    public int Duration { get; set; }

    public PowerUp(string name, int duration, Callback onActivate, Callback onDeactivate)
    {
        this.Name = name;
        this.Duration = duration;
        this.ActivateCallback = onActivate;
        this.DeactivateCallback = onDeactivate;
    }

    public void Activate(Player player, object[] data)
    {
        if (ActivateCallback != null)
        {
            ActivateCallback(player, data);
        }
    }

    public void Deactivate(Player player)
    {
        if (DeactivateCallback != null)
        {
            DeactivateCallback(player);
        }
    }
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        PowerUp other = (PowerUp)obj;
        return this.Name == other.Name;
    }
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}



public class PowerUpModule
{
    private Dictionary<UUID, string> objectMap;
    private Dictionary<string, PowerUp> actionMap;

    private List<string> specialPowerups = new List<string>();
    private Random random = new Random();

    public PowerUpModule()
    {
        objectMap = new Dictionary<UUID, string>();
        actionMap = new Dictionary<string, PowerUp>();
    }

    public PowerUp GetRandomPowerUp()
    {
        if (actionMap.Count == 0)
        {
            return null;
        }
        int randomIndex = random.Next(0, actionMap.Count);
        PowerUp randomPowerUp = actionMap.ElementAt(randomIndex).Value;
        if (specialPowerups.Contains(randomPowerUp.Name))
        {
            return GetRandomPowerUp();
        }
        return randomPowerUp;
    }
    private PowerUp GetNotRandomPowerUp(){
        PowerUp p = getPowerUp("Random");
        while(p.Name == "Random"){
            p = GetRandomPowerUp();
        }
        return p;
    }
    public void AddPowerUp(PowerUp powerUp)
    {
        actionMap.Add(powerUp.Name, powerUp);
    }
    public void AddObject(UUID uuid, string powerUpName)
    {
        objectMap.Add(uuid, powerUpName);
    }

    public void RegisterSpecialPowerUp(string powerUp)
    {
        specialPowerups.Add(powerUp);
    }

    public PowerUp AddPowerUp(UUID powerUpUUID, Player player)
    {
        if (!objectMap.TryGetValue(powerUpUUID, out string powerUpName)) return null;
        PowerUp powerUp = actionMap[powerUpName];
        if (powerUp.Name == "Random")
        {
            powerUp = GetNotRandomPowerUp();
        }
        player.AddPowerUp(powerUp);
        return powerUp;
    }

    public PowerUp getPowerUp(UUID powerUpUUID)
    {
        if (!objectMap.TryGetValue(powerUpUUID, out string powerUpName)) return null;
        return actionMap[powerUpName];
    }

    public PowerUp getPowerUp(string powerUpName)
    {
        return actionMap[powerUpName];
    }
}
