//Child Prim PIN setter
integer PIN=123321;

default 
{
    collision(integer num_detected)
    {
        integer finished = endPointCollision(llDetectedKey(0));
        if (finished == 1){
            llSetScriptState(llGetScriptName(),FALSE);
        }

    }
}