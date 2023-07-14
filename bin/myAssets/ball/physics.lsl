float MOVE_DECAY_RATE = 0.5;
float MOVE_DECAY_FREQ = 0.3;
float MOVE_DECAY_THRESHOLD = 1;

move_decay(){
    vector speed = llGetVel();
    if (speed.z > 0.1) return;
    vector decaySpeed = speed * MOVE_DECAY_RATE;
    if (decaySpeed.x < MOVE_DECAY_THRESHOLD)  llSetVelocity(<0, speed.y, speed.z>,FALSE);
    if (decaySpeed.y < MOVE_DECAY_THRESHOLD) llSetVelocity(<speed.x, 0, speed.z>,FALSE);
    llSetVelocity(decaySpeed, FALSE);
}