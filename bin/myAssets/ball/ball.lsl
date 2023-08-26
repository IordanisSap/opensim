float Velocity = 12.0; //meters / second.
integer listen_handle;
integer listen_handle2;

vector target = ZERO_VECTOR;
integer channel = -13572468;


integer MOVEMENT_START;

#include "ball/parsing.lsl"
#include "ball/physics.lsl"
#include "ball/texture_animations.lsl"
default
{
    state_entry()
    {
        llSleep(1.0);
        llSetStatus(STATUS_PHYSICS, TRUE);
        reset_texture();

        listen_handle = llListen(channel, "", "", "");
        listen_handle2 = llListen(0, "", "", "");
        llSetTimerEvent(MOVE_CHECK_FREQ);

        vector startPos = llGetPos();

    }
    listen( integer channel, string name, key id, string message )
    {
        parse_message(message);
    }
    timer()
    {
        MOVE_CHECK(target);
        list powerUps = getPowerUps(llGetKey());
    }
    on_rez(integer start_param)
    {
        llResetScript();
        llTextBox(getAvatar(), "Commands", channel);

    }
}


parse_message(string input){
    list commands = llParseString2List(llToLower(removeWhitespaces(input)),[";"],[""]);
    integer size = llGetListLength(commands);
    for (integer i = 0; i < size; i++){
        list command = parse_single(llList2String(commands, i));
        print_command(command);
        translate_command(command);
    }
}


translate_command(list command)
{
    if (llList2String(command,0) == "moveright"){
        move(llList2Integer(command,1), 0, 0);
    }
    else if (llList2String(command,0) == "moveleft"){
        move(-llList2Integer(command,1), 0, 0);
    }
    else if (llList2String(command,0) == "moveback"){
        move(0, -llList2Integer(command,1), 0);
    }
    else if (llList2String(command,0) == "moveforward"){
        move(0, llList2Integer(command,1), 0);
    }
    else if (llList2String(command,0) == "activatepowerup"){
        powerup(command);
    }
}

move(integer x, integer y, integer z){
    target = llGetPos()+<2*x,2*y,0>;
    movePlayer(<x,y,z>);
    if (x > 0) llSetVelocity(<2,0,0>, FALSE);
    else if (y > 0) llSetVelocity(<0,2,0>, FALSE);
    else if (x < 0) llSetVelocity(<-2,0,0>, FALSE);
    else if (y < 0) llSetVelocity(<0,-2,0>, FALSE);
    else llSetVelocity(<0,0,0>, FALSE);
    MOVEMENT_START = llGetUnixTime();
    llOwnerSay("START: " + (string)MOVEMENT_START);
}

powerup(list commands){
    list args = llDeleteSubList(commands, 0, 1);
    string formattedCommand = llToUpper(llGetSubString(llList2String(commands,1), 0, 0)) + llDeleteSubString(llList2String(commands,1), 0, 0);
    activatePowerUp(formattedCommand, args);
}

