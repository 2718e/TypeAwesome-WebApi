TypeAwesomeWebApi
=================

TypeAwesomeWebApi is a small C# program which uses the reflection feature to export .net Web Api Controller methods to a typescript file containing functions for calling them.
Also creates interfaces (and enums if applicable) to describe the shape of data returned from and passed to those functions.

Made this to help with development on a different project. Open sourcing in case someone finds it useful. However, be aware that (currently) this has only 
been tested or verified implicitly by checking the project it was used in worked as a whole.

Also be aware this only currently supports the subset of Web Api features that were used in aforementioned project. In particular, the following constraints are assumed:
* no route parameters (i.e. all parameters to controller methods are passed either through body or query string)
* all urls in your project are of the form api/controller/action.
* you already use jquery and lodash (the generated typescript imports these libraries)
* you use modules (the export will be generated in a module)
* the controllers you want to export are in a separate assembly from things you don't.

Of course, feel free to modify the code or raise issues if you want it to support more things.

## Usage

A goal of this project is to be agnostic to how it is integrated in the build/development workflow, so is provided as a 
static function of IEnumerable<Assembly> => string, namely TypeAwesomeWebApi.TypescriptMaker.MakeScriptsFrom. 
You can call this function however you want. Some possibliities are:

* Making a small console app to call the function and write the output to file, and running that app in a build event (example: the "Runner" project).
* Exporting the api from the api itself (example: ServerMethods.SampleController.ApiDescription - to see this run WebApiApp, and go to localhost:your-debug-port/api/sample/apidescription) 

## License

Licensed under [MIT License](https://opensource.org/licenses/MIT).

Note though that any third party software used by this project may have different licenses, and it is your responsibilty to find out what they are and make sure you comply with them.