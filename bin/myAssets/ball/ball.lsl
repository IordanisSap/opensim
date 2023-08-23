float Velocity = 12.0; //meters / second.
integer listen_handle;
integer listen_handle2;

vector target = ZERO_VECTOR;
integer channel = -13572468;

#include "ball/parsing.lsl"
#include "ball/physics.lsl"
#include "ball/texture_animations.lsl"
default
{
    state_entry()
    {
        llOwnerSay(llGetMass());
        llSleep(1.0);
        llSetStatus(STATUS_PHYSICS, TRUE);
        reset_texture();

        listen_handle = llListen(channel, "", "", "");
        listen_handle2 = llListen(0, "", "", "");
        llSetTimerEvent(MOVE_DECAY_FREQ);

        vector startPos = llGetPos();
        llMoveToTarget(llGetPos()+<0,0,-VERTICAL_DIFF>, 0.5);
        llTextBox(getAvatar(), "Commands", channel);

    }
    listen( integer channel, string name, key id, string message )
    {
        parse_message(message);
        llTextBox(id, "Commands", channel);
    }
    timer()
    {
        move_decay(target);
        list powerUps = getPowerUps(llGetKey());
    }
    on_rez(integer start_param)
    {
        llResetScript();
        llMoveToTarget(llGetPos()+<0,0,-VERTICAL_DIFF>, 0.5);
        llTextBox(getAvatar(), "Commands", channel);

    }
    touch_start(integer num_detected)
    {
        llTextBox(llDetectedKey(0), "Commands", channel);
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
    target = llGetPos()+<2*x,2*y,-VERTICAL_DIFF>;
    movePlayer(<x,y,z>);
    llApplyImpulse(<2*x,2*y,-VERTICAL_DIFF> * llGetMass()*500, FALSE);
    llMoveToTarget(llGetPos()+<2*x,2*y,-VERTICAL_DIFF>,llAbs(x+y+z)*0.5);
}

powerup(list commands){
    list args = llDeleteSubList(commands, 0, 1);
    llOwnerSay("Given args to powerup: "+ llList2String(args,0));
    llOwnerSay("Given args to powerup: "+ llList2String(args,1));
    string formattedCommand = llToUpper(llGetSubString(llList2String(commands,1), 0, 0)) + llDeleteSubString(llList2String(commands,1), 0, 0);
    activatePowerUp(formattedCommand, args);
}

