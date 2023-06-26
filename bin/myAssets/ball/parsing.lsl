
list parse_single(string rawCommand){
    integer commandDataStartIndex = llSubStringIndex(rawCommand, "(");
    integer commandDataEndIndex = llSubStringIndex(rawCommand, ")");
    string commandDataRaw = llGetSubString(rawCommand, commandDataStartIndex + 1, commandDataEndIndex - 1);
    list commandData = llParseString2List(commandDataRaw, [","], [""]);
    string command = llGetSubString(rawCommand, 0, commandDataStartIndex - 1);

    list commandList = [command];
    integer i;
    for (i=0; i < llGetListLength(commandData); i++){
        commandList += [llList2String(commandData, i)];
    }
    return commandList;
}

print_command(list command){
    llOwnerSay("Command: "+llList2String(command, 0));
    integer i;
    for (i=1; i < llGetListLength(command); i++){
        llOwnerSay("Arg "+(string)i+": "+llList2String(command, i));
    }
}

string removeWhitespaces(string input)
{
    string result = "";
    
    // Iterate over each character in the input string
    integer length = llStringLength(input);
    for (integer i = 0; i < length; ++i)
    {
        // Get the current character
        string character = llGetSubString(input, i, i);
        
        // Check if the character is a whitespace
        if (character != " ")
        {
            // Append non-whitespace characters to the result
            result += character;
        }
    }
    
    return result;
}