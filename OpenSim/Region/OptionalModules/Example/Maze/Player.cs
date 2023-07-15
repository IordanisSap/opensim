
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

    List<PowerUp> powerUps = new List<PowerUp>();
    public Player(UUID uuid, string name)
    {
        this.uuid = uuid;
        this.name = name;
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
        foreach (PowerUp powerUp in powerUps)
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
        powerUps.Add(powerUp);
        powerUp.Activate(this);
    }

    public void RemovePowerUp(string powerUp)
    {
        foreach (PowerUp p in powerUps)
        {
            if (p.Name == powerUp)
            {
                powerUps.Remove(p);
                p.Deactivate(this);
                return;
            }
        }
    }
}