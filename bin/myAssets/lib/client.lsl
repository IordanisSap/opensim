#ifndef CLIENT_LSL
#define CLIENT_LSL

#include "lib/comm.lsl"
integer listenHandle = 0;
key globalObjectHandler = "";


state setup_client
{
    state_entry()
    {
        listenHandle = llListen(setupChannel, "", "", "");
    }

    listen(integer channel, string name, key id, string message)
    {
        llListenRemove(listenHandle);
        globalObjectHandler = message;
        secureChannel = computeChannel(message);
        llSay(0,"setup connection with " + message);
        isChannelInit = 1;
        state default;
    }
}

#endif // CLIENT_LSL

