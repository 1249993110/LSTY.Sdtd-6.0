axios.defaults.baseURL = 'http://' + window.location.host + '/api';
if (!!top.accessToken) {
    axios.defaults.headers.common['access-token'] = top.accessToken;
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

api.sendGlobalMessage = function (message, senderName = 'Server') {
    return api.sendConsoleCommand(`ty-say \"${message}\" ${senderName}`);
}

api.sendMessageToPlayer = function (playerIdOrName, message, senderName = 'Server') {
    return api.sendConsoleCommand(`ty-pm ${playerIdOrName} \"${message}\" ${senderName}`);
}

api.telePlayer = function (playerIdOrName, target) {
    return api.sendConsoleCommand(`tele ${playerIdOrName} ${target}`);
}

api.giveItem = function (playerIdOrName, itemName, count, quality = 0, durability = 0) {
    return api.sendConsoleCommand(`ty-gi ${playerIdOrName} ${itemName} ${count} ${quality} ${durability}`);
}

api.spawnEntity = function (playerNameOrEntityId, spawnEntityIdOrName) {
    return api.sendConsoleCommand(`se ${playerNameOrEntityId} ${spawnEntityIdOrName}`);
}

api.kickPlayer = function (playerIdOrName) {
    return api.sendConsoleCommand(`kick ${playerIdOrName}`);
}

/** 
 * 封禁玩家
 * @example ban add madmole 2 minutes "Time for a break" "Joel" 
 * @durationUnit minute(s), hour(s), day(s), week(s), month(s), year(s)
 */
api.banPlayer = function (playerIdOrName, duration, durationUnit, reason, displayName) {
    return api.sendConsoleCommand(`ban add ${playerIdOrName} ${duration} ${durationUnit} ${reason} ${displayName}`);
}

api.addAdmin = function (playerIdOrName, level, displayName) {
    return api.sendConsoleCommand(`admin add ${playerIdOrName} ${level} ${displayName}`);
}

api.getLivePlayers = function (realTime = false) {
    return axios.get('/LivePlayers' + (realTime ? '?realTime=true' : ''));
}

api.getLivePlayer = function (playerEntityId, realTime = false) {
    return axios.get('/LivePlayers/' + playerEntityId + (realTime ? '?realTime=true' : ''));
}

api.getPlayerInventory = function (playerEntityId) {
    return axios.get('/PlayerInventory/' + playerEntityId);
}

api.getHistoryPlayers = function (params) {
    return axios.get('/HistoryPlayers', {
        params: params
    });
}

api.getChatRecord = function (params) {
    return axios.get('/ChatRecord', {
        params: params
    });
}