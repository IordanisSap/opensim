#include "lib/utils.lsl"

key npcKey;
 
float npcSpeed = 10;
vector npcVelocity =  npcSpeed * <1.0,0.0,0.0>;
float IMPULSE = 1.6;
float range = 20;

integer roamOffsetX = 15;
integer roamOffsetY = 15;
//list roamArea = [llGetPos().x, llGetPos().y, llGetPos().x+roamOffsetX, llGetPos().y+roamOffsetY];
list roamArea = [100, 100, 180, 180];

string ANIMATION_WALK="Walk";
string ANIMATION_STAND="Stand";
string ANIMATION_SIT="Sit";

string FLY_STATIC_ANIM;
string FLY_FORWARD_ANIM;

key gun = NULL_KEY;
string GUN_NAME = "Touch_Gun";
integer shooting = FALSE;
float reloadTime = 2.0;
float lastShoot = 0.0;
float PROJECTILE_SPEED = 40;

float lastRoam = 0;
float roamWait = 2;
//string FLY_BACKWARD_ANIM;

integer parentListenHandle;
integer initialised = 0;
string READY_MSG = "READY";
integer parentChannel = 0;


setup_anim()
{
    FLY_STATIC_ANIM = llGetInventoryName(INVENTORY_ANIMATION, 0);
    FLY_FORWARD_ANIM = llGetInventoryName(INVENTORY_ANIMATION, 1);
    osNpcSetAnimationOverride(npcKey, "Flying", FLY_FORWARD_ANIM);
    osNpcSetAnimationOverride(npcKey, "Hovering", FLY_STATIC_ANIM);
    osNpcSetAnimationOverride(npcKey, "Flying Up", FLY_STATIC_ANIM);
    osNpcSetAnimationOverride(npcKey, "Flying Down", FLY_STATIC_ANIM);
    osNpcSetAnimationOverride(npcKey, "Falling", FLY_STATIC_ANIM);
    osNpcSetAnimationOverride(npcKey, "Standing", FLY_STATIC_ANIM);
    osNpcSetAnimationOverride(npcKey, "Walking", FLY_STATIC_ANIM);
}

get_gun(key npcKey)
{

    list AttachedUUIDs = llGetAttachedList(npcKey);
    integer i;
    while (i < llGetListLength(AttachedUUIDs) )
    {
        list temp = llGetObjectDetails(llList2Key(AttachedUUIDs,i),[OBJECT_NAME]);
        if (llList2String(temp,0) == GUN_NAME) 
        {
            gun = llList2Key(AttachedUUIDs,i);
            osNpcTouch(npcKey, gun, LINK_THIS); //Activate
            return;
        }
        ++i;
    }
    llShout(0, "Gun not found");
}

print_attachments(key npcKey){
    list AttachedUUIDs = llGetAttachedList(npcKey);
    integer i;
    while (i < llGetListLength(AttachedUUIDs) )
    {
        list temp = llGetObjectDetails(llList2Key(AttachedUUIDs,i),[OBJECT_NAME]);
        llOwnerSay(llList2String(temp,0));
        ++i;
    }
}


spawn_NPC(vector pos, string name, key srcKey, vector size)
{
    npcKey = osNpcCreate(name, "NPC", pos, srcKey);
    //osNpcPlayAnimation(npcKey, FLY_STATIC_ANIM);
    osSetSize(npcKey, size);
    setup_anim();
    get_gun(npcKey);
    osSetSpeed(npcKey, 3);
    llSleep(2);
}

state initNPC
{
    state_entry()
    {
        //llSetStatus(STATUS_PHANTOM, TRUE); // Become un-solid (for NPC invisible parent);
        parentListenHandle = llListen(parentChannel, "", "", "");
        llSetTimerEvent(1);
    }

    listen(integer channel, string name, key id, string message)
    {
        list data = llParseStringKeepNulls( message, "+", "");
        spawn_NPC(llGetPos() + <llGetScale().x * 1.2,0,0>, "Spawned NPC", llList2String(data,0), llList2Vector(data,2));
        initialised = 1;
        osAttachToNPC(npcKey, ATTACH_HEAD);
        if (llList2String(data,1) == "roam") state defaultNPCRoamBehaviour;
        else llSay(0,"unknown state "+ message);
    }
    timer()
    {
        llRegionSay(parentChannel, READY_MSG); //Attempt Connection
    }
}

integer isInitialised()
{
    return initialised;
}

lookAt(vector pos){
    vector start = llRot2Fwd(ZERO_ROTATION); //Object's X axis is forward. (Can be substituted with llRot2Left (Y axis) or llRot2Up (Z axis). Negative values (-llRot2Fwd) can be used to spin the prim 180 degrees)
    vector end = llVecNorm(pos - osNpcGetPos(npcKey));
    osNpcSetRot(npcKey, llRotBetween(start,end)); //Set the prim's rotation accordingly.
}

integer canShoot()
{
    if (llGetTime() - lastShoot < reloadTime) return FALSE;
    return TRUE;
}

shoot()
{
    if (!canShoot()) return;
    lastShoot = llGetTime();
    osNpcTouch(npcKey, gun, LINK_THIS); //Shoot
}



state defaultNPCRoamBehaviour
{
    state_entry()
    {
          llSetTexture(TEXTURE_TRANSPARENT, ALL_SIDES);
          llSensorRepeat("", NULL_KEY, AGENT, 60.0, PI, 0.3);
    }
    no_sensor(){
        if (llGetTime() - lastRoam < roamWait) {
            return;
        }

        lastRoam = llGetTime();
        vector currPos = osNpcGetPos(npcKey);
        if (llKey2Name(npcKey) == "") llDie();
        integer xMove = random_integer(-roamOffsetX/2, roamOffsetX/2);
        integer yMove = random_integer(-roamOffsetY/2, roamOffsetY/2);
        if (osNpcGetPos(npcKey).x + xMove <  llList2Integer(roamArea, 0) ||
            osNpcGetPos(npcKey).x + xMove >  llList2Integer(roamArea, 1)) {xMove = -xMove;}
        if (osNpcGetPos(npcKey).y + yMove <  llList2Integer(roamArea,2) ||
            osNpcGetPos(npcKey).y + yMove >  llList2Integer(roamArea,3)){yMove = -yMove;}
        osNpcMoveToTarget(npcKey, currPos + <xMove,yMove,0>, OS_NPC_FLY);
    }

    sensor(integer num_detected)
    {


        vector currPos = osNpcGetPos(npcKey);
        vector detectedPos = llList2Vector(llGetObjectDetails(llDetectedKey(0), OBJECT_POS),0);
        vector speed =  llDetectedVel(0);

        vector start = llRot2Fwd((osNpcGetRot(npcKey))); //Object's X axis is forward. (Can be substituted with llRot2Left (Y axis) or llRot2Up (Z axis). Negative values (-llRot2Fwd) can be used to spin the prim 180 degrees)
        vector end = llVecNorm(detectedPos - osNpcGetPos(npcKey));
        //llShout(0, "rot1= " +llRotBetween(start,end));
        float enemyDist = llVecDist(currPos, detectedPos);

        if (!canShoot() && enemyDist < 25)
        {
            vector diff = detectedPos - currPos;
            float moveX = 1 + llFrand(4);
            float moveY = 1 + llFrand(4);
            if (llFabs(diff.x) > llFabs(diff.y)) moveY += 2;
            else moveX += 2;

            moveX *= 2;
            moveY *= 2;
        
            if (diff.x > 0) moveX = -moveX;
            if (diff.y > 0) moveY = -moveY;
            osNpcMoveToTarget(npcKey, <currPos.x + moveX, currPos.y + moveY, detectedPos.z>, OS_NPC_FLY );
            llSleep(0.3);
            return;
        }

        vector offset = speed  *  (llVecDist(currPos, detectedPos)/35);
        if(enemyDist > range)
        {
            osNpcMoveToTarget(npcKey, detectedPos + offset , OS_NPC_FLY );
            llSleep(0.1);
        }
        else 
        {
            if (speed.x > 3 || speed.y > 3)
            {
                osNpcMoveToTarget(npcKey, detectedPos + offset*1.5, OS_NPC_FLY );
                llSleep(0.1);
            }
            else {
                lookAt(detectedPos);
                llSleep(0.5);
                osNpcMoveToTarget(npcKey, detectedPos + speed/2, OS_NPC_FLY );
                llSleep(0.4);
            }
        }
        shoot();
        llSleep(0.1);
    }
}