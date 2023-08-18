float MOVE_DECAY_RATE = 0.25;
float MOVE_DECAY_FREQ = 0.15;
float MOVE_DECAY_THRESHOLD = 0.2;

float VERTICAL_DIFF = 20;

move_decay(vector target){
    vector speed = llGetVel();
    vector currPos = llGetPos();
    currPos.z = currPos.z - VERTICAL_DIFF;
    if (llVecDist(currPos, target) < 0.001) return;
    float speedFactor = llVecDist(speed, <0,0,0>);
    if (speedFactor < 1) speedFactor = 1;
    if (llVecDist(target, currPos) < 0.2 * speedFactor){
        llSetVelocity(<0,0,speed.z>, FALSE);
    }
    if (llVecDist(target, currPos) < 0.4){
        llSetStatus(STATUS_PHYSICS, FALSE);
        llSetPos(target + <0,0,VERTICAL_DIFF>);
        llSetStatus(STATUS_PHYSICS, TRUE);
    }
}
