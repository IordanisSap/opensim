float MOVE_CHECK_FREQ = 0.05;
float MOVE_CHECK_THRESHOLD = 0.15;


move_check(vector target){
    vector currPos = llGetPos();
    if (llVecDist(llGetVel(),<0,0,0>)==0){return;}
    if (llVecDist(target, currPos) < MOVE_CHECK_THRESHOLD){
        llSetVelocity(<0,0,0>, FALSE);
        llSetStatus(STATUS_PHYSICS, FALSE);
        llSetPos(target);
        llSetStatus(STATUS_PHYSICS, TRUE);
    }
}

