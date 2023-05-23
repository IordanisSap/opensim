#ifndef COMM_LSL
#define COMM_LSL

/* Region specific broadcasts */

#define setupChannel 55
integer secureChannel = 0;
integer isChannelInit = 0;

integer computeChannel(key serverKey)
{
    return 0x80000000 | (integer) ( "0x" + (string) serverKey );
}

integer broadcast(string msg)
{
    if (!secureChannel) return 1;
    llRegionSay(secureChannel, msg);
    return 0;
}

integer sendTo(key UUID, string msg)
{
    if (!secureChannel) return 1;
    llRegionSayTo(UUID, secureChannel, msg);
    return 0;
}

#endif //COMM_LSL

