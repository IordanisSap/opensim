integer damage = 100;
string TEXTURE_TRANSPARENT = "8dcd4a48-2d37-4909-9f78-f7a9eb4ef903"; 

hit()
{
    llParticleSystem([
        PSYS_PART_FLAGS, PSYS_PART_EMISSIVE_MASK 
            | PSYS_PART_INTERP_COLOR_MASK
            | PSYS_PART_INTERP_SCALE_MASK,
        PSYS_PART_START_COLOR, <0.9, 0.763, 0.000>,
        PSYS_PART_END_COLOR, <0.7, 0.7, 0.7>,
        PSYS_PART_START_SCALE, <0.5, 0.5, 0.5>,
        PSYS_PART_END_SCALE, <3, 3, 3>,
        PSYS_PART_MAX_AGE, 1,
        PSYS_SRC_PATTERN, PSYS_SRC_PATTERN_EXPLODE, 
        PSYS_SRC_BURST_RATE, 4.0,
        PSYS_SRC_BURST_PART_COUNT, 15,
        PSYS_SRC_BURST_SPEED_MIN, 0.2,
        PSYS_SRC_BURST_SPEED_MAX, 0.4,
        PSYS_PART_START_ALPHA, 0.4,
        PSYS_PART_END_ALPHA, 0
        ]);   
}

explode()
{
    llParticleSystem([
        PSYS_PART_FLAGS, PSYS_PART_EMISSIVE_MASK 
            | PSYS_PART_INTERP_COLOR_MASK
            | PSYS_PART_INTERP_SCALE_MASK,
        PSYS_PART_START_COLOR, <0.9, 0.763, 0.000>,
        PSYS_PART_END_COLOR, <0.7, 0.7, 0.7>,
        PSYS_PART_START_SCALE, <6, 6, 6>,
        PSYS_PART_END_SCALE, <24, 24, 24>,
        PSYS_PART_MAX_AGE, 1.8,
        PSYS_SRC_PATTERN, PSYS_SRC_PATTERN_EXPLODE, 
        PSYS_SRC_BURST_RATE, 4.0,
        PSYS_SRC_BURST_PART_COUNT, 250,
        PSYS_SRC_BURST_SPEED_MIN, 10,
        PSYS_SRC_BURST_SPEED_MAX, 12,
        PSYS_PART_START_ALPHA, 0.4,
        PSYS_PART_END_ALPHA, 0
        ]);
}

chase(vector pos, vector vel, key llDetectedKey)
{
    llRotLookAt( llRotBetween( <0.0,1.0,0.0>, llVecNorm( pos - llGetPos() ) ), 1.0, 0.4 ); // Point +Y axis towards vPosTarget
    llMoveToTarget(pos,0.5);  
}

default
{
    state_entry()
    {
        llParticleSystem([]);
        llSetPos(llGetPos()+ <0,0,0.2>);
        llSetStatus(STATUS_PHYSICS, TRUE);
        llSetPhysicsMaterial(GRAVITY_MULTIPLIER, 0,0,0,0);
        llMoveToTarget(llGetPos()+ <0,0,1>, 1);
        llSensorRepeat("", "", AGENT | NPC, 80.0, PI, 0.1);
    }
    
    collision_start(integer num_detected)
    {
        float targetHP = osGetHealth(llDetectedKey(0));
        osCauseDamage(llDetectedKey(0), damage);
        if (targetHP > 0 && targetHP <= damage){
            explode();
        }else {
            hit();
        }
        llSetTexture(TEXTURE_TRANSPARENT, ALL_SIDES);
        llSetVelocity(<0,0,0>, FALSE);
        llSleep(0.2);
        llDie();
    }

    land_collision(vector pos)
     {
        hit();
        llSetTexture(TEXTURE_TRANSPARENT, ALL_SIDES);
        llSetVelocity(<0,0,0>, FALSE);
        llSleep(0.2);
        llDie();
     }

     sensor(integer num_detected)
     {
        llSay(0,"Num is " + (string)num_detected);
        integer index = 0;
        while (index < num_detected)
        {
            if(llDetectedKey(index) != llGetOwner())
            {
                llSay(0,"Found him");
                chase(llDetectedPos(index), llDetectedVel(index), llDetectedKey(index)); 
                return;           
            }
            index++;
        }  
     }
}