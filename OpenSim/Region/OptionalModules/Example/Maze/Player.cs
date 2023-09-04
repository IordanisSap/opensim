
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

public class Player
{
    private UUID uuid;
    private string name;

    List<PowerUp> invPowerUps = new List<PowerUp>();

    List<PowerUp> activePowerUps = new List<PowerUp>();

    List<int[]> path = new List<int[]>();
    int[] lastPos;
    public Player(UUID uuid, string name, int[] startPos)
    {
        this.uuid = uuid;
        this.name = name;
        lastPos = startPos;
    }

    public UUID getUUID()
    {
        return uuid;
    }

    public string getName()
    {
        return name;
    }

    public bool hasPowerUp(string name)
    {
        foreach (PowerUp powerUp in activePowerUps)
        {
            if (powerUp.Name == name)
            {
                return true;
            }
        }
        return false;
    }
    public void AddPowerUp(PowerUp powerUp)
    {
        invPowerUps.Add(powerUp);
    }
    public PowerUp ActivatePowerUp(string powerUp, object[] args)
    {
        foreach (PowerUp p in invPowerUps)
        {
            if (p.Name == powerUp)
            {
                invPowerUps.Remove(p);
                activePowerUps.Add(p);
                p.Activate(this, args);
                return p;
            }
        }
        Console.WriteLine("PowerUp not found");
        return null;
    }

    public void RemovePowerUp(string powerUp)
    {
        foreach (PowerUp p in activePowerUps)
        {
            if (p.Name == powerUp)
            {
                activePowerUps.Remove(p);
                p.Deactivate(this);
                return;
            }
        }
    }
    public void AddToPath(int[] position)
    {
        lastPos = position;
        path.Add(position);
    }

    public string[] GetInventory()
    {
        string[] inventory = new string[invPowerUps.Count];
        for (int i = 0; i < invPowerUps.Count; i++)
        {
            inventory[i] = invPowerUps[i].Name;
        }
        return inventory;
    }

    public int[] GetLastPos()
    {
        return lastPos;
    }

    public List<PowerUp> GetInvPowerUps()
    {
        return invPowerUps;
    }
    public List<PowerUp> GetActivePowerUps()
    {
        return activePowerUps;
    }
    
    public void Reset(){
        invPowerUps.Clear();
        activePowerUps.Clear();
        path.Clear();
    }

}