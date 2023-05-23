integer npcCount = 0;
integer listen_handle = 0;
integer modeMaxNPCs = 0;
list NPCs = [];
list NPCguns = [];


integer isAlreadyRegistered(key npc)
{
    integer i = 0;
    while (i < npcCount)
    {
        if (llList2Key(NPCs,i) == npc) return TRUE;
        i++;
    }
    return FALSE;
}

state setup
{
    state_entry()
    {
        llOwnerSay("Setup");
        modeMaxNPCs = getModePlayerNum();
        listen_handle = llListen(getArenaCommChannel(), "", "", "");
    }
    listen(integer channel, string name, key id, string message)
    {
        list command = llParseStringKeepNulls(message, "+", "");
        integer length = llGetListLength(command);
        if (llList2String(command,0)  == "SetupGun")
        {
            if (isAlreadyRegistered(llList2Key(command,1)))
            {
                llOwnerSay("NPC already registered");
            }else{
                npcCount++;
                NPCs += llList2Key(command,1);
                NPCguns += llList2Key(command,2);
                osNpcTouch(llList2Key(command,1), llList2Key(command,2), LINK_THIS);

                llRegionSayTo(id, getArenaCommChannel(), "ok");
                if (npcCount >= modeMaxNPCs)
                {
                    llOwnerSay("Setup complete");
                    llListenRemove(listen_handle);
                    state main;
                }
            }

        }
        else llOwnerSay("Unknown command");
    }
    changed(integer change)
    {
        if (change & CHANGED_REGION_START) 
        {
            llOwnerSay("Region changed");
            llListenRemove(listen_handle);
            llResetScript();
        }
    }
}


state main
{
    state_entry()
    {
        listen_handle = llListen(getArenaCommChannel(), "Router", "", "");
    }
    listen(integer channel, string name, key id, string message)
    {
        list command = llParseStringKeepNulls(message, "+", "-");
        integer length = llGetListLength(command);
        key npc = getPlayerNPC(llGetOwnerKey(id));
        if (llList2String(command,0)  == "Move")
        {
            vector targetPos = (vector) llList2String(command,1);
            osNpcMoveToTarget(npc ,targetPos, OS_NPC_FLY);
        }
        else if (llList2String(command,0)  == "Shoot")
        {
            integer npcGunIndex = getIndex(npc);
            if (npcGunIndex >= 0) osNpcTouch(npc, llList2Key(NPCguns,npcGunIndex), LINK_THIS);
            else llOwnerSay("NPCgun or npc not found");
        }
        else if (llList2String(command,0) == "LookAt")
        {
            vector start = llRot2Fwd(ZERO_ROTATION); //Object's X axis is forward
            vector targetPos = (vector) llList2String(command,1);
            vector end = llVecNorm(targetPos - osNpcGetPos(npc));
            osNpcSetRot(npc, llRotBetween(start,end)); //Set the prim's rotation accordingly.
        }
    }
    changed(integer change)
    {
        if (change & CHANGED_REGION_START) 
        {
            llOwnerSay("Region changed");
            llListenRemove(listen_handle);
            llResetScript();
        }
    }
}


default
{
    state_entry()
    {
        state setup;
    }
}

integer getIndex(key npc)
{
    integer i = 0;
    while (i < npcCount)
    {
        if (llList2Key(NPCs,i) == npc) return i;
        i++;
    }
    llOwnerSay("returning -1" );
    return -1;
}