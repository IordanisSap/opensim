integer listen_handle;
key npcKey;


setup_anim()
{
    string FLY_STATIC_ANIM = llGetInventoryName(INVENTORY_ANIMATION, 0);
    string FLY_FORWARD_ANIM = llGetInventoryName(INVENTORY_ANIMATION, 1);
    osNpcSetAnimationOverride(npcKey, "Flying", FLY_FORWARD_ANIM);
    osNpcSetAnimationOverride(npcKey, "Hovering", FLY_STATIC_ANIM);
    osNpcSetAnimationOverride(npcKey, "Flying Up", FLY_STATIC_ANIM);
    osNpcSetAnimationOverride(npcKey, "Flying Down", FLY_STATIC_ANIM);
    osNpcSetAnimationOverride(npcKey, "Falling", FLY_STATIC_ANIM);
    osNpcSetAnimationOverride(npcKey, "Standing", FLY_STATIC_ANIM);
    osNpcSetAnimationOverride(npcKey, "Walking", FLY_STATIC_ANIM);
}

spawn(string name){
    
    vector v = llRot2Fwd( llGetLocalRot() );
    npcKey = osNpcCreate(name, "drone", llGetPos() + v * 25 + <0,0,2>, llGetInventoryKey("npc"));
    osSetSize(npcKey, <3.5,3.1,0.8>);
    setup_anim();
    osSetSpeed(npcKey, 1);
    osNpcSetRot(npcKey, <0.000000, 0.000000, llGetRot().z, 0.000000>);
    setNPC(npcKey);
    vector v = llRot2Fwd( osNpcGetRot(npcKey));
    osNpcMoveToTarget(npcKey, osNpcGetPos(npcKey) + v * 10 + <0,0,3>, OS_NPC_FLY);
    completeSetup();
    llSetTimerEvent(5.0);
}

default
{
    state_entry()
    {
        llSay(0,llGetObjectName() + "ArenaMod");
        listen_handle = llListen(-5,"ArenaMod", NULL_KEY, "");
    }
    listen( integer channel, string name, key id, string message )
    {
        // Stop listening until script is reset
        //llListenRemove(listen_handle);
        list UUIDs = llParseStringKeepNulls(message, '+', ' ');
        integer length = llGetListLength(UUIDs);
        integer i;
        for (i = 0; i < length; i++)
        {
            llOwnerSay("Spawning1");
            llShout(0,"Spawning2");
            llOwnerSay(llList2String(UUIDs, i));
            if (canSetNPC()) spawn(llKey2Name(llList2String(UUIDs, i)));
        }

    }

    timer()
    {
        osSetSpeed(npcKey, 3);
    }
}   





