var webpack = require('webpack');
// TODO is used by all build targets (not only release)
module.exports = {
    plugins: [
        new webpack.optimize.UglifyJsPlugin({ compress: { warnings: true }, output: { comments: false } }),
        new webpack.DefinePlugin({ 'process.env.NODE_ENV': '"production"' })
    ]
};
