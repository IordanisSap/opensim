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

class LandmarkModule
{
    private Dictionary<UUID, Landmark> landmarkMap;
    private List<UUID> landmarkOrder;
    private Dictionary<UUID, UUID> playerLandmark;

    private Scene scene;

    public LandmarkModule(Scene scene)
    {
        this.scene = scene;
        landmarkMap = new Dictionary<UUID, Landmark>();
        playerLandmark = new Dictionary<UUID, UUID>();
        landmarkOrder = new List<UUID>();
    }

    public void addLandmark(UUID landmarkUUID, Landmark landmark)
    {
        landmarkMap.Add(landmarkUUID, landmark);
        landmarkOrder.Add(landmarkUUID);
    }

    public void addPlayerToLandmark(UUID player, UUID landmarkUUID)
    {
        if (playerLandmark.ContainsKey(player))
            playerLandmark.Remove(player);
        playerLandmark.Add(player, landmarkUUID);
    }

    public void reset(){
        landmarkMap.Clear();
        playerLandmark.Clear();
    }

    public Landmark getLandmark(UUID landmarkUUID)
    {
        return landmarkMap[landmarkUUID];
    }

    public UUID getPlayerLandmark(UUID player)
    {
        return playerLandmark[player];
    }
}