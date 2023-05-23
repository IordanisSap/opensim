#ifndef SERVER_LSL
#define SERVER_LSL

#include "lib/comm.lsl"

initServer()
{
    llRegionSay(setupChannel, llGetKey());
    secureChannel = computeChannel(llGetKey());
}

#endif // SERVER_LSL


