integer npcCount = 0;
integer listen_handle = 0;
integer modeMaxNPCs = 0;
list ReadyNPCs = [];
list NPCs = [];
list NPCguns = [];

integer cleanedUp = FALSE;
string displayText = "Waiting for players...";


init(){
    npcCount = 0;
    listen_handle = 0;
    ReadyNPCs = [];
    NPCs = [];
    NPCguns = [];
    string displayText = "Waiting for players...";
}

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


cleanUp()
{
    integer i = 0;
    integer size = llGetListLength(NPCs);
    while (i < size)
    {
        if (llGetAgentSize(llList2Key(NPCs,i))) osNpcRemove(llList2Key(NPCs,i));
        llOwnerSay("NPC removed");
        llOwnerSay(llList2Key(NPCs,i));
        i++;
    }
}

state setup
{
    state_entry()
    {
        llRegionSay(getArenaCommChannel() + 1, displayText);
        llOwnerSay("Setup");
        modeMaxNPCs = getModePlayerNum();
        listen_handle = llListen(getArenaCommChannel(), "", "", "");
    }
    listen(integer channel, string name, key id, string message)
    {
        list command = llParseStringKeepNulls(message, "+", "");
        if(llList2String(command,0) != "SetupGun"){ return;}
        if (cleanedUp == FALSE)
        {
            cleanUp();
            cleanedUp = TRUE;
            init();
        }

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
                    state wait;
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

state wait
{
    state_entry()
    {
        llOwnerSay("Wait");
        llRegionSay(getArenaCommChannel() + 1, llList2Key(NPCs,0) + "+"+ llList2Key(NPCs,1));
        listen_handle = llListen(getArenaCommChannel(), "Router", "", "");
    }
    listen(integer channel, string name, key id, string message)
    {
        if (llListFindList(ReadyNPCs, [id]) == -1)
        {
            ReadyNPCs += [id];
            llOwnerSay("NPC ready");
            if (llGetListLength(ReadyNPCs) >= modeMaxNPCs)
            {
                llOwnerSay("All NPCs ready");
                llListenRemove(listen_handle);
                state main;
            }
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
        if (!llGetAgentSize(npc)) {
            displayText = llKey2Name(getOtherIndex(npc)) + " wins";
            reset();
            cleanedUp = FALSE;
            state setup;
        }
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

key getOtherIndex(key npc)
{
    if (llList2Key(NPCs,0) == npc) return llList2Key(NPCs,1);
    return llList2Key(NPCs,0);
}