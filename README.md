# <img height="30" src="./ScChrom_logo.svg"> ScChrom

ScChrom (**Sc**riptable **Chrom**ium) is an open source, easy to deploy and extensible 
Chromium based highly configurable browser build with CefSharp and Jint. It is suited for many 
use cases like:

- Enhancing/customizing web applications, 
- automated testing of web sites and applications,
- complex webscraping that may or may not require user input, 
- quick creation of desktop applications based mainly on web technologies.   

ScChrom is completely configured via config files and/or command line arguments, making it as easy
as sharing the config file to share the logic of your ScChrom based application.


## Quickstart:
1. Downloaded the latest [ScChrom release](https://github.com/Felmachersoft/ScChrom/releases).
2. Run the ScChrom.exe, select a destination folder and start downloading all dependencies (around 300 Mb).
3. After the restart check out the example scripts to get an idea what is possible and use 
 the build-in editor to create your own config files.


## Support
This page provides general information about ScChrom. More information about specific parameters,
[JavaScript controllers](#javascript-controller) as well as many examples are integrated into 
ScChrom itself. If, after reading this page and trying out ScChrom, you are unsure if it fits your
use case, have questions about how to use it, want to request a new feature or found a bug, get in
contact with us:
- [ScChroms GitHub issue tracker](https://github.com/Felmachersoft/ScChrom/issues)
- [Contact form on our webpage](https://felmachersoft.de/en) 
- E-mail us at `contact@felmachersoft(dot)de`. 

## Architecture
<img src="./architecture.svg">

ScChrom is built on top of [Chromium](https://www.chromium.org/Home) using 
[CefSharp](https://github.com/cefsharp/CefSharp). As the base for many modern browsers Chromium 
comes with a build in JavaScript interpreter operating in the browser context. ScChrom also 
allows JavaScript execution in its own context using the .net based 
**J**avascript **int**erpretor [Jint](https://github.com/sebastienros/jint). <br>
ScChrom is configured via parameters. Some of them control ScChroms behavior, some change 
settings of chromium, other provide ways to inject JavaScript in the mentioned contexts.
[JavaScript controllers](#javascript-controller) provide a simple way to add functionality that
can be called from within the JavaScript contexts.

## Setup / Update
The ScChrom.exe alone doesn't come with all dependencies included. If started without the dependencies in place it will automatically show the setup window.
If a newer version is available ScChrom shows it in the main window on the upper right when started without parameters. <br>
**BEWARE:** The update will remove everything but *.sccf-files and directories not related to Cef from the directory of the ScChrom.exe.

### Command line setup arguments
ScChrom can also be setup and updated via command line. Following command line arguments are available (if any of them are used all following parameters are ignored):
  - `check` <br>
    Checks if ScChrom is correctly set up, if not the exit code won't be 0.
  - `version` <br>
    Writes ScChroms version to the standard output.
  - `latest_version_info` <br>
    Writes multiple lines to the standard output. The first line is the latest
    online available version of ScChrom. The second line is the direct download URL of the latest ScChrom release. The last line ends with *False* if a newer version is available.
  - `cef_version` <br>
    Writes the necessary Cef version to the standard output.
  - `dependencies` <br>
    Writes the names and versions of the dependencies as used by [NuGet](https://www.nuget.org/) separated by a : to the standard output.
  - `update_args` <br>
    Writes the command line arguments that can be used to update this ScChrom instance. An example for the written out line (for a ScChrom.exe placed in C:\ScChrom):
    ```
    cmd_install,processId:1244,installedDependencies:CefSharp.Common/85.3.130:CefSharp.WinForms/85.3.130:jint/3.0.0-beta-1632:esprima/1.0.1251:Newtonsoft.Json/12.0.3:cef.redist.x64/85.3.13=C:\ScChrom
    ```        
    See `cmd_install` for details about the parameters.<br>    
  - `cmd_install` <br>
    Starts the installation or update of ScChrom with the given parameters. The argument structure is:
    ``` 
    cmd_install<list of parameters>=<destination path>
    ```
    the parameters are separated by commas. Following parameters are possible:
    - *processId*:\<processId\> <br>
    The updater/installer waits with the installation/update till the process with the given \<processId\> is no longer running.
    - *installedDependencies:*\<list of dependencies\> <br>
    The \<list of dependencies\> lists all dependencies separated by `:` and is only used for updates. It is used to check if the dependencies from the called ScChrom.exe and the updated one match. If they match only the ScChrom.exe is replace. Otherwise the destination directory is cleared (subdirectories - other than the locales and swiftshader directories - and .sccf files will not be deleted) and the new dependencies are downloaded and setup.
    - *dontCheck* <br>
    If set the installed/updated ScChrom will not be checked for missing dependencies. If not set and one or more dependencies are missing, the setup will fail with an error message.
    - *startAfter* <br>
    If set the installed/updated ScChrom.exe is started.
    - *cleanup* <br>
    Removes all but the *.sccf* files from the installation directory. Additionally, the chromium specific subdirectories *locales* and *swiftshader* are deleted.
    - *showUpdateGui* <br>
    If set a window will be shown that displays the update progress. If not set the progress will be written out to the standard output in the form of:
        ```
        Downloading next dependency
        1% finished    
        Extracting <dependency>
        ...   
        100% finished
        Extracting <another dependency>    
        Check finished
        Installation finished
        ```


### Command line setup examples
#### Installation
  An example line to setup ScChrom via the command line (the directory has to exist before execution):    
  ```
  ScChrom.exe cmd_install=C:\ScChrom
  ```
#### Update
  Here is an example workflow demonstrating how ScChrom could be updated by another program via command line commands. In this example ScChrom is correct setup in ``C:\ScChrom`` .<br>
  - Check for newer version via:
    ```
    C:\ScChrom\ScChrom.exe latest_version_info
    ```
    Example output:
    ``` 
    1.1.0.0
    https://github.com/Felmachersoft/ScChrom/releases/latest/download/ScChrom.exe
    up to date:False
    ```
    The last line ending with *False* shows that a newer version is available. <br>
  - Your application has to download the latest version of ScChrom via the provided URL. For the next steps we will assume the latest ScChrom.exe has been downloaded to ``C:\newScChrom`` .
  - Get the necessary update parameters via:
    ```
    C:\ScChrom\ScChrom.exe update_args
    ```
    Example output:
    ``` 
    cmd_install,processId:6580,installedDependencies:CefSharp.Common/85.3.130:CefSharp.WinForms/85.3.130:jint/3.0.0-beta-1632:esprima/1.0.1251:Newtonsoft.Json/12.0.3:cef.redist.x64/85.3.13=C:\ScChrom
    ```
  - Using the output of the previous step with the new downloaded ScChrom.exe will update the old version:
    ```
    C:\newScChrom\ScChrom.exe cmd_install,processId:6580,installedDependencies:CefSharp.Common/85.3.130:CefSharp.WinForms/85.3.130:jint/3.0.0-beta-1632:esprima/1.0.1251:Newtonsoft.Json/12.0.3:cef.redist.x64/85.3.13=C:\ScChrom
    ```
    After execution the ScChrom instance at ``C:\ScChrom`` is up to date.


## Parameters
ScChrom is configured by parameters provided via command line arguments and/or a config file. 
A parameter starts with `--` followed by the parameters name and the parameters value separated
by a `=` like:

`--parameter_name=parameter_value`

All used parameters with additional information can be viewed in ScChroms build in editor.
Additionally, you can use your own parameters and reference them in scripts via the 
[JsController](#javascript-controller)`ArgumentsController`.

### Stacking parameters
Some parameters support stacking, which enables reusing parameters for different cases that are 
distinguished by their respective stack key. What the stack key is used for depends on the 
parameter and can be checked at the parameters description in the editor. Many stackable 
parameters provide default fallbacks if no stack key is given. The syntax to set stack keys is 
as follows:

`--parameter_name<stack_key>=parameter_value`

##### Patterns
The stack keys for several parameters support the usage of patterns. For example, many parameters
allow patterns to distinct whether or not the parameter will be applied based on the URL of the 
page. Patterns can either be an exact match (first and last character are `"`) or a pattern that 
might contain one or more wildcards (`?` for single random character, `*` none or more random 
characters).
Some examples:
  `streambuffre.com`
  matches `https://streambuffre.com`, `streambuffre.com` and `google.com/streambuffre.com`
  but not 'test.com' for example
  
  - `"streambuffre.com"`
  only matches `streambuffre.com`, nothing else  
  - `streambuffre.com?`
  matches `streambuffre.com/` but not `streambuffre.com/license` for example  
  - `*streambuffre.com*`
  matches `https://streambuffre.com` and `streambuffre.com/license` but not 
  `google.com/` for example

### Command line arguments
ScChrom can be used like a console program that writes text to the standard output. When using 
parameters as command line arguments they need to be separated by spaces and if a 
value contains spaces it needs to be surrounded by `"` like so:

`ScChrom.exe --param1=true --param2="value with spaces" --param3<sk>=12`

Following special arguments can only be used as command line arguments:
  - `--config-file=PATH` <br>
    see next passage
  - `--config-base64=CONTENT` <br>
    can be used instead of `--config-file` to provide a complete (base64 encoded) config without using an extra file
  - `--logfile=PATH` <br>
    if set everything logged to the standard output will also be written to the given file


### Config files
In order to start ScChrom with a config file you can either use it as the only command line
argument (this is the same as dragging and dropping the config file directly onto the 
ScChrom.exe)

`ScChrom.exe "c:/myConfigfile"`

or if you want additional parameters like this

`ScChrom.exe --config-file="c:/myConfigfile" --anotherParameter=true`

In config files each parameter starts on a new line and after the `=` the value goes on until 
either another parameter starts or the config file ends (do NOT surround values with `"` even if 
they have spaces in config files):

```
--param1=true
--param2=value with spaces
--param3<sk>=12
--param4=
<style>
    body {
        background-color: gray;
    }
</style>
--param5=false
```

If the first line of a config file starts with a comment containing a json serialized object, the
content of this json object is understood by ScChrom as the config files metadata. ScChroms build 
in editor helps you editing the metadata of a config file.<br>
Here a simple example with several values:
```
//{"author":"Felmachersoft","license":"MIT"}
--parameter=true
```

In ScChrom you can browse through a list of example config files. You can also get these examples
directly from the [ScChrom_examples repository](https://github.com/Felmachersoft/ScChrom_examples).



## JavaScript contexts

Some parameters allow the injection of JavaScript into the two available contexts. Additionally to 
the standard JavaScript functions several new ones are injected by ScChrom into the contexts. In 
addition to these injected functions the [JavaScript controllers](#Javascript_controller) provide 
additional functionality and extensibility to ScChrom.

##### Jint context
In ScChroms own JavaScript context following injected functions are available:
- `getVersion() : string` : Returns the version string of ScChrom
- `log(text : string)` : Writes the given text into ScChroms log
- `error(text : string)` : Writes the given text into ScChroms log as error
- `write(text : string)` : Writes the given text to ScChroms standard output
- `writeLine(text : string)` : Writes the given text to ScChroms standard output with a new 
  line character appended
- `sleep(milliseconds : integer)` : Blocks the execution for the given amount of milliseconds

Additional functions from the [JavaScript controllers](#Javascript_controller) are called via the 
name of the JavaScript Controller and the methods like:
```
JsController.method('my input');
let value = JsController.method2();
```
All injected and via JavaScript controller provided functions are executed blocking and return their 
respective results directly as seen in the example above.

##### Browser context
In the browser context all injected functions and access to the JavaScript controllers are 
encapsulated in the *main object* called `ScChrom` by default. This main object has following 
fields:

- `version : text` : The version string of ScChrom
- `log(text : string)` : Writes the given text into ScChroms log
- `err(text : string)` : Writes the given text into ScChroms log as error
- `openLink(url : string)` : Opens the given URL with the systems default browser
- `matchText(text : string, pattern : string) : boolean` : Returns true if the given text
  [matches](#patterns)
- `addCallback(id: string, callback : function(value : string))` : Add a callback that can 
  be called from within the Jint context via 
  `BrowserController.callInBrowserCallback(id : string, value : string)`
- `removeCallback(id : string)` : Removes a callback added via `addCallback`
- `jintCallbacks : object` : stores the callbacks stored via addCallback

All functions of the *main object* and its fields are [promises](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise).
In order to get their return values you can [`await`](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/await)
them.<br>

```
let scchromVersion = ScChrom.version;

// without await this will NOT block but executed asynchronously
ScChrom.log('hello world');

// await will block the execution until the result is returned
let value = await ScChrom.JsController.method('my input');
```

__Note 1:__ In order to access functions of a JavaScript controller in the browser context they need
 to be allowed inside the browser context via the `browser-js-allow_objects` parameter.<br>
__Note 2:__ The *main object* might not be initialized once JavaScript of the page gets executed.
If you need to execute a function of the *main object* after page load use the 
`injected-javascript` parameter.



## JavaScript controller
JavaScript Controller (JsController) are implemented in C# and connect the JavaScript contexts 
with the .net code. They can be distributed as separated `.dll` files and are automatically
detected by ScChrom on startup when placed into the same directory as the `ScChrom.exe`.
For examples check out the `JsController` directory of ScChroms source code.

##### Implementation
JsControllers are C# classes that implement either the `IBrowserContextCallable` interface 
(for the browser context), the `IJintContextCallable` interface (for the Jint context) or both.
The class must be `public` and only `public` methods will be accessible from the JavaScript 
contexts. All parameters should use basic data types like `string`, `int` and so on. 

##### Documentation
All methods of all JsControllers are listed in ScChroms editor with their parameters and return
values. Additional information can be provided by implementing the getter of the following 
property:
```
public static List<JsControllerMethodInfo> MethodInfos {
    get {
        return ...
    }
}
```

## Limitations:
For now, ScChrom has following limitations:
- For now only compatible with windows 64 bit.<br>
- ScChrom can't play media encoded with propertary codecs like H.264 or H.265 (most of .mp4 files).
  [More details](https://github.com/cefsharp/CefSharp/wiki/General-Usage#multimedia-audiovideo).

## Tests
The __ScChrom_Tests project__ contains several test cases for ScChrom and some of its parameters
and JsControllers. Due to CefSharps limitation that the Chromium core can't be restarted with 
the same process, the ScChrom tests can't use Visual Studios unit tests. For every test the 
same chromium instance is reused via ScChroms restart mechanism. This prevents testing of
some parameters.<br>
In order to run the tests, start debugging the ScChrom_Tests project. While testing a small ScChrom-
window will flash several times. If all tests are passed the Output in visual studio will finish
with: `All tests passed`
<br>
__NOTE__: Sometimes parts of the cef redis binaries are not correctly copied to the tests 
binary folder (Causing a bad image or missing file exception). To fix this copy the content 
of ScChroms binary folder (ScChrom/bin/x64/Debug) into the test folder 
(ScChrom/bin/tests/Debug) after creating a ScChrom debug build.


## Used software:
#### Jint (https://github.com/sebastienros/jint)
  BSD 2-Clause License
  
  Copyright (c) 2013, Sebastien Ros
  All rights reserved.
  
  Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
  1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
  2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#### CefSharp (https://github.com/cefsharp/CefSharp)
  Copyright © The CefSharp Authors. All rights reserved.

  Redistribution and use in source and binary forms, with or without
  modification, are permitted provided that the following conditions are
  met:

   * Redistributions of source code must retain the above copyright
     notice, this list of conditions and the following disclaimer.

   * Redistributions in binary form must reproduce the above
     copyright notice, this list of conditions and the following disclaimer
     in the documentation and/or other materials provided with the
     distribution.

   * Neither the name of Google Inc. nor the name Chromium Embedded
     Framework nor the name CefSharp nor the names of its contributors
     may be used to endorse or promote products derived from this software
     without specific prior written permission.

  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
  A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
  OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
  LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
  OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
  
#### AnimatedGif (https://github.com/mrousavy/AnimatedGif)
  Copyright (c) 2017 mrousavy 
  MIT License

#### PureCss (https://github.com/pure-css/pure)
  Software License Agreement (BSD License)
  
  Copyright 2013 Yahoo! Inc.
  
  Redistribution and use in source and binary forms, with or without
  modification, are permitted provided that the following conditions are met:
  
      * Redistributions of source code must retain the above copyright
        notice, this list of conditions and the following disclaimer.
  
      * Redistributions in binary form must reproduce the above copyright
        notice, this list of conditions and the following disclaimer in the
        documentation and/or other materials provided with the distribution.
  
      * Neither the name of the Yahoo! Inc. nor the
        names of its contributors may be used to endorse or promote products
        derived from this software without specific prior written permission.
  
  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
  DISCLAIMED. IN NO EVENT SHALL YAHOO! INC. BE LIABLE FOR ANY
  DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
  LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
  ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
  SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#### Newtonsoft Json (https://www.newtonsoft.com/json)
  The MIT License (MIT)

  Copyright (c) 2007 James Newton-King
  
  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
  
  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
  
  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#### Sysem.Buffers (https://www.nuget.org/packages/System.Buffers/)
  The MIT License (MIT)

  Copyright (c) .NET Foundation and Contributors

  All rights reserved.

  Permission is hereby granted, free of charge, to any person obtaining a copy
  of this software and associated documentation files (the "Software"), to deal
  in the Software without restriction, including without limitation the rights
  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
  copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all
  copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
  SOFTWARE.

#### DnsClient.NET (https://github.com/MichaCo/DnsClient.NET)
   Copyright 2021 Michael Conrad

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
  limitations under the License.