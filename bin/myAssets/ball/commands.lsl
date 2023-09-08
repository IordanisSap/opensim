
command(){
    // Commands start here.


    // move_left(1);
    // sleep(1);
    move_fwd(1);
    sleep(1);
    level_up();
    // move_left(1);
    // sleep(1);
    // move_fwd(2);
    // sleep(2);
    // move_back(2);
    // sleep(2);
    // move_left(2);
    // sleep(2);
    // level_up();
    // build_fwd();

    // Commands end here.
 }
 
 example_command(){
    move_fwd(2);  
    sleep(2);
    build_fwd();
}
 
 
 
 // Do not edit beyond this point.
 integer channel = -13572468;

 sleep(integer seconds){
    llSleep(seconds);
 }

 shield(){
    activate_powerup("Shield",[]);
 }

 build_fwd(){
    activate_powerup("Build",["forward"]);
 }

build_left(){
    activate_powerup("Build",["left"]);
}

build_right(){
    activate_powerup("Build",["right"]);
}

build_back(){
    activate_powerup("Build",["back"]);
}

level_up(){
    activate_powerup("LevelUp",[]);
}

integer powerup_num(string searchPowerup){
    list powerups = getPowerUps(llGetKey());
    integer count = 0;
    integer totalPowerups = llGetListLength(powerups);
    for (integer i = 0; i < totalPowerups; i++)
    {
        string powerup = llList2String(powerups, i);
        if (powerup == searchPowerup)
        {
            count++;
        }
    }
    return count;
}

 default
 {
     state_entry()
     {
         command();
     }
 }