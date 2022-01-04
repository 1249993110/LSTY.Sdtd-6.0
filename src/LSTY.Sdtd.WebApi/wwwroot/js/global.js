if(window.top === window.self){

    window.accessToken = '';

    top.NProgress.configure({ showSpinner: false });
    
    axios.interceptors.request.use(function (config) {
        top.NProgress.start();
        return config;
    });
    
    axios.interceptors.response.use(function (response) {
        top.NProgress.done();
        return response;
    }, function (error) {
        top.NProgress.done();
        if (error.response) {
            var result = error.response.data;
            switch (result.code) {
                case 400:
                case 500:
                    top.Vue.prototype.$message.error(result.message);
                    break;
                default:
                    top.Vue.prototype.$message.error(error.toJSON());
            }
        } else {
            top.Vue.prototype.$message.error(error.toJSON());
        }
    
        return Promise.reject(error);
    });
    
    top.Vue.prototype.$EventBus = new Vue();
}

