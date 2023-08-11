float MOVE_DECAY_RATE = 0.25;
float MOVE_DECAY_FREQ = 0.15;
float MOVE_DECAY_THRESHOLD = 0.2;

move_decay(vector target){
    vector speed = llGetVel();
    vector currPos = llGetPos();
    currPos.z = currPos.z - 10;
    if (llVecDist(currPos, target) < 0.001) return;
    float speedFactor = llVecDist(speed, <0,0,0>);
    if (speedFactor < 1) speedFactor = 1;
    if (llVecDist(target, currPos) < 0.2 * speedFactor){
        llSetVelocity(<0,0,speed.z>, FALSE);
    }
    if (llVecDist(target, currPos) < 0.1){
        llSetStatus(STATUS_PHYSICS, FALSE);
        llSetPos(target + <0,0,10>);
        llSetStatus(STATUS_PHYSICS, TRUE);
    }
}
