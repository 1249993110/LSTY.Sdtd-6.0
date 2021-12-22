const accessToken = '';
axios.defaults.baseURL = 'http://localhost:8089/api';
if (!!this.accessToken) {
    axios.defaults.headers.common['access-token'] = this.accessToken;
}

const api = {};

api.executeConsoleCommand = function (command, inMainThread) {
    return axios.get('/ServerManage/ExecuteConsoleCommand', {
        params: {
            command: command,
            inMainThread: inMainThread ? true : false
        }
    });
}
