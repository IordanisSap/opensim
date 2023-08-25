
command(){
    // Commands start here.
    moveForward(1);    
    // Commands end here.
 }
 
 
 
 
 // Do not edit beyond this point.
 integer channel = -13572468;


 moveForward(integer distance){
    llRegionSay(channel, "moveForward(" + distance + ")");
    llOwnerSay("moveForward(" + distance + ")");

 }
 
 default
 {
     state_entry()
     {
         command();
     }
 }