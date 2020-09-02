using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom.ConfigParams {

    public enum ConfigParameterType {
        None,
        Boolean,
        Uri,
        Text,
        Javascript,
        Number,
        Enumeration,
        Html
    }

    public class Example {
        public string Description { get; set; }
        public string Content { get; set; }
    }

    public class Enumerationvalues {
        public string Value { get; set; }
        public string Description { get; set; }
    }

    public class ConfigParameter {
        public string Name { get; set; }
        public string Information { get; set; }
        public string DefaultValue { get; set; }
        public string CategoryName { get; set; }
        public bool Stackable { get; set; }
        public List<Example> Examples { get; set; }
        public ConfigParameterType ParameterType { get; set; }
        public string ParamTypeName { 
            get {
                return Enum.GetName(typeof(ConfigParameterType), ParameterType);
            }
            set { 
                // only here to include this property in the serialzation
            }
        }
        public List<Enumerationvalues> EnumerationValues { get; set; }
        public override string ToString() {
            return "ConfigParameter " + Name;
        }
    }

    public static class ConfigParamFactory {
        private static Dictionary<string, ConfigParameter> _cachedConfigParams = null;
        
        public static IReadOnlyDictionary<string, ConfigParameter> AllParameters {
            get {
                if (_cachedConfigParams != null)
                    return _cachedConfigParams;

                _cachedConfigParams = new Dictionary<string, ConfigParameter>() {
// basic
                    /*
                    { "config-file", new ConfigParameter() {
                        Name = "config-file",
                        Information = @"Sets a config file to read the configuration from. All configurations can be written down there instead of using command line arguments. Values from the config file will be overwritten by command line arguments.  The value for a parameter can use multiple lines allowing multi line scripts.<br>
                                        If the config file could not be found the program will imidiatly exit with code 1, or with exit code 2 if the file could not be parsed. <br>
                                        Lines starting with // are ignored and can be used for comments. <br>
                                        Do not surround values with spaces with """", values are separated by new lines.",
                        CategoryName = "basic",
                        DefaultValue = null,
                        ParameterType = ConfigParameterType.Uri
                    }},*/
                    { "url", new ConfigParameter() {
                        Name = "url",
                        Information = @"Sets the initial page to load, can be an internet address or path to a local file.<br>
                                 Will be ignored, if the 'html' parameter is set.",
                        CategoryName = "basic",
                        DefaultValue = null,
                        Examples = new List<Example>() {
                            new Example() {
                                Content = @"--url=https://streambuffre.com",
                                Description = "Opens StreamBuffRes main page."
                            }
                        },
                        ParameterType = ConfigParameterType.Uri
                    }},
                    { "html", new ConfigParameter() {
                        Name = "html",
                        Information = "Set the html to load instead of loading it from an address. Can be used to create self-contained application. If set the url parameter will be ignored.",
                        CategoryName = "basic",
                        DefaultValue = null,
                        Examples = new List<Example>() {
                            new Example() {
                                Content = @"--html=
                                                <html>
                                                    <head></head>
                                                    <body>
                                                        <h1>This is a test page</h1>
                                                        <script>
                                                            alert('This is the script of the test html')
                                                        </script>
                                                    </body>
                                                </html>",
                                Description = "Shows a testpage with an alert box."
                            }
                        },
                        ParameterType = ConfigParameterType.Html
                    }},
                    { "log-level", new ConfigParameter() {
                        Name = "log-level",
                        Information = @"Sets the log-level. Every value other than the valid ones will default to error.",
                        CategoryName = "basic",
                        DefaultValue = "error",
                        EnumerationValues = new List<Enumerationvalues>() {
                            new Enumerationvalues() { Value = "none",   Description = "no logging at all"},
                            new Enumerationvalues() { Value = "error",  Description = "only errors"},
                            new Enumerationvalues() { Value = "info",   Description = "errors and infos"},
                            new Enumerationvalues() { Value = "debug",  Description = "debug output, infos and errors, might be slow!"}
                        },
                        ParameterType = ConfigParameterType.Enumeration
                    }},
                    { "output-browser-console", new ConfigParameter() {
                        Name = "output-browser-console",
                        Information = @"Set to true to redirect the output of the chromium JavaScript console to this programs console.",
                        CategoryName = "basic",
                        DefaultValue = "false",
                        ParameterType = ConfigParameterType.Boolean
                    }},
                    { "max-runtime", new ConfigParameter() {
                        Name = "max-runtime",
                        Information = @"Sets the max timespan in seconds the script is allowed to run.
                            When the timespan is over, the program will terminate itself.<br>
                            If set to 0 or negative values, the script has no timelimit.",
                        CategoryName = "basic",
                        DefaultValue = "0",
                        ParameterType = ConfigParameterType.Number
                    }},
                    { "cache-disabled", new ConfigParameter() {
                        Name = "cache-disabled",
                        Information = @"Disables the browser cache. This is the same as setting the --disable-application-cache command line argument for cef based applications.",
                        DefaultValue = "false",
                        ParameterType = ConfigParameterType.Boolean,
                        CategoryName = "basic"
                    }},
                    { "cache-path", new ConfigParameter() {
                        Name = "cache-path",
                        Information = @"Sets the path to the browsers cache folder. 
                            If set to false no cache folder will be used; the browser will act like in incognito mode and loose all session info upon closing. This includes the GPUcache folder.
                            The localstorage and cookies are included in the cache. A change in the setting requires ScChrom to restart.
                            Relative path will be relative to the executable of ScChrom and must NOT start with a slash. <br>
                            Special folders like %USERPROFILE%\Desktop can be used. Also the special variable %configfile_dir% can be used. It points at the directory of the loaded config file or, if none used, the current working directory.",
                        DefaultValue = "false",
                        ParameterType = ConfigParameterType.Text,
                        CategoryName = "basic"
                    }},
                    { "useragent", new ConfigParameter() {
                        Name = "useragent",
                        Information = @"Sets the useragent used for requests. Whitespace before and after the text will be removed. 
                            Will default to chromiums default useragent if not set or only whitespace.",
                        ParameterType = ConfigParameterType.Text,
                        CategoryName = "basic"
                    }},
                    { "proxy-settings", new ConfigParameter() {
                        Name = "proxy-settings",
                        Information = @"Set proxy settings for CEF. The value contains the Ip, Port, username, password, ignorelist for the proxy separated by <b>|</b>.
                            Setting name and password might not work. In this case use a proxy forwarder that injects the name and password for you (search for proxy-login-automator).<br>
                            In order to specify a protocol (one of http (default), socks, socks4 or socks5) prepend it to the Ip like: <i>socks://localhost|8080</i>.<br>
                            The ignorelist contains domains separated by <b>;</b>",
                        ParameterType = ConfigParameterType.Text,
                        CategoryName = "basic",
                        Examples = new List<Example>() {
                            new Example() {
                                Description = "No username, password and ignorelist used.(NOT a real proxy address)",
                                Content = @"--proxy-settings=127.0.0.1|12345"
                            },
                            new Example() {
                                Description = "No ignorelist used.(NOT a real proxy address)",
                                Content = @"--proxy-settings=127.0.0.1|12345|myUsername|secretPassword"
                            },
                            new Example() {
                                Description = "Proxy is not used for google.com and youtube.com.(NOT a real proxy address)",
                                Content = @"--proxy-settings=127.0.0.1|12345|||google.com;youtube.com"
                            },
                            new Example() {
                                Description = "Proxy using the socks5 protocol.(NOT a real proxy address)",
                                Content = @"--proxy-settings=socks5://localhost|8080"
                            }
                        }
                    }},
                    { "cookies", new ConfigParameter() {
                        Name = "cookies",
                        Information = @"Allows to set cookies for specified URLs. This can either be a list of cookies or a file containing the cookie list.
                            The netscape cookie format is used; a description can be found here: http://www.cookiecentral.com/faq/#3.5 .
                            You can get the cookie list or file via chromium addon cookies.txt (https://chrome.google.com/webstore/detail/cookiestxt/njabckikapfpffapmjgojcnbfjonfjfg).",
                        Examples = new List<Example>(){
                            new Example(){
                                Content = @"--cookies<https://www.my.domain>=
                                    #This is a comment
                                    www.my.domain	FALSE	/	FALSE	0	key	value",
                                Description = "Adds a cookie for the webpage www.my.domain"
                            }
                        },  
                        ParameterType = ConfigParameterType.Text,
                        CategoryName = "basic",
                        Stackable = true
                    }},
                    { "allow-internal_resource", new ConfigParameter() {
                        Name = "allow-internal_resource",
                        Information = @"Prevents access to the internal resource bundled within the scchrom.exe.
                            Such a resource can be accessed via the address scchrom://internal/[resourceName] where [resourceName] is the name as given in visual studio.",
                        ParameterType = ConfigParameterType.Boolean,
                        CategoryName = "basic",
                        DefaultValue = "true"
                    }},
                    { "allow-file_resource", new ConfigParameter() {
                        Name = "allow-file_resource",
                        Information = @"Allows access to files stored relative to the scchrom.exe.
                            They can be accessed via scchrom://file/[pathToFile] where [pathToFile] is the path to the file relative to the folder of the scchrom.exe. 
                            The mimeType will be guessed by the filenames extension.",
                        ParameterType = ConfigParameterType.Boolean,
                        CategoryName = "basic",
                        DefaultValue = "false"
                    }},
                    { "allow-external-links", new ConfigParameter() {
                        Name = "allow-external-links",
                        Information = @"If true links that start with <i>external:</i>, like <i>external:https://google.com</i>, will open the link after <i>external:</i> in the system browser.",
                        ParameterType = ConfigParameterType.Boolean,
                        CategoryName = "basic",
                        DefaultValue = "false"
                    }},
                    { "reject-downloads", new ConfigParameter() {
                        Name = "reject-downloads",
                        Information = @"If set to true, all downloads will be rejected and neither the on-before-download nor the on-progress-download handler will be called.",
                        ParameterType = ConfigParameterType.Boolean,
                        CategoryName = "basic",
                        DefaultValue = "false"
                    }},
                    { "remote-debugging-port", new ConfigParameter() {
                        Name = "remote-debugging-port",
                        Information = @"If set, the port will be available to be used for remote debugging from other chromium based browsers. <br>
                            Default port used by other chromium derivates is 9222.",
                        ParameterType = ConfigParameterType.Number,
                        CategoryName = "basic"
                    }},
                    { "background-color", new ConfigParameter() {
                        Name = "background-color",
                        Information = @"Sets the background color of the browser (only visible if the page doesn't set a background color). The color must have the color hex format like: 0xFFFFFFFF for white.",
                        ParameterType = ConfigParameterType.Text,                        
                        CategoryName = "basic"
                    }},
// program window
                    { "window-width", new ConfigParameter() {
                        Name = "window-width",
                        Information = @"Sets the initial width as integer of the window.",
                        CategoryName = "window",
                        DefaultValue = "800",
                        ParameterType = ConfigParameterType.Number
                    }},
                    { "window-height", new ConfigParameter() {
                        Name = "window-height",
                        Information = @"Set the initial height as integer of the window.",
                        CategoryName = "window",
                        DefaultValue = "500",
                        ParameterType = ConfigParameterType.Number
                    }},
                    { "window-pos-x", new ConfigParameter() {
                        Name = "window-pos-x",
                        Information = @"Sets the horizontal window position as integer.<br>
                                Positive values are the left offset, negative values are the right offset.",
                        CategoryName = "window",
                        DefaultValue = "400",
                        ParameterType = ConfigParameterType.Number
                    }},
                    { "window-pos-y", new ConfigParameter() {
                        Name = "window-pos-y",
                        Information = @"Sets the vertical window position as integer.<br>
                                Positive values are the top offset, negative values are the bottom offset.",
                        CategoryName = "window",
                        DefaultValue = "400",
                        ParameterType = ConfigParameterType.Number
                    }},
                    { "window-state", new ConfigParameter() {
                        Name = "window-state",
                        Information = @"Sets the initial window-state.<br>
                            <b>BEWARE</b>: When starting ScChrom minimized the page will be rendered at a height and width of 0.<br>
                            This prevents the WindowControllers click method from working correctly.",
                        CategoryName = "window",
                        DefaultValue = "normal",
                        EnumerationValues = new List<Enumerationvalues>() {
                            new Enumerationvalues { Value = "normal",    Description = "not maximized nor minimized, the default behaviour for new windows"},
                            new Enumerationvalues { Value = "minimized", Description = "starts the window minimized"},
                            new Enumerationvalues { Value = "maximized", Description = "maximized window"}
                        },
                        ParameterType = ConfigParameterType.Enumeration
                    }},
                    { "window-show_in_taskbar", new ConfigParameter() {
                        Name = "window-show_in_taskbar",
                        Information = @"Set to true if the ScChrom-icon should be visible in the taskbar.",
                        CategoryName = "window",
                        DefaultValue = "true",
                        ParameterType = ConfigParameterType.Boolean
                    }},
                    { "window-show_notifyicon", new ConfigParameter() {
                        Name = "window-show_notifyicon",
                        Information = @"Set to true to show the ScChrom-icon at the right side of the windows taskbar.",
                        CategoryName = "window",
                        DefaultValue = "false",
                        ParameterType = ConfigParameterType.Boolean
                    }},
                    { "window-topmost", new ConfigParameter() {
                        Name = "window-topmost",
                        Information = @"Set to true to keep the window always in front of all other windows.",
                        CategoryName = "window",
                        DefaultValue = "false",
                        ParameterType = ConfigParameterType.Boolean
                    }},
                    { "window-prevent_stealing_focus", new ConfigParameter() {
                        Name = "window-prevent_stealing_focus",
                        Information = @"Set to true if the window should not steal the current focus upon opening. <br>
                            Normally a new window always gets the focus when it opens, which can be annoying if ScChrom is opened by another program.",
                        CategoryName = "window",
                        DefaultValue = "false",
                        ParameterType = ConfigParameterType.Boolean
                    }},
                    { "window-hide_url_field", new ConfigParameter() {
                        Name = "window-hide_url_field",
                        Information = @"Set true to hide the top bar where an url can be entered.",
                        CategoryName = "window",
                        DefaultValue = "false",
                        ParameterType = ConfigParameterType.Boolean
                    }},
                    { "window-border", new ConfigParameter() {
                        Name = "window-border",
                        Information = @"Set to adjust the border of the main window.",
                        CategoryName = "window",
                        DefaultValue = "sizable",
                        ParameterType = ConfigParameterType.Enumeration,
                        EnumerationValues = new List<Enumerationvalues>() {
                            new Enumerationvalues() {
                                Value = "sizable",
                                Description = "The main window can be resized an has a visible border."
                            },
                            new Enumerationvalues() {
                                Value = "none",
                                Description = "The main window has no border, fixed size and no close, minimize and maximize boxes on the top right."
                            },
                            new Enumerationvalues() {
                                Value = "fixed",
                                Description = "The main window has a border but has a fixed size."
                            }
                        }                       
                    }},
// request manipulation
                    { "request-whitelist", new ConfigParameter() {
                        Name = "request-whitelist",
                        Information = @"Set a whitelist of allowed addresses for requests. Every entry must be separated via vertical bar | . <br>
                            The wildcards ? (single random character) and * (one or more random character) are allowed. The complete list of addresses must be a single line.<br>
                            If set, all requests to other addresses will result in empty responses.",
                        CategoryName = "request manipulation",
                        Examples = new List<Example>() {
                            new Example() {
                                Content = @"--request-whitelist=""*google.com/*|*bing.*""",
                                Description = "Allows all request to google.com and bing.com adresses"
                            }
                        },
                        ParameterType = ConfigParameterType.Text
                    }},
                    { "append-to-mainpage", new ConfigParameter() {
                        Name = "append-to-mainpage",
                        Information = @"Adds the given text to the end of the first loaded page.
                            The examples show how to add custom JavaScript or styles to the page.<br>
                            For more granularity use the <b>on-response</b> event handler.<br>
                            <b>BEWARE</b>: <br>
                                1. When the added JavaScript gets executed the ScChrom object must not be ready yet.<br>
                                2. This will not be executed for local files or if the <b>html</b> parameter is used.<br>",
                        CategoryName = "request manipulation",                        
                        Examples = new List<Example>() {
                            new Example() {
                                Content = @"--append-to-mainpage=
                                            <style>
                                                body{
                                                   background-color:rgb(1, 1, 1);
                                                }
                                            </style>",
                                Description = "Add css"
                            },
                            new Example() {
                                Content = @"--append-to-mainpage=
                                            <script>
                                                alert('hello world');
                                            </script>",
                                Description = "Add javascript"
                            }                            
                        },
                        ParameterType = ConfigParameterType.Text
                    }},
                    { "exchange-response-utf8", new ConfigParameter() {
                        Name = "exchange-response-utf8",
                        Information = @"Set to exchange the actual response with a utf8-formated response for any request to the specified address. <br>
                            The method of the request is ignored. <br>
                            Can only be used with a config file, NOT as command line argument. <br>
                            The url has to be provided as well to call this script only for certain requests. <br>
                            Either a complete URL or an URL pattern (with ? and * wildcards) are supported (to catch all requests <br>
                            simple use * as URL). The URL and the script must be separated by either a space or a new <br>
                            line (in the config file).<br>
                            No request will be send to the target.",
                        CategoryName = "request manipulation",
                        Examples = new List<Example>() {
                            new Example() {
                                Content = @"--exchange-response-utf8<https://api.example.com/*>=
                                   { 'data' : 'json data retuned on request to https://api.example.com/'}",
                                Description = "Will return the complete text with brackets"
                            }
                        },
                        ParameterType = ConfigParameterType.Javascript,
                        Stackable = true
                    }},
                    { "exchange-response-utf8_script", new ConfigParameter() {
                        Name = "exchange-response-utf8_script",
                        Information = @"Set to exchange the actual response with the utf8-formated result of a JavaScript script. <br>                            
                            The script gets the url and method as input and is executed in the Jint context. <br>
                            Can only be used with a config file, NOT as command line argument. <br>
                            To call this script only for certain requests the URL has to be provided as stack key. <br>
                            Either a complete URL or an URL pattern (with ? and * wildcards) are supported (to catch all requests simple use * as URL). <br>
                            The URL and the script must be separated by either a space or a new line (in the config file). <br>                            
                            The variables 'url' and 'method' are predefined, the result must be returned.<br>
                            No request will be send to the target.",
                        CategoryName = "request manipulation",
                        Examples = new List<Example>() {
                            new Example() {
                                Content = @"--exchange-response-utf8_script=https://api.example.com/*
                                    if(method == 'get' && url.endsWith('1')) {
                                        return 'the URL of the request ends with 1';
                                    }
                                    return 'no 1 at the end';",
                                Description = "Will return a text depending on the URL"
                            }
                        },
                        ParameterType = ConfigParameterType.Javascript,
                        Stackable = true
                    }},
// scripting
                    { "initial-script", new ConfigParameter() {
                        Name = "initial-script",
                        Information = @"Set a custom script that gets executed before anything else in the Jint context (NOT the browsers JavaScript context). <br>
                                   It can be used to define functions used by other scripts in the Jint context.<br>
                                   The script will be executed only once, no matter how often the page changes.",
                        CategoryName = "scripting",
                        ParameterType = ConfigParameterType.Javascript                        
                    }},
// browser js settings
                    { "disable-localstorage", new ConfigParameter() {
                        Name = "disable-localstorage",
                        Information = @"If true the localstorage is disabled. Same as the cef command line argument disable-local-storage.",
                        CategoryName = "browser js settings",
                        DefaultValue = "false",
                        ParameterType = ConfigParameterType.Boolean
                    }},
                    { "browser-js-mainController_name", new ConfigParameter() {
                        Name = "browser-js-mainController_name",
                        Information = @"Set to change the name of the main object used in the browsers JavaScript context.",
                        CategoryName = "browser js settings",
                        DefaultValue = "ScChrom",
                        ParameterType = ConfigParameterType.Text
                    }},
                    { "browser-js-allow_objects", new ConfigParameter() {
                        Name = "browser-js-allow_objects",
                        Information = @"Set to allow or prevent calling program functions from the browsers javascript context. <br>
                            This can be either true to allow all controllers, false to allow none or a comma separated list of allowed controllers. <br>
                            All allowed controllers and their functions can be accessed in the browsers JavaScript context <br>
                            via the global mainController object (see browser-js-mainController_name parameter). <br>
                            The default is false due to security concerns. Malicious JavaScripts on pages can get control over ScChroms internals. <br>",
                        CategoryName = "browser js settings",
                        DefaultValue = "false",
                        ParameterType = ConfigParameterType.Text
                    }},
                    { "prevent-pagechange", new ConfigParameter() {
                        Name = "prevent-pagechange",
                        Information = @"If set to true it prevents ScChrom from opening another page.
                            This can help to prevent unwanted redirects via JavaScript.
                            If set, the arguments for <b>allowed-pagechange_urls</b> will be ignored.<br>
                            <b>BEWARE</b>: Setting this to true will prevent the dev_tools from loading!",
                        CategoryName = "browser js settings",
                        DefaultValue = "false",
                        ParameterType = ConfigParameterType.Boolean,
                        Examples = new List<Example>() {
                            new Example() {
                                Content = @"--prevent-pagechange=true 
                                    --url=https://www.twitter.com",
                                Description = "After the page loaded completely, google can't be left anymore."
                            }
                        },
                    }},
                    { "allowed-pagechange_urls", new ConfigParameter() {
                        Name = "allowed-pagechange_urls",
                        Information = @"Set to allow page changes to certain URLs. If prevent-pagechange is true, this is ignored. <br>
                            The urls are given as a list of patterns separated by a vertical bar | . All URLs are lower cases.<br>
                            <b>BEWARE</b>: This will prevent the dev_tools from loading unless you allow devtools://devtools/devtools_app.html .",
                        CategoryName = "browser js settings",
                        DefaultValue = "false",
                        Examples = new List<Example>() {
                            new Example() {
                                Content = @"--allowed-pagechange_urls=https://goolge.com/*|https://wikipedia.com/*",
                                Description = "Only allows page changes to goolge and wikipedia pages"
                            }
                        },
                        ParameterType = ConfigParameterType.Boolean
                    }},
                    { "prevent-JS-dialog", new ConfigParameter() {
                        Name = "prevent-JS-dialog",
                        Information = @"Set to true to prevent opening a JavaScript dialog on a page.
                            These dialogs can be opened via alert('content') for example.",
                        CategoryName = "browser js settings",
                        DefaultValue = "false",
                        ParameterType = ConfigParameterType.Boolean
                    }},
                    { "prevent-popups", new ConfigParameter() {
                        Name = "prevent-popups",
                        Information = @"Set if the spawning of new popup windows should be prevented.",
                        CategoryName = "browser js settings",
                        DefaultValue = "true",
                        ParameterType = ConfigParameterType.Boolean
                    }},
                    { "ignore-crossorigin-from-files", new ConfigParameter() {                        
                        Name = "ignore-crossorigin-from-files",
                        Information = @"Set to true to allow the JavaScript in the browser context from a local loaded file to fetch from any source,
                            ignoring the same-origin-policy.",
                        CategoryName = "browser js settings",
                        DefaultValue = "false",
                        ParameterType = ConfigParameterType.Boolean
                    }},                    
                    { "webrtc-media-enabled", new ConfigParameter() {
                        Name = "webrtc-media-enabled",
                        Information = @"Set true to allow webrtc to use video inputs and audio inputs like camera and microphone.
                            This can only be set once at startup and will stay even when reloading the config file.",
                        CategoryName = "browser js settings",
                        DefaultValue = "false",
                        ParameterType = ConfigParameterType.Boolean
                    }},
// browser js scripts
                    { "injected-javascript", new ConfigParameter() {
                        Name = "injected-javascript",
                        Information = @"Set to inject custom JavaScript into the browser context after the page and all assets are 
                            fully loaded. After a page change, this script will be injected again. This can have multiple lines if stored in a config file.<br>
                            <b>BEWARE:</b> This will not work when using the <b>html</b> parameter.",
                        CategoryName = "browser js scripts",
                        ParameterType = ConfigParameterType.Javascript
                    }},
                    { "disable-browser-js", new ConfigParameter() {
                        Name = "disable-browser-js",
                        Information = @"Set to true to deactivate all JavaScript in the browser context.<br>
                            JavaScript will still be executed in the Jint context.",
                        CategoryName = "browser js scripts",
                        DefaultValue = "false",
                        ParameterType = ConfigParameterType.Boolean
                    }},
// event handler
                    { "on-console-message", new ConfigParameter() {
                        Name = "on-console-message",
                        Information = @"Set to add a custom script that will be executed when the browsers JavaScript console prints something. <br>
                            Following variables are provided to the script:
                            <ul>
                                <li><b>content</b>: The content of the message.</li>
                                <li><b>level</b>: The log severity of message (like info, error, verbose, default, warning).</li>
                            </ul>",
                        CategoryName = "event handler",
                        ParameterType = ConfigParameterType.Javascript,
                        Stackable = true,
                        Examples = new List<Example>() {
                            new Example() {
                                Content = @"--on-console-message=
                                    if(content == 'exit')
                                        WindowController.closeMainwindow();",
                                Description = "Closes ScChrom when the browser console writes 'exit' for example via console.log('exit') ."
                            }
                        }
                    }},                    
                    { "on-before-key", new ConfigParameter() {
                        Name = "on-before-key",
                        Information = @"Set to add a custom script that will be executed before a key event (like key press or release) is forwarded to the browser.<br>
                            In order to prevent the key from being forwarded to the browser return true.<br>
                            The script gets a json encoded string called keyEvent with following fields:
                            <pre style=""margin: 0;"">" + Tools.Common.TrimStartLines(@"
                            {
                                // The type of key event (most important: 1 == down, 3 == up, 4 == char)
                                'type' : 0,
                                // The windows key code, see WebCore/platform/chromium/KeyboardCodes.h for the list of values.
                                'winKeyCode' : 0,
                                // The native code provided by the operating system (always 0 if invoked via InputController)
                                'natKeyCode' : 0,
                                // can be one or more (via logical or) of the available <a style=""color:white;"" href=""external:https://cefsharp.github.io/api/71.0.0/html/T_CefSharp_CefEventFlags.htm"">flags</a>
                                'modifiers' : 0,
                                // 1 if the key is a system key, 0 otherwise 
                                'isSysKey' : 0
                            }", 28) + @"
                            </pre>",                        
                        CategoryName = "event handler",
                        ParameterType = ConfigParameterType.Javascript,
                        Stackable = false,
                        Examples = new List<Example>() {
                            new Example() {
                                Content = @"--on-before-key=
                                    let ki = JSON.parse(keyEvent);
                                    if(ki.winKeyCode < 65 || ki.winKeyCode > 90)
                                        return true;
                                    write('keycode: ' + ki.winKeyCode);",
                                Description = "Every key event but the one between A-Z are ignored and the win key code is written to the standard output."
                            },
                        },
                    }},
                    { "on-after-key", new ConfigParameter() {
                        Name = "on-after-key",
                        Information = @"Set to add a custom script that will be executed after a key event (like key press or release) was handled to the browser.<br>                            
                            The script gets a json encoded string called keyEvent with following fields:
                            <pre style=""margin: 0;"">" + Tools.Common.TrimStartLines(@"
                            {
                                // The type of key event (most important: 1 == down, 3 == up, 4 == char)
                                'type' : 0,
                                // The windows key code, see WebCore/platform/chromium/KeyboardCodes.h for the list of values.
                                'winKeyCode' : 0,
                                // The native code provided by the operating system (always 0 if invoked via InputController)
                                'natKeyCode' : 0,
                                // can be one or more (via logical or) of the available <a style=""color:white;"" href=""external:https://cefsharp.github.io/api/71.0.0/html/T_CefSharp_CefEventFlags.htm"">flags</a>
                                'modifiers' : 0,
                                // 1 if the key is a system key, 0 otherwise 
                                'isSysKey' : 0
                            }", 28) + @"
                            </pre>",
                        CategoryName = "event handler",
                        ParameterType = ConfigParameterType.Javascript,
                        Stackable = false,
                        Examples = new List<Example>() {
                            new Example() {
                                Content = @"--on-after-key=
                                    let ki = JSON.parse(keyEvent);
                                    if(ki.type == 3)
                                        write('keycode: ' + ki.winKeyCode);",
                                Description = "Write out every key release event that was previously sent to the browser."
                            },
                        },
                    }},
                    { "on-before-download", new ConfigParameter() {
                        Name = "on-before-download",
                        Information = @"Set to add a custom script that will be executed before a download starts.<br>
                            If true is returned, the download will be rejected.<br>
                            If the stacked key is omitted, this will be called for every download, otherwise this is only called if the URL of the website that initiates the download matched the stack key.<br>
                            Following variables are provided to the script:
                            <ul>
                                <li><b>suggested_filename</b>: The filename as suggested by the website.</li>
                                <li><b>page_url</b>: The URL of the website on which the download was started on.</li>
                                <li><b>url</b>: Actual URL of the file to download without redirects.</li>
                                <li><b>fullpath</b>: The absolute path of the files destination; change this value to set the path of the downloaded file. Start with %desktop% to get the path to desktop. <b>BEWARE:</b> If the path is invalid the download will be rejected silently.</li>
                                <li><b>total_bytes</b>: The files size in bytes.</li>
                                <li><b>mime_type</b>: The provided mime type.</li>
                                <li><b>show_dialog</b>: Setting it to false prevents the download dialog from showing.</li>
                            </ul>",
                        CategoryName = "event handler",
                        ParameterType = ConfigParameterType.Javascript,
                        Stackable = true,
                        Examples = new List<Example>() {                            
                            new Example() {
                                Content = @"--on-before-download=
                                    if(page_url.includes('google'))
                                        return true;
                                    fullpath = '%desktop%/test.file'                                        
                                    show_dialog = total_bytes > 1024;",
                                Description = "Rejects all downloads from URLs including 'google', sets the new file to be 'test.file' on the desktop and shows only the save file dialog for files larger than 1024 bytes."
                            }
                        }
                    }},
                    { "on-progress-download", new ConfigParameter() {
                        Name = "on-progress-download",
                        Information = @"Set to add a custom script that will be executed when the progress of a download changes.<br>
                            This handler will be called in short, random times, so the progress won't contain all percentages and even after canceling it might be called few more times.                            
                            Return true to cancel the download.<br>
                            Following variables are provided to the script:
                            <ul>
                                <li><b>page_url</b>: URL of the website on which the download was started on.</li>
                                <li><b>url</b>: Actual URL of the file to download without redirects.</li>
                                <li><b>fullpath</b>: Absolute path of the files destination (read only).</li>
                                <li><b>suggested_filename</b>: Filename as suggested by the website (or as set in the on-before-download handler).</li>
                                <li><b>percentage</b>: Percentage of the download (0 - 100).</li>
                                <li><b>received_bytes</b>: All received bytes so far.</li>
                                <li><b>total_bytes</b>: The files size in bytes.</li>
                                <li><b>current_speed</b>: The current download speed in bytes per second.</li>
                                <li><b>is_complete</b>: True if the download is finished.</li>
                            </ul>",
                        CategoryName = "event handler",
                        ParameterType = ConfigParameterType.Javascript,
                        Stackable = true,
                        Examples = new List<Example>() {
                            new Example() {
                                Content = @"--on-progress-download=
                                    write(percentage);",
                                Description = "Writes out the finished percentage."
                            },
                            new Example() {
                                Content = @"--on-progress-download=
                                    if(percentage > 10)
                                        return true;
                                    write('download speed (kb/s): ' + (current_speed / 1024));",
                                Description = "Writes the download speeds in the console; cancels the download after 10% are done."
                            },
                        },
                    }},
                    { "on-before-browse", new ConfigParameter() {
                        Name = "on-before-browse",
                        Information = @"Set to add a custom script that will be executed before the page changes (including the initial opening).<br>                            
                            In order to prevent browsing to the target URL return true.<br>
                            Following variables are provided to the script:
                            <ul>
                                <li><b>targetUrl</b>: The target URL.</li>
                                <li><b>currentUrl</b>: The current URL.</li>
                                <li><b>isRedirect</b>: true if it is a redirect (403).</li>
                            </ul>",
                        CategoryName = "event handler",
                        ParameterType = ConfigParameterType.Javascript,
                        Stackable = false,
                        Examples = new List<Example>() {
                            new Example() {
                                Content = @"--on-before-browse=
                                    write(targetUrl);",
                                Description = "Writes all URLs ScChrom browses to to the standard output."
                            },
                            new Example() {
                                Content = @"--on-before-browse=
                                    if(!currentUrl.includes('google.com'))
                                      return;
                                    if(targetUrl.includes('google.com'))
                                      return true;",
                                Description = "If on a google page only page changes to none google pages are possible."
                            }
                        },
                    }},
                    { "on-before-request", new ConfigParameter() {
                        Name = "on-before-request",
                        Information = @"Set to add a custom script that will be executed before a request is sent to the target URL.<br>                            
                            If a stack key is provided, the handler will only be called if the stacked key matches the requests URL. Otherwise the handler will be called for every request. <br>
                            If returning false the request gets canceled; if returning a string the request gets redirected.<br>
                            Following variables are provided to the script:
                            <ul>
                                <li><b>url</b>: The URL of the request.</li>
                                <li><b>current_url</b>: The URL of the current page.</li>
                            </ul>",
                        CategoryName = "event handler",
                        Examples = new List<Example>() {
                            new Example() {
                                Content = @"--on-before-request<https://www.youtube.com/>=
                                    return 'https://google.com';",
                                Description = "Redirects requests to https://www.youtube.com/ to https://google.com"
                            },
                            new Example() {
                                Content = @"--on-before-request<https://google.com*>=
                                    write(url);
                                    return false;",
                                Description = "Writes out the URL of the request to google and cancels it."
                            },
                            new Example() {
                                Content = @"--on-before-request=
                                    if(url.includes('google'))
                                        return false;",
                                Description = "Applied to all requests and rejects the request if the target URL includes 'google'."
                            }
                        },
                        ParameterType = ConfigParameterType.Javascript,
                        Stackable = true
                    }},
                    { "on-request-response-utf8", new ConfigParameter() {
                        Name = "on-request-response-utf8",
                        Information = @"Set to add a custom script that will be executed if a certain request gets a response.<br>   
                             If a stack key is provided, the handler will only be called if the stacked key matches the requests url. Otherwise the handler will be called for every request. <br>
                            Following variables are provided to the script:
                            <ul>
                                <li><b>url</b>: The url of the request.</li>
                                <li><b>response</b>: The read only, utf-8 decoded response of the request.</li>                                
                            </ul>",
                        CategoryName = "event handler",
                        Examples = new List<Example>() {
                            new Example() {
                                Content = @"--on-request-response-utf8<https://www.google.com/>=
                                    write(response);",
                                Description = "Writes to the complete html of the response to the standard oputput."
                            }
                        },
                        ParameterType = ConfigParameterType.Javascript,
                        Stackable = true
                    }},
                    { "on-response", new ConfigParameter() {                       
                        Name = "on-response",
                        Information = @"Set to add a custom script that will be executed if a response arrives and allows to manipulate it.<br>
                            The return value of the script is the new content of the response; if the return value is missing, the response will be empty.<br>
                            The event is only fired for successful requests (with Html code 200) and will ignore redirects. <br>
                            If a stack key is provided, the handler will only be called if the stacked key matches the requests URL. Otherwise the handler will be called for every request. <br>
                            If the stacked key is missing or empty the listener will be called only once for the first response.<br>
                            <b>BEWARE</b>: <br>
                                1. When the added JavaScript gets executed the ScChrom object must not be ready yet.<br>
                                2. This will not be executed for local files or if the <b>html</b> parameter is used.<br>
                            Following variables are provided to the script:
                            <ul>
                                <li><b>url</b>: The URL of the request.</li>
                                <li><b>response</b>: The UTF-8 decoded response of the request. Manipulating it does change the response accordingly.</li>
                            </ul>",
                        CategoryName = "event handler",
                        Examples = new List<Example>() {
                            new Example() {
                                Content = @"--on-response<https://google.com>=
                                    write(response);
                                    return 'ABC';",
                                Description = "Writes to the complete Html of the response to the standard output and exchanges the response with 'ABC'"
                            },
                            new Example() {
                                Content = @"--on-response=
                                    return response + '<style>body{background-color:black;}</style>';",
                                Description = "Adds some css to the main page."
                            },
                            new Example() {
                                Content = @"--on-response<*.css>=                                    
                                    return '/* ' + response + ' */';",
                                Description = "Comments out the content of every external css file."
                            },
                            new Example() {
                                Content = @"--on-response<*.js>=
                                    write(url);",
                                Description = "Writes out all .js files and removes the content of them."
                            }
                        },
                        ParameterType = ConfigParameterType.Javascript,
                        Stackable = true
                    }},                    
                    { "on-file-change", new ConfigParameter() {
                        Name = "on-file-change",
                        Information = @"Set to add a listener that will be executed when the file or one of the files of the directory (including subdirectories) changes.<br>                                                      
                            The path of the file or directory has to be provided as the stack key.<br>
                            Following variables are provided to the script:
                            <ul>
                                <li><b>file</b>: The absolute path of the changed file or folder.</li>
                                <li><b>type</b>: The type of change (one of: created, deleted, changed, renamed).</li>
                            </ul>", 
                        CategoryName = "event handler",
                        Examples = new List<Example>() {
                            new Example() {
                                Content = @"--on-file-change<c:/test/folder>=
                                    write(file);",
                                Description = "Writes the name of the changed file to the standard output."
                            },
                            new Example() {
                                Content = @"--on-file-change<c:/test/folder/testfile.txt>=
                                        write(file);
                                    --on-file-change<c:/test/directory/>=
                                        if(type == 'deleted')
                                            write(file);",
                                Description = "Demonstrates how to use multiple hadnlers. Second handler only writes the filepath if ot was removed."
                            },
                        },
                        ParameterType = ConfigParameterType.Javascript,
                        Stackable = true
                    }},
                    { "on-before-contextmenu", new ConfigParameter() {
                        Name = "on-before-contextmenu",
                        Information = @"Set to add a listener that will be executed before a context menu (trigger by right click) will be shown.
                            Use it together with the <b>on-click-contextmenu</b> handler to create a custom context menu.<br>
                            If a stack key is provided, the handler will only be called if the stacked key matches the current page URL. Otherwise the handler will be called on every page. <br>                            
                            Following variables are provided to the script:
                            <ul>
                                <li><b>url</b>: Url of the current page.</li>
                                <li><b>mediasource_url</b>: If clicked on media (audio, video, image) this holds the medias source URL.</li>
                                <li><b>mediatype</b>: If clicked on a media, this holds the type (none if no media clicked, otherwise one of image, video, audio, file or plugin).</li>
                                <li><b>link_url</b>: If clicked on a link, this holds the links target URL.</li>
                                <li><b>selected_text</b>: If any text is selected this will be it.</li>
                                <li><b>position_x</b>: Horizontal value of the click position (zero based, goes up from left to right).</li>
                                <li><b>position_y</b>: Vertical value of the click position (zero based, goes up from top to bottom).</li>
                                <li><b>selected_types</b>: JSON stringified array of the elementtypes that have been clicked (values are: none, page, frame, link, media, selection, editable).</li>
                            </ul>
                            The script must return a json serialized (via JSON.stringify) string of following structured json object:
                            <pre style=""margin: 0;"">" + Tools.Common.TrimStartLines(@"
                            {
                                // if true old entries are removed
                                'clear' : true,
                                // entries to add
                                'entries' : [
                                    // simple label
                                    {
                                        'text' : 'first entry',
                                        'id' : 1
                                        // default ('type' is a label)
                                    },
                                    // a separator
                                    {
                                        'type' : 'separator'
                                    },
                                    // a checkbox
                                    {
                                        'text' : 'im a checkbox',
                                        'id' : 2,
                                        'type' : 'checkbox',
                                        'checked' : true
                                    },
                                ]
                            }", 28) + @"
                            </pre>
                            ", 
                        CategoryName = "event handler",
                        Examples = new List<Example>() {
                            new Example() {
                                Content = @"--on-before-contextmenu=
                                    write('click location: ' + position_x + ':' + position_y);
                                    return JSON.stringify({
                                        'clear' : false,
                                        'entries' : [
                                            {
                                                'text' : 'exit',
                                                'id' : 1
                                            }
                                        ]
                                    })",
                                Description = "Adds an 'exit' menu to the default contextmenu and writes out the click location; use with the <b>on-click-contextmenu</b> example for full usage."
                            }
                        },
                        ParameterType = ConfigParameterType.Javascript,
                        Stackable = true
                    }},
                    { "on-click-contextmenu", new ConfigParameter() {
                        Name = "on-click-contextmenu",
                        Information = @"Set to add a listener that will be executed when clicking a custom entry in the context menu.<br>
                            Use the <b>on-before-contextmenu</b> handler to create custom entries.<br>
                            If a stack key is provided, the handler will only be called if the stacked key matches the current page URL. Otherwise the handler will be called on every page. <br>
                            Following variables are provided to the script:
                            <ul>
                                <li><b>url</b>: URL of the current page.</li>
                                <li><b>mediasource_url</b>: If clicked on media (audio, video, image) this holds the medias source url.</li>
                                <li><b>mediatype</b>: If clicked on a media, this holds the type (none if no media clicked, otherwise one of image, video, audio, file or plugin).</li>
                                <li><b>link_url</b>: If clicked on a link, this holds the links target url.</li>
                                <li><b>selected_text</b>: If any text is selected this will be it.</li>
                                <li><b>position_x</b>: Horizontal value of the click position (zero based, goes up from left to right).</li>
                                <li><b>position_y</b>: Vertical value of the click position (zero based, goes up from top to bottom).</li>
                                <li><b>selected_types</b>: JSON stringified array of the elementtypes that have been clicked (values are: none, page, frame, link, media, selection, editable).</li>
                                <li><b>clicked_id</b>: The entry id as set on creation in the <b>on-click-contextmenu</b> handler.</li>
                            </ul>",
                        CategoryName = "event handler",
                        Examples = new List<Example>() {
                            new Example() {
                                Content = @"--on-click-contextmenu=
                                    if(clicked_id == 1){
                                        WindowController.closeMainwindow();
                                    }",
                                Description = "If the item with id==1 get clicked, ScChrom closes. Use it together with the example from the <b>on-before-contextmenu</b> handler."
                            }
                        },
                        ParameterType = ConfigParameterType.Javascript,
                        Stackable = true
                    }},
                    { "on-before-notifyicon-contextmenu", new ConfigParameter() {
                        Name = "on-before-notifyicon-contextmenu",
                        Information = @"Set to add a listener that will be executed before the context menu of the notifyicon will be shown (trigger by right clicking it).<br>
                            <b>NOTE</b>: This is only useful if the parameter <b>window-show_notifyicon</b> is set to <i>true</i>.<br>
                            Use it together with the <b>on-click-notifyicon-contextmenu</b> handler to create a custom context menu for the notifyicon.<br>                          
                            Following variables are provided to the script:
                            <ul>
                                <li><b>url</b>: Url of the current page.</li>                                
                            </ul>
                            The script must return a json serialized (via JSON.stringify) string of following structured json object:
                            <pre style=""margin: 0;"">" + Tools.Common.TrimStartLines(@"
                            {
                                // if true old entries are removed
                                'clear' : true,
                                // entries to add
                                'entries' : [
                                    // simple label
                                    {
                                        'text' : 'first entry',
                                        'id' : 'some text'
                                        // default ('type' is a label)
                                    },
                                    // a separator
                                    {
                                        'type' : 'separator'
                                    },
                                    // a checkbox
                                    {
                                        'text' : 'im a checkbox',
                                        'id' : 'a checkbox',
                                        'type' : 'checkbox',
                                        'checked' : true
                                    },
                                ]
                            }", 28) + @"
                            </pre>
                            ",
                        CategoryName = "event handler",
                        Examples = new List<Example>() {
                            new Example() {
                                Content = @"--on-before-notifyicon-contextmenu=
                                    write('creating notifyicons contextmenu');
                                    return JSON.stringify({
                                        'clear' : false,
                                        'entries' : [
                                            {                                            
                                                'type' : 'separator'
                                            },
                                            {
                                                'text' : 'minimize',
                                                'id' : 'minimize'
                                            }
                                        ]
                                    })",
                                Description = "Adds an 'minimize' menu to the default contextmenu and writes out 'creating notifyicons contextmenu'; use with the <b>on-click-contextmenu</b> example for full usage."
                            }
                        },
                        ParameterType = ConfigParameterType.Javascript
                    }},                    
                    { "on-click-notifyicon-contextmenu", new ConfigParameter() {
                        Name = "on-click-notifyicon-contextmenu",
                        Information = @"Set to add a listener that will be executed when clicking a custom entry in the notfiy icons context menu.<br>
                            Use the <b>on-before-notifyicon-contextmenu</b> handler to create custom entries.<br>
                            Following variables are provided to the script:
                            <ul>
                                <li><b>url</b>: URL of the current page.</li>                                
                                <li><b>clicked_id</b>: The entry id as set on creation in the <b>on-click-contextmenu</b> handler.</li>
                            </ul>",
                        CategoryName = "event handler",
                        Examples = new List<Example>() {
                            new Example() {
                                Content = @"--on-click-notifyicon-contextmenu=
                                    if(clicked_id == 'minimize'){
                                        WindowController.setWindowState('minimized');
                                    }",
                                Description = "If the item with id=='minimize' get clicked, ScChrom minimizes. Use it together with the example from the <b>on-before-notifyicon-contextmenu</b> handler."
                            }
                        },
                        ParameterType = ConfigParameterType.Javascript
                    }},
                    { "on-js-dialog", new ConfigParameter() {
                        Name = "on-js-dialog",
                        Information = @"Set to add a listener that will be executed before a JavaScript dialog is shown in the browser context. <br>                            
                            The results override the 'prevent-JS-dialog' setting. <br>
                            If the script returns true, the dialog will not be shown. <br>                            
                            Following variables are provided to the script:
                            <ul>
                                <li><b>dialog_type</b>: Type of dialog ('alert', 'confirm' or 'prompt').</li>
                                <li><b>dialog_messageText</b>: Text of the dialog.</li>
                                <li><b>dialog_url</b>: URL of current page.</li>
                                <li>Only for confirm dialogs : <br>
                                    <ul><li><b>dialog_success</b> - assign a boolean to it to accept (true) or cancel (false) the dialog</li></ul>
                                </li>
                                <li>Only for prompt dialogs: <br>
                                    <ul>
                                        <li><b>dialog_success</b> - assign a boolean to it to accept (true) or cancel (false) the dialog </li>
                                        <li><b>dialog_inputtext</b> - assign a string to it to provide the inserted text of the dialog </li>
                                    </ul>
                                </li>
                            </ul>",
                        CategoryName = "event handler",
                        Examples = new List<Example>() {
                            new Example() {
                                Content = @"--on-js-dialog=
                                switch(dialog_type){
                                    case 'alert':
                                        return false;
                                    case 'confirm':
                                        dialog_success = false;
                                        return true;
                                    case 'prompt':
                                        if(dialog_messageText == 'hi'){
                                            dialog_success = true;
                                            dialog_inputtext = 'hello';
                                        } else {
                                            dialog_success = false;
                                            dialog_inputtext = '';
                                        }
                                        return true;
                                }",
                                Description = "Shows only alerts, cancels all confirms and accepts prompts with 'hello' as input if the 'dialog_messageText' is 'hi'."
                            }
                        },
                        ParameterType = ConfigParameterType.Javascript                       
                    }},
                    { "on-before-close", new ConfigParameter() {
                        Name = "on-before-close",
                        Information = @"Set to add a custom script that will be executed before ScChroms mainwindow closes.",
                        CategoryName = "event handler",
                        ParameterType = ConfigParameterType.Javascript,
                        Stackable = false
                    }}
                };
                return _cachedConfigParams;
            }
        }


    }
}
