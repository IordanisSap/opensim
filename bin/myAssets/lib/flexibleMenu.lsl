integer    gActionsPerPage = 3;           // Number of action choice buttons per menu page (must be 1 to 10, or 12)
integer    gTotalActions;
integer    gPage;                     // Current dialog page number (counting from zero)
integer    gMaxPage;                  // Highest page number (counting from zero)


buildDialogPage(key user)
{
    integer start = gActionsPerPage * gPage;
    list buttons = [ "<<", "Close", ">>" ];
    if (gActionsPerPage == 10)           buttons = [ "<<", ">>" ];
    else if (gActionsPerPage > 10)       buttons = [];          // No room for paging buttons

    // 'start + gActionsPerPage -1' might point beyond the end of the list -
    // - but LSL stops at the list end, without throwing a wobbly
    buttons += llList2List(gListActions, start, start + gActionsPerPage - 1);
    llDialog(user, "\nPage " + (string) (gPage+1) + " of " + (string) (gMaxPage + 1) + "\n\nChoose an action", buttons, userChan);
}

integer initMenu(list gListActions)
{
    gTotalActions = (gListActions != [] );
    if (gActionsPerPage < 1 || gActionsPerPage > 12)
    {
        llOwnerSay("Invalid 'gActionsPerPage' - must be 1 to 12");
        return 1;
    }

    gMaxPage = (gTotalActions - 1) / gActionsPerPage;
    if (gActionsPerPage > 10)
    {
        gMaxPage = 0;
        if (gTotalActions > gActionsPerPage)
        {
            llOwnerSay("Too many actions in total for this ActionsPerPage setting");
            return 2;
        }
    }
    return 0;
}

integer startMenu()
{
    llListen(userChan, "", user, "");                // This listener will be used throughout this state
    gPage = 0;
    buildDialogPage(user);                        // Show  Page 0 dialog to current user
    llSetTimerEvent(60);              // If no response in time, return to 'ready' state
    return 0;
}

integer handlePageChange(string msg, key id)
{
    if (msg == "<<" || msg == ">>")                   // Page change ...
    {
        if (msg == "<<")        --gPage;              // Page back
        if (msg == ">>")        ++gPage;              // Page forward
        if (gPage < 0)          gPage = gMaxPage;     // cycle around pages
        if (gPage > gMaxPage)   gPage = 0;
        buildDialogPage(id);
        return 0;
    }
    return 1;
}