/// <binding AfterBuild='default' Clean='clean' />
const gulp = require("gulp"),
    rimraf = require("rimraf"),
    minify = require('gulp-clean-css'),
    sass = require("gulp-sass"),
    ren = require("gulp-rename");

sass.compiler = require('node-sass');

var files = {
    css: {
        spectre: "./Styles/blazorgrid-spectre.scss",
        bootstrap: "./Styles/blazorgrid-bootstrap.scss"
    }
};

gulp.task("clean", (done) => {
    rimraf("wwwroot/dist/**/*", done);
});

gulp.task("watch", () => {
    gulp.watch([files.css.spectre, files.css.bootstrap], (done) => gulp.parallel(
        cssTask(files.css.spectre),
        cssTask(files.css.bootstrap)
    )(done));
});

function cssTask(src) {
    return function (done) {
        gulp.src(src)
            .pipe(sass().on("error", sass.logError))
            .pipe(minify())
            .pipe(ren({
                extname: ".min.css"
            }))
            .pipe(gulp.dest("./wwwroot/dist"));

        done();
    }
}

gulp.task("default", (done) => {
    return gulp.parallel(
        cssTask(files.css.spectre),
        cssTask(files.css.bootstrap)
    )(done);
});