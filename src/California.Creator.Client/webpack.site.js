var path = require('path');
var webpack = require('webpack');
const ExtractTextPlugin = require("extract-text-webpack-plugin");
const extractCSS = new ExtractTextPlugin('californiaclient.css');
var merge = require('extendify')({ isDeep: true, arrays: 'concat' });
var devConfig = require('./webpack.config.dev');
var prodConfig = require('./webpack.config.prod');
var isDevelopment = process.env.CALIFORNIA_ENVIRONMENT === 'Development';
// TODO reveal hidden modules
// TODO create one script for static/vendor assets, one for current work in progress javascript

module.exports = merge({
    resolve: {
        alias: { 'jquery': path.resolve(path.join('node_modules', 'jquery')) }, // import(jquery) in dependency (in this case bootstrap) led to different jquery module being (re)imported
        extensions: [ '.js', '.ts', '.tsx' ]
    },
    module: {
        loaders: [
            { test: /\.(png|jpg|gif|eot|svg|ttf|woff|woff2)/, loader: 'url-loader?limit=10000' },
            { test: /\.css/, loader: extractCSS.extract('css-loader?sourceMap') },
            { test: /\.scss/, loader: extractCSS.extract('css-loader?sourceMap!sass-loader?sourceMap') },
            { test: /\.ts(x)?$/, loader: 'babel-loader!awesome-typescript-loader' }
        ]
    },
    entry: {
        californiaclient: ['jquery', 'jquery-validation', 'jquery-validation-unobtrusive', 'tether', 'popper.js', /*'bootstrap', 'bootstrap/dist/css/bootstrap.css',*/ './sass/main.scss', './site.tsx']
    },
    output: {
        path: path.join(__dirname, '..', 'Nusushi', 'wwwroot', 'assets'),
        filename: '[name].js',
        publicPath: '/assets/',
        library: 'Californiaclient'
    },
    plugins: [
        extractCSS,
        new webpack.ProvidePlugin({
            $: "jquery",
            jQuery: "jquery",
            "window.jQuery": "jquery",
            Tether: "tether",
            Util: "exports?Util!bootstrap/js/dist/util",
            Popper: ['popper.js', 'default']
        })/*,
        new webpack.DllReferencePlugin({
            context: __dirname,
            manifest: path.join(__dirname, '..', 'wwwroot', 'assets', 'vendor-manifest.json'),
            name: "vendor.js",
            sourceType: "this"
        })*/
    ]
}, isDevelopment ? devConfig : prodConfig);
