/*
This file in the main entry point for defining grunt tasks and using grunt plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkID=513275&clcid=0x409
*/
module.exports = function (grunt) {
  grunt.initConfig({
    pkg: grunt.file.readJSON('package.json'),

    clean: ['SwaggerUi'],
    
    bowercopy: {
      options: {
        runBower: true
      },
      libs: {
        files: {
          "SwaggerUi": "swagger-ui/dist"
        }
      }
    }
  });

  grunt.loadNpmTasks('grunt-contrib-clean');
  grunt.loadNpmTasks('grunt-bowercopy');

  grunt.registerTask('default', [ 'clean', 'bowercopy:libs' ]);
};