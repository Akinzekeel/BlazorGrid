/// <binding AfterBuild='default' Clean='clean' />
const gulp = require("gulp"),
    rimraf = require("rimraf"),
    minify = require('gulp-clean-css'),
    sass = require("gulp-sass"),
    ren = require("gulp-rename");

sass.compiler = require('node-sass');

var files = {
    css: {
        themes: {
            main: "./Styles/app.scss"
        }
    }
};

gulp.task("watch", () => {
    gulp.watch("./Styles/**/*", compileCss);
    gulp.watch("./../BlazorGrid/Styles/**/*", compileCss);
});

gulp.task("clean", (done) => {
    rimraf("wwwroot/dist/**/*", done);
});

function compileCss(done) {
    gulp.src(files.css.themes.main, { allowEmpty: true })
        .pipe(sass().on("error", sass.logError))
        .pipe(minify())
        .pipe(ren("app.min.css"))
        .pipe(gulp.dest("./wwwroot/dist"));

    done();
}

function copyContent(done) {
    gulp.src("./Content/**/*")
        .pipe(gulp.dest("./wwwroot/dist"));

    gulp.src("./Styles/font-awesome/fonts/**/*")
        .pipe(gulp.dest("./wwwroot/dist/fonts"));

    done();
}

gulp.task("default", (done) => {
    return gulp.series(
        compileCss,
        copyContent
    )(done);
});