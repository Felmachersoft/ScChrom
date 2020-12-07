using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScChrom.Tools {
    public static class Common {

        /// <summary>
        /// Creates a regex from the given string with wildcards.
        /// Possible wildcards are ? for single random element and * for 0 to n random elements.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static String WildCardToRegular(String value) {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }

        /// <summary>
        /// Matches the given text with the given pattern containing wildcrads. 
        /// Possible wildcards are ? for single random element and * for 0 to n random elements.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static bool MatchText(string text, string pattern) {
            string regexPattern = WildCardToRegular(pattern);
            return Regex.IsMatch(text, regexPattern);
        }

        /// <summary>
        /// Checks if the given path is well formed.
        /// It does NOT check if the file exists!
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsValidLocalPath(string path) {
            FileInfo fi = null;
            try {
                fi = new FileInfo(path);
            } catch (ArgumentException) {
            } catch (PathTooLongException) {
            } catch (NotSupportedException) {
            }

            return fi != null;
        }

        /// <summary>
        /// Decodes the given base64 data as an utf-8 string.
        /// </summary>
        /// <param name="base64EncodedData"></param>
        /// <returns></returns>
        public static string Base64Decode(string base64EncodedData) {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        /// <summary>
        /// Returns all types that implement the given interface
        /// </summary>
        /// <param name="implementedInterface"></param>
        /// <returns></returns>
        public static Dictionary<string, Type> GetAllTypes(Type implementedInterface) {
            var ret = new Dictionary<string, Type>();
            
            var q = from t in Assembly.GetExecutingAssembly().GetTypes()
                    where t.IsClass && t.GetInterfaces().Contains(implementedInterface)
                    select t;

            foreach (var t in q) {                
                ret[t.Name] = t;
            }

            return ret;
        }
        
        /// <summary>
        /// Returns a text in which the given amount of spaces is removed at the start of each new line.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="removeSpaces"></param>
        /// <returns></returns>
        public static string TrimStartLines(string text, int removeSpaces = 0) {
            StringBuilder sb = new StringBuilder();

            var lines = text.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach(var line in lines) {
                string trimedLine = line.TrimStart();
                if(removeSpaces > 0) {
                    int leadingSpaces = line.Length - trimedLine.Length;
                    int addingSpaces = leadingSpaces - removeSpaces;
                    if (addingSpaces > 0)                        
                        trimedLine = " ".PadLeft(addingSpaces-1, ' ') + trimedLine;
                }
                sb.AppendLine(trimedLine);
            }
            
            string ret = sb.ToString();
            return ret;
        }

        /// <summary>
        /// Get all flagged values of this enum value.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static IEnumerable<Enum> GetFlags(this Enum e) {
            return Enum.GetValues(e.GetType()).Cast<Enum>().Where(e.HasFlag);
        }

        /// <summary>
        /// Get all names of enumerations that are flagged in the this value.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="onlyLowercase">Set to get only lower cased names</param>
        /// <returns></returns>
        public static IEnumerable<string> GetFlaggedNames(this Enum e, bool onlyLowercase = false) {
            var flags= e.GetFlags();
            var ret = new List<string>();

            foreach(var flag in flags) {
                string name = Enum.GetName(flag.GetType(), flag);
                if(onlyLowercase) 
                    name = name.ToLower();                
                ret.Add(name);
            }

            return ret;
        }

        /// <summary>
        /// Opens the default system browser with the given link.        
        /// </summary>
        /// <param name="url">If no valid url given, nothing will be done</param>
        public static void OpenLinkInDefaultBrowser(string url) {
            if (url != null && !url.Contains(" ") && !IsValidLocalPath(url)) {
                Uri uri = null;
                if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri)) {
                    try {
                        Process.Start(uri.ToString());
                    } catch (Win32Exception ex) {
                        Logger.Log("Error occured while opening link: " + ex.Message, Logger.LogLevel.error);
                    }
                }
            }
        }

        /// <summary>
        /// Parses a cookie file conisting of multiple lines of cookie strings as defined in http://www.cookiecentral.com/faq/#3.5.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static List<CefSharp.Cookie> ParseCookieFile(string filePath) {
            return ParseCookieLines(System.IO.File.ReadAllLines(filePath).ToList());
        }

        /// <summary>
        /// Parses a list of cookie strings. <see cref="ParseCookieLine(string)"/> for details.
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static List<CefSharp.Cookie> ParseCookieLines(List<string> lines) {
            List<CefSharp.Cookie> ret = new List<CefSharp.Cookie>();
            foreach (var line in lines) {
                var cookie = ParseCookieLine(line);
                if(cookie != null)
                    ret.Add(cookie);
            }
            return ret;
        }

        /// <summary>
        /// Parses a single cookie line. <see cref="ParseCookieLines(List{string})"/> uses this function.
        /// </summary>
        /// <param name="line">A line in the netscpae format. Check http://www.cookiecentral.com/faq/#3.5 for details.</param>
        /// <returns></returns>
        public static CefSharp.Cookie ParseCookieLine(string line) {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            if (line.StartsWith("#") || !line.Contains("\t"))
                return null;

            var lineParts = line.Split("\t".ToArray(), 7);
            if (lineParts.Length < 6)
                return null;

            string domain = lineParts[0];
            bool flag = lineParts[1].ToLower() == "true";
            string path   = lineParts[2];
            bool secure = lineParts[3].ToLower() == "true";
            string expirationString = lineParts[4];
            DateTime? expiration = null;
            if (expirationString != "0") {
                long expirationLong = 0;
                if (long.TryParse(expirationString, out expirationLong)) {
                    DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    expiration = dtDateTime.AddSeconds(expirationLong).ToLocalTime();
                }
            }
            string name = lineParts[5];

            CefSharp.Cookie ret = new CefSharp.Cookie() { 
                Domain = domain,
                Path = path,
                Secure = secure,
                Expires = expiration,
                Name = name
            };

            if (lineParts.Length > 6)
                ret.Value = lineParts[6];
           

            return ret;


            // Example lines:
            // https://unix.stackexchange.com/questions/36531/format-of-cookies-when-using-wget
            // http://www.cookiecentral.com/faq/#3.5
            /*
            # HTTP Cookie File for google.com by Genuinous @genuinous.
            # To download cookies for this tab click here, or download all cookies.
            # Usage Examples:
            #   1) wget -x --load-cookies cookies.txt "https://chrome.google.com/webstore/detail/cookiestxt/njabckikapfpffapmjgojcnbfjonfjfg/related"
            #   2) curl --cookie cookies.txt "https://chrome.google.com/webstore/detail/cookiestxt/njabckikapfpffapmjgojcnbfjonfjfg/related"
            #   3) aria2c --load-cookies cookies.txt "https://chrome.google.com/webstore/detail/cookiestxt/njabckikapfpffapmjgojcnbfjonfjfg/related"
            #
            .google.com	TRUE	/	FALSE	2145916806	CONSENT	PENDING.274ca8
            .google.com	TRUE	/	TRUE	1603869418	NID	203=nn0JFuS0JWq-Nk9u7ESYVSxgUfiwGEBKeHyX_kLpYUxoybBZQ0xgmsiV1dA_0OXxcDd9Yi6Cvs_Ni6pd1NFMjgE63p3mplbSi2Wa-vFFy5avRCtqwiklJFuFqVpxbLVtIR7TFg4b1oV7BMDwtZw2cB3yO4YLdg8pSVPHWj9lANU
            .google.com	TRUE	/	TRUE	1593805783	1P_JAR	2020-6-3-19
            .chrome.google.com	TRUE	/	FALSE	1654285790	__utma	73091649.403592129.1591213786.1591213786.1591213786.1
            .chrome.google.com	TRUE	/	FALSE	0	__utmc	73091649
            .chrome.google.com	TRUE	/	FALSE	1606981790	__utmz	73091649.1591213786.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none)
            .chrome.google.com	TRUE	/	FALSE	1591214385	__utmt	1
            .chrome.google.com	TRUE	/	FALSE	1591215590	__utmb	73091649.20.0.1591213788088
            */

        }

        /// <summary>
        /// Clears and/or adds contextmenuitems to the given ContexteMenuStrip according to the given json string.
        /// </summary>
        /// <param name="cms"></param>
        /// <param name="result"></param>
        /// <param name="parameterName">Optional parameter name to customize the possible error messages</param>
        public static void ApplyJsonToContextmenustrip(ContextMenuStrip cms, string json, string parameterName = "") {

            JObject result = null;
            try {
                result = JObject.Parse(json);
            } catch (Exception ex) {
                string errorText = "Error while parsing json result";
                if(!string.IsNullOrEmpty(parameterName)) 
                    errorText += " from parameter " + parameterName;
                errorText += ": " + ex.Message;

                Logger.Log(errorText, Logger.LogLevel.error);
                return;
            }

            if (result.ContainsKey("clear") && result["clear"].ToObject<bool>())
                cms.Items.Clear();
            
            if (!result.ContainsKey("entries") || result["entries"] == null)
                return;

            JArray entries = result["entries"] as JArray;
            if (entries == null) {
                Logger.Log("Error while getting entries of result from parameter " + parameterName, Tools.Logger.LogLevel.error);
                return;
            }

            foreach (var entry in entries) {
                string id = "";
                string text = "missing text";
                if (entry["id"] != null) {
                    id = entry["id"].ToObject<string>();                    
                }
                
                if (entry["text"] != null) {
                    try {
                        text = entry["text"].ToObject<string>();
                    } catch (Exception) {
                        Logger.Log("Invalid 'text' for entry of result from parameter " + parameterName, Tools.Logger.LogLevel.error);
                    }
                }

                string type = "label";
                if (entry["type"] != null) {
                    try {
                        type = entry["type"].ToObject<string>();
                    } catch (Exception) {
                        Logger.Log("Invalid 'type' for entry of result from parameter " + parameterName, Tools.Logger.LogLevel.error);
                    }
                }

                ToolStripItem newItem = null;

                switch (type) {
                    case "separator":
                        newItem = new ToolStripSeparator();
                        break;
                    case "checkbox":
                        bool isChecked = false;
                        if (entry["checked"] != null) {
                            try {
                                isChecked = entry["checked"].ToObject<bool>();
                            } catch (Exception) {
                                Logger.Log("Invalid value for 'checked' for entry of result from parameter " + parameterName, Tools.Logger.LogLevel.error);
                            }                            
                        }
                        newItem = new ToolStripButton(text) { Checked = isChecked };
                        break;
                    default:
                        newItem = new ToolStripMenuItem(text);
                        break;
                }

                newItem.Tag = id;
                cms.Items.Add(newItem);
            }

        }

        public static void CopyFolder(string sourceFolder, string destFolder) {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files) {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest, true);
            }
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders) {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest);
            }
        }
    }
}
