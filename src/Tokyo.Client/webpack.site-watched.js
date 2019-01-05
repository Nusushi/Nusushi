var path = require('path');
var wrappedWebpack = path.join(__dirname, 'webpack.site.js');
var merge = require('extendify')({ isDeep: true, arrays: 'concat' });
var watchedConfig = require(wrappedWebpack);

module.exports = merge(watchedConfig, {
    watch: true,
    watchOptions: {
        poll: 1000,
    }
});
