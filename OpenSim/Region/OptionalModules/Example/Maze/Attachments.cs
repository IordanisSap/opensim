
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

public class AttachmentModule
{
    private UUID HUDContainer = UUID.Zero;
    private string HUDContainerName = "MazeController";
    IAttachmentsModule attachmentsModule = null;

    private Scene m_scene;

    private Dictionary<Player, List<PowerUpAttachment>> displayPowerUps = new Dictionary<Player, List<PowerUpAttachment>>();

    private class PowerUpAttachment
    {
        public string powerUpName;
        public UUID powerUpUUID;
        public PowerUpAttachment(string powerUpName, UUID powerUp)
        {
            this.powerUpName = powerUpName;
            this.powerUpUUID = powerUp;
        }
    }
    private UUID getHUDContainer()
    {
        if (HUDContainer == UUID.Zero) findHUDContainer(m_scene);
        return HUDContainer;
    }

    public AttachmentModule(Scene scene)
    {
        m_scene = scene;
        attachmentsModule = m_scene.RequestModuleInterface<IAttachmentsModule>();
    }
    private void findHUDContainer(Scene scene)
    {
        List<SceneObjectGroup> sceneObjects = scene.GetSceneObjectGroups();
        foreach (SceneObjectGroup sc in sceneObjects)
        {
            if (HUDContainerName.Equals(sc.Name))
            {
                HUDContainer = sc.UUID;
            }
        }
    }

    public void attachToPlayer(Player player, string powerUpName, UUID avatar)
    {
        try
        {
            SceneObjectPart playerObj = m_scene.GetSceneObjectPart(player.getUUID());
            ScenePresence objOwner = m_scene.GetScenePresence(avatar);
            SceneObjectPart HUDobj = m_scene.GetSceneObjectPart(getHUDContainer());
            Console.WriteLine(objOwner.Name);
            float displayOffset = -0.5f;
            if (objOwner != null)
            {
                displayOffset -= 0.1f * player.GetInventory().Length;
                TaskInventoryItem powerUp = HUDobj.Inventory.GetInventoryItem("HUD" + powerUpName);
                if (powerUp == null) return;
                List<SceneObjectGroup> newPowerup = m_scene.RezObject(HUDobj, powerUp, objOwner.AbsolutePosition, null, Vector3.Zero, 0, false, false);
                newPowerup[0].ResumeScripts();
                newPowerup[0].SetOwnerId(avatar);
                attachmentsModule.AttachObject(objOwner, newPowerup[0], (uint)AttachmentPoint.HUDBottomLeft, false, false, true);
                newPowerup[0].UpdateGroupPosition(new Vector3(0f, displayOffset, 0.07f));
                Console.WriteLine("Success:"+ objOwner.UUID +",  " + newPowerup[0].OwnerID);
                if (displayPowerUps.ContainsKey(player))
                {
                    displayPowerUps[player].Add(new PowerUpAttachment(powerUpName, newPowerup[0].UUID));
                }
                else
                {
                    displayPowerUps.Add(player, new List<PowerUpAttachment>());
                    displayPowerUps[player].Add(new PowerUpAttachment(powerUpName, newPowerup[0].UUID));
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public void removeAttachment(Player player, string powerUpName)
    {
        List<PowerUpAttachment> powerUps = displayPowerUps[player];
        int flag = -1;
        for (int i = powerUps.Count - 1; i >= 0; i--)
        {
            if (powerUps[i].powerUpName.Equals(powerUpName))
            {
                SceneObjectPart toRemovePowerUp = m_scene.GetSceneObjectPart(powerUps[i].powerUpUUID);
                if (toRemovePowerUp != null)
                {
                    toRemovePowerUp.ParentGroup.DetachToGround();
                    toRemovePowerUp.ParentGroup.DeleteGroupFromScene(false);
                }
                powerUps.RemoveAt(i);
                flag = i;
                i--;
                break;
            }
        }
        for (int i = powerUps.Count - 1; i >= flag; i--){
            SceneObjectPart toMovePowerUp = m_scene.GetSceneObjectPart(powerUps[i].powerUpUUID);
            toMovePowerUp.ParentGroup.UpdateGroupPosition(toMovePowerUp.AttachedPos + new Vector3(0f, 0.1f, 0f));
        }
    }

    public void Reset(){
        foreach (KeyValuePair<Player, List<PowerUpAttachment>> entry in displayPowerUps)
        {
            foreach (PowerUpAttachment powerUp in entry.Value)
            {
                SceneObjectPart toRemovePowerUp = m_scene.GetSceneObjectPart(powerUp.powerUpUUID);
                if (toRemovePowerUp != null)
                {
                    toRemovePowerUp.ParentGroup.DetachToGround();
                    toRemovePowerUp.ParentGroup.DeleteGroupFromScene(false);
                }
            }
            entry.Value.Clear();
        }
        displayPowerUps.Clear();
    }

}
