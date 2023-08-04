
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


public class Landmark
{
    private int[] startPoint;
    private int[] endPoint;

    public Landmark(int[] startPoint, int[] endPoint)
    {
        this.startPoint = startPoint;
        this.endPoint = endPoint;
    }
}

public class LandmarkModule
{
    private Dictionary<UUID, Landmark> landmarkMap;

    public LandmarkModule()
    {
        landmarkMap = new Dictionary<UUID, Landmark>();
    }

    public void AddLandmark(UUID uuid, Landmark landmark)
    {
        landmarkMap.Add(uuid, landmark);
    }

    public Landmark GetLandmark(UUID uuid)
    {
        return landmarkMap[uuid];
    }

    public void RemoveLandmark(UUID uuid)
    {
        landmarkMap.Remove(uuid);
    }
}