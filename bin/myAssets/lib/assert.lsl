#ifndef ASSERT_LSL
#define ASSERT_LSL

assert(integer condition, string msg){
    if (!condition)
    {
        llSay(DEBUG_CHANNEL, msg);
        llSetScriptState(llGetScriptName(),FALSE);
    }
}

#endif // ASSERT_LSL