
default
{
    state_entry()
    {
        generateMaze(30, llGetPos());
        llSetTimerEvent(350);
    }
    timer()
    {
        resetMaze();
    }
    touch(integer num_detected)
    {
        llOwnerSay("Resetting maze");
        resetMaze();
    }
}

