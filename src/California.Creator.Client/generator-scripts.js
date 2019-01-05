"use strict";

var gulp = require('gulp'),
    webpack = require('webpack-stream'),
    exec = require('child_process').exec;

var isWin = /^win/.test(process.platform);

module.exports = {
    // functions are also called by node services runtime (Visual Studio)
    // expected result is string or error

    bundleCalifornia: function (cb) {
        // node node_modules/webpack/bin/webpack.js --config Client/webpack.site.js
        return gulp.src('.')
        .pipe(webpack(require('./webpack.site.js'), null, function (err, stats) {
            err ? cb(err, false) : cb(null, stats.toString());
        })).on('error', function (err) {
            cb(err, false);
            this.emit('end');
        })
        .pipe(gulp.dest('../Nusushi/wwwroot/assets'));
    },
	
	bundleCaliforniaWatched: function (cb) {
        // node node_modules/webpack/bin/webpack.js --config Client/webpack.site.js --watch
        return gulp.src('.')
        .pipe(webpack(require('./webpack.site-watched.js'), null, function (err, stats) {
            err ? cb(err, false) : cb(null, stats.toString());
        })).on('error', function (err) {
            cb(err, false);
            this.emit('end');
        })
        .pipe(gulp.dest('../Nusushi/wwwroot/assets'));
    },

    nodeservice: function (cb, name) {
        var gulp = require('gulp');
        // reload settings (ftp data etc)
        delete require.cache[require.resolve('./gulpfile')];
        var tasks = require('./generator-scripts');
        tasks[name](cb);
    }
};