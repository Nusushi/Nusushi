/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/
"use strict";

var gulp = require('gulp');

var californiaGenerator = require('./generator-scripts');

gulp.task('bundleCaliforniaRelease', function () {
    process.env.CALIFORNIA_ENVIRONMENT = 'Production';
    return californiaGenerator.bundleCalifornia(function (err, text) { err ? console.log(err) : console.log(text) });
});

gulp.task('bundleCaliforniaDebug', function () {
    process.env.CALIFORNIA_ENVIRONMENT = 'Development';
    return californiaGenerator.bundleCalifornia(function (err, text) { err ? console.log(err) : console.log(text) });
});

gulp.task('bundleCaliforniaDebugWatched', function () {
    process.env.CALIFORNIA_ENVIRONMENT = 'Development';
    return californiaGenerator.bundleCaliforniaWatched(function (err, text) { err ? console.log(err) : console.log(text) });
});