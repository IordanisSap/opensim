//This script is used to control your drone in the arena
//You can use the following functions to implement your own AI inside the AI function
//getSelfPos() returns the position of your drone
//getEnemyPos() returns the position of the enemy drone
//moveTo(vector pos) moves your drone to the specified position
//lookAt(vector pos) makes your drone look at the specified position
//shoot() makes your drone shoot

//The AI function will be called every 0.1 seconds
//Warning: Do not change anything other than the AI function or the script may not work and you will be disqualified




AI(){
    exampleAI();
}

exampleAI(){
    vector targetPos = getEnemyPos();
    vector selfPos = getSelfPos();
    if (llVecDist(selfPos, targetPos) < 60){
        if (targetPos.x > selfPos.x) targetPos.x -= 25 + llFrand(5);
        else targetPos.x += 25 + llFrand(5) ;
        if (targetPos.y > selfPos.y) targetPos.y -= 25 + llFrand(5);
        else targetPos.y += 25 + llFrand(5);
    }
    moveTo(targetPos);
    llSleep(0.6);
    moveTo(getEnemyPos());
    //lookAt(targetPos);
    llSleep(0.05);
    shoot();
    llSleep(0.05);
    moveTo(targetPos);
}



key _self = NULL_KEY;
key _enemy = NULL_KEY;
key _controller = NULL_KEY;
integer _channel = -1;


default
{
    state_entry()
    {
        _self =  getControllableObject();
        _enemy = getEnemy();
        _controller = getController();
        _channel = getArenaCommChannel();
        if (_self == NULL_KEY || _enemy == NULL_KEY || _controller == NULL_KEY || _channel == -1){
            llOwnerSay("Error: Could not find all objects");
            return;
        }
        llSetTimerEvent(0.4);
    }
    timer()
    {
        AI();
        if (objectExists(_enemy) == 0 || objectExists(_self) == 0){
            llOwnerSay("Error: Enemy or Self does not exist");
            llSetScriptState(llGetScriptName(),FALSE);
            llSleep(0.2);
            llResetScript();
        }
    }
    on_rez(integer start_param)
    {
        llResetScript();
    }
}


integer objectExists(key uuid){
    if (llKey2Name(uuid)== "") {return 0;}
    return 1;
}

vector getEnemyPos(){
    return llList2Vector(llGetObjectDetails(_enemy, [OBJECT_POS]), 0);
}

vector getSelfPos(){
    return llList2Vector(llGetObjectDetails(_self, [OBJECT_POS]), 0);
}

moveTo(vector pos){
    llOwnerSay("Moving to " + (string)pos);
    llRegionSayTo(_controller, _channel, "Move+" + (string)pos);
}

lookAt(vector pos){
    llRegionSayTo(_controller, _channel, "LookAt+" + (string)pos);

}

shoot(){
    llRegionSayTo(_controller, _channel, "Shoot");
}

