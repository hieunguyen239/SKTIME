var gulp = require('gulp');
var sass = require('gulp-sass');
var concat = require('gulp-concat');
var uglify = require('gulp-uglify');

var config = {
    src: {
        scripts: {
            base: ['Scripts/App/base.js']
        },
        styles: {
            base: ['Content/App/base.scss'],
            login: ['Content/App/login.scss'],
            home: ['Content/App/home.scss']
        }
    }
}

gulp.task('style-base', function () {
    return gulp.src(config.src.styles.base)
        .pipe(sass().on('error', sass.logError))
        .pipe(concat('base.css'))
        .pipe(gulp.dest('./Content/App/'));
});
gulp.task('style-login', function () {
    return gulp.src(config.src.styles.login)
        .pipe(sass().on('error', sass.logError))
        .pipe(concat('login.css'))
        .pipe(gulp.dest('./Content/App/'));
});
gulp.task('style-home', function () {
    return gulp.src(config.src.styles.home)
        .pipe(sass().on('error', sass.logError))
        .pipe(concat('home.css'))
        .pipe(gulp.dest('./Content/App/'));
});
gulp.task('script-base', function () {
    return gulp.src(config.src.scripts.base)
      .pipe(uglify())
      .pipe(concat('base.min.js'))
      .pipe(gulp.dest('./Scripts/App/'));
});

gulp.task('default', function () {
    gulp.watch(config.src.styles.base, ['style-base']);
    gulp.watch(config.src.styles.login, ['style-login']);
    gulp.watch(config.src.styles.home, ['style-home']);

    gulp.watch(config.src.scripts.base, ['script-base']);
});