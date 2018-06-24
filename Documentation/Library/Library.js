function getSidebar(page) {

    document.getElementsByClassName("sidebar")[0].innerHTML = sidebar.replace(">" + page + "<", "><strong>" + page + "</strong><");

    var path = window.location.href.split("/");
    for (var p = path.length - 2; p >= 0; p--) {
        if (path[p].toLowerCase() == "library") {
            path.length = p + 1;
            path = path.join("/") + "/";
            break;
        }
    }

    document.getElementsByTagName("head")[0].innerHTML += "<base href='" + path + "' />";
}

function toggleDropdown(div) {
    var hidediv = div.nextElementSibling;

    if (hidediv.style.display == "none") {
        hidediv.style.display = "block";
        div.innerHTML = "<img src=\"/Library/Assets/vs/open.png\" />"
    }
    else {
        hidediv.style.display = "none";
        div.innerHTML = "<img src=\"/Library/Assets/vs/closed.png\" />"
    }
}

function getMaps() {
    // VERY not future proof. Update this if the map class recieves any format changes.

    var req = new XMLHttpRequest();

    req.onload = function () {
        var page = this.responseText;

        var startIndex = page.indexOf("// Assault", page.indexOf("public class Map")) + 10;
        var endIndex = page.indexOf("/// <summary>", startIndex);

        var parse = page.substring(startIndex, endIndex);

        var table = "<table style=\"height: 500px; overflow-y: scroll; display: block; \"><tr><th>Map Name</th><th>Mode</th><th>Event</th></tr>";

        var maps = parse.split("\n");
        for (var i = 0; i < maps.length; i++) {
            var map = maps[i].trim();
            if (map.substring(0, 17) == "public static Map") {

                var name = map.substring(18, map.indexOf(" ", 18));

                var modeIndex = map.indexOf("Gamemode", 18) + 9;
                var mode = map.substring(modeIndex, map.indexOf(",", modeIndex));

                var eventIndex = map.indexOf("Event", modeIndex) + 6;
                var event = map.substring(eventIndex, map.indexOf(")", eventIndex));

                table += ("<tr><td>" + name + "</td><td>" + mode + "</td><td>" + event + "</td></tr>");
            }
        }

        table += "</table>";

        document.getElementById("mapList").innerHTML = table;
    }

    req.open('GET', "https://raw.githubusercontent.com/ItsDeltin/Overwatch-Custom-Game-Automation/master/CustomGameLib/CustomGameLib/Map.cs");
    req.send();
}

var sidebar = "<a href=\"Library.html\">Getting started<\/a>\r\n<hr \/>\r\n\r\n<a href=\"Deltin.CustomGameAutomation.html\" class=\"vsnamespace\">Deltin.CustomGameAutomation<\/a>\r\n<div class=\"sidebarSection\">\r\n\r\n    <a href=\"CustomGame\/CustomGame.html\" class=\"vsclass\">CustomGame<\/a>\r\n    <div class=\"sidebarSection\">\r\n\r\n        <a href=\"CustomGame\/AI\/CG_AI.html\" class=\"vsclass\">CG_AI<\/a>\r\n        <div class=\"sidebarSection\">\r\n            <a href=\"CustomGame\/AI\/AddAI.html\" class=\"vsmethod\">AddAI(AIHero, Difficulty, BotTeam, int)<\/a>\r\n            <a href=\"CustomGame\/AI\/EditAI-1.html\" class=\"vsmethod\">EditAI(int, AIHero, Difficulty)<\/a>\r\n            <a href=\"CustomGame\/AI\/EditAI-2.html\" class=\"vsmethod\">EditAI(int, AIHero)<\/a>\r\n            <a href=\"CustomGame\/AI\/EditAI-3.html\" class=\"vsmethod\">EditAI(int, Difficulty)<\/a>\r\n            <a href=\"CustomGame\/AI\/GetAIDifficulty.html\" class=\"vsmethod\">GetAIDifficulty(int, bool)<\/a>\r\n            <a href=\"CustomGame\/AI\/IsAI.html\" class=\"vsmethod\">IsAI(int)<\/a>\r\n            <a href=\"CustomGame\/AI\/RemoveAllBotsAuto.html\" class=\"vsmethod\">RemoveAllBotsAuto()<\/a>\r\n        <\/div>\r\n\r\n        <a href=\"CustomGame\/Chat\/CG_Chat.html\" class=\"vsclass\">CG_Chat<\/a>\r\n        <div class=\"sidebarSection\">\r\n            <a href=\"CustomGame\/Chat\/Chat.html\" class=\"vsmethod\">Chat(string)<\/a>\r\n            <a href=\"CustomGame\/Chat\/JoinChannel.html\" class=\"vsmethod\">JoinChannel(Channel)<\/a>\r\n            <a href=\"CustomGame\/Chat\/LeaveChannel.html\" class=\"vsmethod\">LeaveChannel(Channel)<\/a>\r\n            <a href=\"CustomGame\/Chat\/SwapChannel.html\" class=\"vsmethod\">SwapChannel(Channel)<\/a>\r\n        <\/div>\r\n\r\n        <a href=\"CustomGame\/GameSettings\/CG_Settings.html\" class=\"vsclass\">CG_Settings<\/a>\r\n        <div class=\"sidebarSection\">\r\n            <a href=\"CustomGame\/GameSettings\/LoadPreset.html\" class=\"vsmethod\">LoadPreset(int)<\/a>\r\n            <a href=\"CustomGame\/GameSettings\/SetGameName.html\" class=\"vsmethod\">SetGameName(string)<\/a>\r\n            <a href=\"CustomGame\/GameSettings\/SetJoinSetting.html\" class=\"vsmethod\">SetJoinSetting(Join)<\/a>\r\n            <a href=\"CustomGame\/GameSettings\/SetMaxPlayers.html\" class=\"vsmethod\">SetMaxPlayers(int?, int?, int?, int?)<\/a>\r\n            <a href=\"CustomGame\/GameSettings\/SetTeamName.html\" class=\"vsmethod\">SetTeamName(PlayerTeam, string)<\/a>\r\n        <\/div>\r\n\r\n        <a href=\"CustomGame\/Interact\/CG_Interact.html\" class=\"vsclass\">CG_Interact<\/a>\r\n        <div class=\"sidebarSection\">\r\n            <a href=\"CustomGame\/Interact\/Move.html\" class=\"vsmethod\">Move(int, int)<\/a>\r\n            <a href=\"CustomGame\/Interact\/RemoveAllBots.html\" class=\"vsmethod\">RemoveAllBots(int)<\/a>\r\n            <a href=\"CustomGame\/Interact\/RemoveFromGame.html\" class=\"vsmethod\">RemoveFromGame(int)<\/a>\r\n            <a href=\"CustomGame\/Interact\/SwapAll.html\" class=\"vsmethod\">SwapAll()<\/a>\r\n            <a href=\"CustomGame\/Interact\/SwapTeam.html\" class=\"vsmethod\">SwapTeam(int)<\/a>\r\n            <a href=\"CustomGame\/Interact\/SwapToBlue.html\" class=\"vsmethod\">SwapToBlue(int)<\/a>\r\n            <a href=\"CustomGame\/Interact\/SwapToNeutral.html\" class=\"vsmethod\">SwapToNeutral(int)<\/a>\r\n            <a href=\"CustomGame\/Interact\/SwapToRed.html\" class=\"vsmethod\">SwapToRed(int)<\/a>\r\n            <a href=\"CustomGame\/Interact\/SwapToSpectators.html\" class=\"vsmethod\">SwapToSpectators(int)<\/a>\r\n        <\/div>\r\n\r\n        <a href=\"CustomGame\/Maps\/CG_Maps.html\" class=\"vsclass\">CG_Maps<\/a>\r\n        <div class=\"sidebarSection\">\r\n            <a href=\"CustomGame\/Maps\/MapIDFromName.html\" class=\"vsmethod\">MapIDFromName(string)<\/a>\r\n            <a href=\"CustomGame\/Maps\/MapNameFromID.html\" class=\"vsmethod\">MapNameFromID(Map)<\/a>\r\n            <a href=\"CustomGame\/Maps\/ToggleMap.html\" class=\"vsmethod\">ToggleMap(ToggleAction, Map[])<\/a>\r\n        <\/div>\r\n\r\n        <a href=\"CustomGame\/Pause\/CG_Pause.html\" class=\"vsclass\">CG_Pause<\/a>\r\n        <div class=\"sidebarSection\">\r\n            <a href=\"CustomGame\/Pause\/Pause.html\" class=\"vsmethod\">Pause()<\/a>\r\n            <a href=\"CustomGame\/Pause\/TogglePause.html\" class=\"vsmethod\">TogglePause()<\/a>\r\n            <a href=\"CustomGame\/Pause\/Unpause.html\" class=\"vsmethod\">Unpause()<\/a>\r\n        <\/div>\r\n\r\n        <a href=\"CustomGame\/PlayerInfo\/CG_PlayerInfo.html\" class=\"vsclass\">CG_PlayerInfo<\/a>\r\n        <div class=\"sidebarSection\">\r\n            <a href=\"CustomGame\/PlayerInfo\/GetHero.html\" class=\"vsmethod\">GetHero(int)<\/a>\r\n            <a href=\"CustomGame\/PlayerInfo\/GetQueueTeam.html\" class=\"vsmethod\">GetQueueTeam(int)<\/a>\r\n            <a href=\"CustomGame\/PlayerInfo\/IsHeroChosen.html\" class=\"vsmethod\">IsHeroChosen(int, bool)<\/a>\r\n            <a href=\"CustomGame\/PlayerInfo\/IsUltimateReady.html\" class=\"vsmethod\">IsUltimateReady(int)<\/a>\r\n            <a href=\"CustomGame\/PlayerInfo\/MaxPlayerCount.html\" class=\"vsmethod\">MaxPlayerCount()<\/a>\r\n            <a href=\"CustomGame\/PlayerInfo\/ModeratorSlot.html\" class=\"vsmethod\">ModeratorSlot()<\/a>\r\n            <a href=\"CustomGame\/PlayerInfo\/PlayerExists.html\" class=\"vsmethod\">PlayerExists(string)<\/a>\r\n            <a href=\"CustomGame\/PlayerInfo\/PlayersDead.html\" class=\"vsmethod\">PlayersDead(bool)<\/a>\r\n        <\/div>\r\n\r\n        <a href=\"CustomGame\/Command\/Commands.html\" class=\"vsclass\">Commands<\/a>\r\n\r\n        <a href=\"CustomGame\/GetCurrentOverwatchEvent.html\" class=\"vsmethod\" style=\"margin-top: 10px;\">GetCurrentOverwatchEvent()<\/a>\r\n        <a href=\"CustomGame\/GetGameState.html\" class=\"vsmethod\">GetGameState()<\/a>\r\n        <a href=\"CustomGame\/GetInvitedCount.html\" class=\"vsmethod\">GetInvitedCount(List&lt;int&gt;)<\/a>\r\n        <a href=\"CustomGame\/GetInvitedSlots.html\" class=\"vsmethod\">GetInvitedSlots(List&lt;int&gt;)<\/a>\r\n        <a href=\"CustomGame\/InvitePlayer.html\" class=\"vsmethod\">InvitePlayer(string, InviteTeam)<\/a>\r\n        <a href=\"CustomGame\/IsSlotBlue.html\" class=\"vsmethod\">IsSlotBlue(int)<\/a>\r\n        <a href=\"CustomGame\/IsSlotInQueue.html\" class=\"vsmethod\">IsSlotInQueue(int)<\/a>\r\n        <a href=\"CustomGame\/IsSlotRed.html\" class=\"vsmethod\">IsSlotRed(int)<\/a>\r\n        <a href=\"CustomGame\/IsSlotSpectator.html\" class=\"vsmethod\">IsSlotSpectator(int)<\/a>\r\n        <a href=\"CustomGame\/IsSlotValid.html\" class=\"vsmethod\">IsSlotValid(int)<\/a>\r\n        <a href=\"CustomGame\/RestartGame.html\" class=\"vsmethod\">RestartGame()<\/a>\r\n        <a href=\"CustomGame\/SendServerToLobby.html\" class=\"vsmethod\">SendServerToLobby()<\/a>\r\n        <a href=\"CustomGame\/SetHeroRoster.html\" class=\"vsmethod\">SetHeroRoster(ToggleAction, BotTeam, Hero[])<\/a>\r\n        <a href=\"CustomGame\/SetHeroSettings.html\" class=\"vsmethod\">SetHeroSettings(SetHero[])<\/a>\r\n        <a href=\"CustomGame\/StartGame.html\" class=\"vsmethod\">StartGame()<\/a>\r\n        <a href=\"CustomGame\/StartGamemode.html\" class=\"vsmethod\">StartGamemode()<\/a>\r\n\r\n        <a class=\"vsproperty\">BlueCount<\/a>\r\n        <a class=\"vsproperty\">BlueSlots<\/a>\r\n        <a class=\"vsproperty\">PlayerCount<\/a>\r\n        <a class=\"vsproperty\">PlayerSlots<\/a>\r\n        <a class=\"vsproperty\">QueueCount<\/a>\r\n        <a class=\"vsproperty\">QueueSlots<\/a>\r\n        <a class=\"vsproperty\">RedCount<\/a>\r\n        <a class=\"vsproperty\">RedSlots<\/a>\r\n        <a class=\"vsproperty\">SpectatorCount<\/a>\r\n        <a class=\"vsproperty\">SpectatorSlots<\/a>\r\n        <a class=\"vsproperty\">TotalPlayerCount<\/a>\r\n        <a class=\"vsproperty\">TotalPlayerSlots<\/a>\r\n\r\n        <a href=\"CustomGame\/OnGameOver.html\" class=\"vsevent\">OnGameOver<\/a>\r\n    <\/div>\r\n<\/div>\r\n\r\n<hr \/>\r\n<a href=\"Tag.html\">1. Creating a game of tag<\/a>\r\n<a href=\"MapVoting.html\">2. Map voting system<\/a>";