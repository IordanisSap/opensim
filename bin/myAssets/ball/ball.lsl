float Velocity = 12.0; //meters / second.
integer listen_handle;
list powerups = ["Shield_powerup"];
integer powerup_duration = 8;
float current_powerup_timestamp = 0;
vector target = ZERO_VECTOR;

#include "ball/parsing.lsl"
#include "ball/physics.lsl"
#include "ball/texture_animations.lsl"
default
{
    state_entry()
    {
        llSleep(1.0);
        reset_texture();

        listen_handle = llListen(0, "", "", "");

        llSetTimerEvent(MOVE_DECAY_FREQ);

        llCollisionFilter("", NULL_KEY, FALSE);
    }
    listen( integer channel, string name, key id, string message )
    {
        parse_message(message);
    }
    timer()
    {
        move_decay(target);
    }
    on_rez(integer start_param)
    {
        llResetScript();
    }
    collision_start(integer num_detected)
    {
        string name = llDetectedName(0);
    }
    moving_end()
    {
        //target = llGetPos();
        //llSetTimerEvent(move_time);
    }
}


parse_message(string input){
    list commands = llParseString2List(removeWhitespaces(input),[";"],[""]);
    integer size = llGetListLength(commands);
    for (integer i = 0; i < size; i++){
        list command = parse_single(llList2String(commands, i));
        print_command(command);
        translate_command(command);
    }
}


translate_command(list command)
{
    if (llList2String(command,0) == "moveRight"){
        move(llList2Integer(command,1), 0, 0);
    }
    else if (llList2String(command,0) == "moveLeft"){
        move(-llList2Integer(command,1), 0, 0);
    }
    else if (llList2String(command,0) == "moveBack"){
        move(0, -llList2Integer(command,1), 0);
    }
    else if (llList2String(command,0) == "moveForward"){
        move(0, llList2Integer(command,1), 0);
    }
}

move(integer x, integer y, integer z){
    target = llGetPos()+<2*x,2*y,-10>;
    llMoveToTarget(llGetPos()+<2*x,2*y,-10>,llAbs(x+y+z)*1);
}
