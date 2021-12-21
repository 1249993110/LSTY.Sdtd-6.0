axios.interceptors.request.use(function (config) {
    top.NProgress.start();
    return config;
});

axios.interceptors.response.use(function (response) {
    top.NProgress.done();
    return response;
}, function (error) {
    top.NProgress.done();
    var result = error.response.data;
    switch (result.code) {
        case 400:
        case 500:
            Vue.prototype.$message.error(result.message);
            break;
        default:
            Vue.prototype.$message.error(error.toJSON());
    }

    return Promise.reject(error);
});