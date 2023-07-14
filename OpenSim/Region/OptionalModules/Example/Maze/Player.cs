
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

    public void AddPowerUp(PowerUp powerUp)
    {
        powerUps.Add(powerUp);
    }

    public void OnCollision(Obstacle obstacle)
    {
        obstacle.OnCollision(this);
    }
}