integer rotations = 0;
integer colliding = FALSE;

default
{
    state_entry()
    {
        llVolumeDetect(TRUE);
        integer leverLink = get_prim("Lever");
        llSetLinkPrimitiveParamsFast(leverLink, [PRIM_ROT_LOCAL, <0.0, -0.33689, 0, 0.941544>]);
    }
    
    collision_start(){
        if (playerExists(llDetectedKey(0)) == FALSE) return;
        if (collision_check(llDetectedPos(0)) == FALSE) {
            return;
        }
        if (colliding == TRUE) return;
        if (rotations < 14) {
            rotate();
            llMessageLinked(get_prim("Fire"), 14 - rotations, llGetScriptName(), "");
            colliding = TRUE;
        }
    }

    collision_end(){
        colliding = FALSE;
    }

    link_message(integer sender_num, integer num, string str, key id)
    {
        if (sender_num == get_prim("Fire")) obstacleCollision(id);
    }
}

rotate(){
    // Calculate the relative rotation (rotate by pi radians around Y-axis)
    rotations += 1;
    rotation relativeRotation = llEuler2Rot(<0.0, PI/32, 0.0>);

    integer leverLink = get_prim("Lever");
    rotation currentRotation = llList2Rot(llGetLinkPrimitiveParams(leverLink, [PRIM_ROT_LOCAL]),0);
    
    // Get the current rotation of the object
    //rotation currentRotation = llGetLocalRot();

    // Combine the current rotation with the relative rotation
    rotation newRotation = relativeRotation * currentRotation;
    
    // Set the new rotation for the object
            llSetLinkPrimitiveParamsFast(leverLink, [PRIM_ROT_LOCAL, newRotation]);
            }


integer get_prim(string name)
{
    integer i = llGetNumberOfPrims();
    for (; i >= 0; --i)
    {
        if (llGetLinkName(i) == name)
        {
            return i;
        }
    }
    return -1;
}


integer collision_check(vector pos){
    integer leverLink = get_prim("FireObstacle");
    vector collisionBoxPos = llList2Vector(llGetLinkPrimitiveParams(leverLink, [PRIM_POSITION]),0);
    vector collisionBoxSize = llList2Vector(llGetLinkPrimitiveParams(leverLink, [PRIM_SIZE]),0);
    vector collisionBoxMin = collisionBoxPos - collisionBoxSize * 1.2;
    vector collisionBoxMax = collisionBoxPos + collisionBoxSize * 1.2;
    if (pos.x > collisionBoxMin.x && pos.x < collisionBoxMax.x && pos.y > collisionBoxMin.y && pos.y < collisionBoxMax.y && pos.z > collisionBoxMin.z && pos.z < collisionBoxMax.z){
        return TRUE;
    }
    return FALSE;
}