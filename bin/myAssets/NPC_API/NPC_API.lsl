#include "NPC_API/Weapon.lsl"
#include "lib/assert.lsl"


initNPCWeapon(key npcKey)
{
    assert(WEAPON_NAME != "", "WEAPON_NAME not set");
    list AttachedUUIDs = llGetAttachedList(npcKey);
    integer i;
    key tmp_weapon = NULL_KEY;
    while (i < llGetListLength(AttachedUUIDs) )
    {
        list temp = llGetObjectDetails(llList2Key(AttachedUUIDs,i),[OBJECT_NAME]);
        if (llList2String(temp,0) == WEAPON_NAME) 
        {
            tmp_weapon = llList2Key(AttachedUUIDs,i);
            osNpcTouch(npcKey, gun, LINK_THIS); //Activate
            i = llGetListLength(AttachedUUIDs); //Break
        }
        ++i;
    }
    assert(tmp_weapon != NULL_KEY, "Gun not found");
    initWeapon(tmp_weapon, );


}

lookAt(key npcKey, vector pos){
    vector start = llRot2Fwd(ZERO_ROTATION); //Object's X axis is forward. (Can be substituted with llRot2Left (Y axis) or llRot2Up (Z axis). Negative values (-llRot2Fwd) can be used to spin the prim 180 degrees)
    vector end = llVecNorm(pos - osNpcGetPos(npcKey));
    osNpcSetRot(npcKey, llRotBetween(start,end)); //Set the prim's rotation accordingly.
}


lookAt(key npcKey, key target){
    vector pos = llGetPos(target);
    vector start = llRot2Fwd(ZERO_ROTATION); //Object's X axis is forward. (Can be substituted with llRot2Left (Y axis) or llRot2Up (Z axis). Negative values (-llRot2Fwd) can be used to spin the prim 180 degrees)
    vector end = llVecNorm(pos - osNpcGetPos(npcKey));
    osNpcSetRot(npcKey, llRotBetween(start,end)); //Set the prim's rotation accordingly.
}