float Velocity = 12.0; //meters / second.
integer listen_handle;

#include "ball/parsing.lsl"
#include "ball/physics.lsl"

default
{
    state_entry()
    {
        llSleep(1.0);

        listen_handle = llListen(0, "", "", "");

        llSetTimerEvent(MOVE_DECAY_FREQ);
    }
    listen( integer channel, string name, key id, string message )
    {
        parse_message(message);
    }
    timer()
    {
        move_decay();
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
    if (llList2String(command,0) == "move"){
        move(llList2Float(command,1), llList2Float(command,2), llList2Float(command,3));
    }
}

move(float x, float y, float z){
    vector adjVel = <x,y,z> * llGetMass();
    llSetVelocity(adjVel, FALSE);
}

