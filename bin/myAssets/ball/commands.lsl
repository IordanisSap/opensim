
command(){
    // Commands start here.
    move_fwd(3);
    sleep(3);
    move_right(2);
    sleep(2);
    move_fwd(4);
    sleep(4);
    move_left(2);
    sleep(2);
    move_back(2);
    sleep(2);
    move_fwd(4);
    activate_powerup("Shield",[]);
    sleep(4);
    move_fwd(3);
    sleep(3);
    //activate_powerup("Jump",[]);

    //build_fwd();
    //activate_powerup("Build",["front"]);
    // Commands end here.
 }
 
 example_command(){
    move_fwd(2);  
    activate_powerup("build",["forward"]);
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

 default
 {
     state_entry()
     {
         command();
     }
 }