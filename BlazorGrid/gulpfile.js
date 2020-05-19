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
            main: "./Content/BlazorGrid.scss"
        }
    }
};

gulp.task("watch", () => {
    gulp.watch("./Content/**/*", compileCss);
});

gulp.task("clean", (done) => {
    rimraf("wwwroot/dist/**/*", done);
});

function compileCss(done) {
    gulp.src(files.css.themes.main)
        .pipe(sass().on("error", sass.logError))
        .pipe(minify())
        .pipe(ren("blazor-grid.min.css"))
        .pipe(gulp.dest("./wwwroot/dist"));

    done();
}

gulp.task("default", (done) => {
    return gulp.series(
        compileCss
    )(done);
});