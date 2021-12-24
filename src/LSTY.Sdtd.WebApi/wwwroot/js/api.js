const accessToken = '';
axios.defaults.baseURL = 'http://localhost:8089/api';
if (!!accessToken) {
    axios.defaults.headers.common['access-token'] = accessToken;
}

const api = {};

api.sendConsoleCommand = function (command, inMainThread = false) {
    return axios.get('/ServerManage/ExecuteConsoleCommand', {
        params: {
            command: command,
            inMainThread: inMainThread
        }
    });
}

api.sendGlobalMessage = function (message, senderName) {
    return api.executeConsoleCommand(`ty-say \"${message}\" ${senderName}`);
}

api.sendMessageToPlayer = function (playerIdOrName, message, senderName) {
    return api.executeConsoleCommand(`ty-pm ${playerIdOrName} \"${message}\" ${senderName}`);
}

api.telePlayer = function (playerIdOrName, targetId) {
    return api.executeConsoleCommand(`tele ${playerIdOrName} ${targetId}`);
}

api.giveItem = function (playerIdOrName, itemName, count, quality = 0, durability = 0) {
    return api.executeConsoleCommand(`ty-gi ${playerIdOrName} ${itemName} ${count} ${quality} ${durability}`);
}

api.spawnEntity = function (playerNameOrEntityId, spawnEntityIdOrName) {
    return api.executeConsoleCommand(`se ${playerNameOrEntityId} ${spawnEntityIdOrName}`);
}

api.kickPlayer = function (playerIdOrName) {
    return api.executeConsoleCommand(`kick ${playerIdOrName}`);
}

/** 
 * 封禁玩家
 * @example ban add madmole 2 minutes "Time for a break" "Joel" 
 * @durationUnit minute(s), hour(s), day(s), week(s), month(s), year(s)
 */
api.banPlayer = function (playerIdOrName, duration, durationUnit, reason, displayName) {
    return api.executeConsoleCommand(`ban add ${playerIdOrName} ${duration} ${durationUnit} ${reason} ${displayName}`);
}

api.addAdmin = function (playerIdOrName, level, displayName) {
    return api.executeConsoleCommand(`admin add ${playerIdOrName} ${level} ${displayName}`);
}

api.getLivePlayers = function () {
    return axios.get('/ServerManage/LivePlayers');
}

api.getLivePlayer = function (playerEntityId) {
    return axios.get('/ServerManage/LivePlayers/' + playerEntityId);
}

api.getPlayerInventory = function (playerEntityId) {
    return axios.get('/ServerManage/PlayerInventory/' + playerEntityId);
}