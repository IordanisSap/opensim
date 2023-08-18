
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

    public Landmark(int[] startPoint)
    {
        this.startPoint = startPoint;
    }

    public void Print()
    {
        Console.WriteLine("Landmark: [" + startPoint[0] + "," + startPoint[1] + "]");
    }

    public int[] getStartPoint()
    {
        return startPoint;
    }
}
