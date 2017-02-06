TypeAwesomeWebApi
=================

TypeAwesomeWebApi is a small C# program which uses the C# reflection feature to export .net Web Api Controller methods to a typescript file containing functions for calling them.
Also creates interfaces to describe the shape of data returned from and passed to those functions.

Please note that at the time of writing this is still in development (pre alpha) and utterly untested.

Also note that this project is designed for one specific way of using web api and may not support everything possible. In particular, it assumes:
* no route parameters (i.e. all parameters to controller methods are passed either through body or query string)
* all urls in your project are of the form api/controller/action.
* you already use jquery and lodash in your client side code (the generated typescript imports these libraries)
* you use modular javascript in client side code (the export will be generated in a module)

## Usage

Call TypeAwesomeWebApi.TypescriptMaker.MakeScriptsFrom with an IEnumerable of Assembly objects containing the controllers you want to export, and a string giving the name of the 
module you want the exports to be generated in. This will return the export as a string. It is up to you how, where and when you want to call it and what you want to do with the output.

## License

Licensed under [MIT License](https://opensource.org/licenses/MIT).

Note though that any third party software used by this project may have different licenses, and it is your responsibilty to find out what they are and make sure you comply with them.