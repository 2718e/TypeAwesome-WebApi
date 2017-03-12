TypeAwesomeWebApi
=================

TypeAwesomeWebApi is a small C# program which uses the reflection feature to export .net Web Api Controller methods to a typescript file containing functions for calling them.
Also creates interfaces (and enums if applicable) to describe the shape of data returned from and passed to those functions.

Made this to help with development on a different project. Open sourcing in case someone finds it useful. However, be aware that (currently) this has only 
been tested or verified implicitly by checking the project it was used in worked as a whole - as such not all potential configurations are tested.

Also be aware this only currently supports the subset of Web Api features that were used in aforementioned project. In particular, the following constraints are assumed:
* no route parameters (i.e. all parameters to controller methods are passed either through body or query string)
* all urls in your project are of the form api/controller/action.
* you use modules (the export will be generated in a module)
* the controllers you want to export are in a separate assembly or assembies from things you don't.

By default, the exported functions will call the Backend via JQuery.Ajax and return PromiseLike objects. This can be configured by setting HeaderOverride in the config and providing your
own generic functions to call the backend. You can also set the ImportStatements parameter if, upon doing this, you do not require JQuery or Lodash anymore. 

Of course, feel free to modify the code or raise issues if you want it to support more things.

## Usage

A goal of this project is to be agnostic to how it is integrated in the build/development workflow, so is provided as a class, TypescriptMaker that 
can be instantiated wherever you want. You are expected to pass a TypescriptMakerConfig object to the constructor, and specify what Assemlies to export web Api controllers from
via setting the ControllerAssemblies property. All other properties of the config object can be left unset and TypescriptMaker will assume reasonable defaults.

THis repository contains some examples of ways to call TypescriptMaker. Some of these are:

* Making a small console app to call the function and write the output to file, and running that app in a build event (example: the "Runner" project).
* Exporting the api from the api itself (example: ServerMethods.SampleController.ApiDescription - to see this run WebApiApp, and go to localhost:your-debug-port/api/sample/apidescription) 

## License

Licensed under [MIT License](https://opensource.org/licenses/MIT).

Note though that any third party software used by this project may have different licenses, and it is your responsibilty to find out what they are and make sure you comply with them.