/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/
"use strict";

var gulp = require('gulp');

var tokyoTrackerGenerator = require('./generator-scripts');

gulp.task('bundleTokyoTrackerRelease', function () {
    process.env.TOKYOTRACKER_ENVIRONMENT = 'Production';
    return tokyoTrackerGenerator.bundleTokyoTracker(function (err, text) { err ? console.log(err) : console.log(text) });
});

gulp.task('bundleTokyoTrackerDebug', function () {
    process.env.TOKYOTRACKER_ENVIRONMENT = 'Development';
    return tokyoTrackerGenerator.bundleTokyoTracker(function (err, text) { err ? console.log(err) : console.log(text) });
});

gulp.task('bundleTokyoTrackerDebugWatched', function () {
    process.env.TOKYOTRACKER_ENVIRONMENT = 'Development';
    return tokyoTrackerGenerator.bundleTokyoTrackerWatched(function (err, text) { err ? console.log(err) : console.log(text) });
});