
default
{
    state_entry()
    {
        llSetTimerEvent(600);
    }
    timer()
    {
        resetMaze();
    }
    touch(integer num_detected)
    {
        if (mazeHasStarted())
            return;
        generateMaze(4, llGetPos()-<20,-3,3>);
    }
}

