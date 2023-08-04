float MOVE_DECAY_RATE = 0.25;
float MOVE_DECAY_FREQ = 0.15;
float MOVE_DECAY_THRESHOLD = 0.05;
float MAX_SPEED = 1.55;

move_decay(){
    llOwnerSay(llGetVel());
    vector speed = llGetVel();
    if (llFabs(speed.z) > 0.1) return;
    if (llFabs(speed.x) < MOVE_DECAY_THRESHOLD && llFabs(speed.y) < MOVE_DECAY_THRESHOLD) return;
    float factor = 1;
    if (llFabs(speed.x) > MAX_SPEED || llFabs(speed.y) > MAX_SPEED) factor = 3.67;
    vector decaySpeed = -speed * MOVE_DECAY_RATE * factor;
    llApplyImpulse(decaySpeed, FALSE);
}