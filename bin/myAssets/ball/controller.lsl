
default
{
    touch(integer num_detected)
    {
        if (mazeHasStarted())
            return;
        generateMaze(6, llGetPos()-<20,-3,3>, llDetectedKey(0));
    }
}

