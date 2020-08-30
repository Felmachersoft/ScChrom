using CefSharp;
using ScChrom.BrowserJs;
using ScChrom.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom.JsController {
    public class FilesystemController: IBrowserContextCallable, IJintContextCallable {

        public static List<JsControllerMethodInfo> MethodInfos {
            get {
                return new List<JsControllerMethodInfo>() {                    
                    new JsControllerMethodInfo(
                        "getDirectoryContent",
                        "Gets all directories and files inside the given directory.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "path",
                                "The path of the directory.",
                                JsControllerMethodInfo.DataType.text
                            ),
                        }, 
                        new JsControllerMethodReturnValue(
                            @"A json encoded array containing objects of following structure:<br>
                                {<br>
                                    &nbsp;&nbsp;&nbsp;""<b>path</b>"" : ""The absolute path to the directory or file"",<br>
                                    &nbsp;&nbsp;&nbsp;""<b>isFile</b>"" : ""A string; 'true' or 'false'. 'true' if it's a file"",<br>
                                    &nbsp;&nbsp;&nbsp;""<b>size</b>"" : ""A string; the size of a file in bytes. Missing for directories or missing rights to read a files size"",<br>
                                }<br>
                            ",
                            JsControllerMethodInfo.DataType.json
                        )
                    ),
                    new JsControllerMethodInfo(
                        "getDrives",
                        "Gets all drives of this pc.",
                        null, 
                        new JsControllerMethodReturnValue(
                            @"A json encoded array containing objects of following structure:<br>
                                {<br>
                                    &nbsp;&nbsp;&nbsp;""<b>name</b>"" : ""Name of the drive (like 'c:\\')"",<br>
                                    &nbsp;&nbsp;&nbsp;""<b>label</b>"" : ""The label of the drive (like 'system')"",<br>
                                    &nbsp;&nbsp;&nbsp;""<b>total_size</b>"" : ""Total size of the drive"",<br>
                                    &nbsp;&nbsp;&nbsp;""<b>available_space</b>"" : ""Available space on the drive"",<br>
                                    &nbsp;&nbsp;&nbsp;""<b>path</b>"" : ""The path of the drive (like 'c:\\')"",<br>
                                }<br>
                            ",
                            JsControllerMethodInfo.DataType.json
                        )
                    ),
                    new JsControllerMethodInfo(
                        "getFileContent",
                        "Gets the complete UTF-8 encoded content of a file.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "filepath",
                                "The path of the file.",
                                JsControllerMethodInfo.DataType.text
                            ),
                        },
                        new JsControllerMethodReturnValue(
                            @"The content of the file or an empty string if reading the file failed.",
                            JsControllerMethodInfo.DataType.text
                        )
                    ),
                    new JsControllerMethodInfo(
                        "appendToFile",
                        "Appends the given UTF-8 encoded content to the selected file. Creates the file if not present.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "path",
                                "The path of the file.",
                                JsControllerMethodInfo.DataType.text
                            ),
                            new JsControllerMethodParameter(
                                "contents",
                                "The content to append to the file.",
                                JsControllerMethodInfo.DataType.text
                            ),
                        },
                        new JsControllerMethodReturnValue(
                            @"'true' if it worked correctly, otherwise the error message.",
                            JsControllerMethodInfo.DataType.text
                        )
                    ),
                    new JsControllerMethodInfo(
                        "writeToFile",
                        "Writes the given UTF-8 encoded content into the selected file; overwritting the old content if any. Creates the file if not present.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "path",
                                "The path of the file.",
                                JsControllerMethodInfo.DataType.text
                            ),
                            new JsControllerMethodParameter(
                                "contents",
                                "The content to write into the file.",
                                JsControllerMethodInfo.DataType.text
                            ),
                        },
                        new JsControllerMethodReturnValue(
                            @"'true' if it worked correctly, otherwise the error message.",
                            JsControllerMethodInfo.DataType.text
                        )
                    ),
                    new JsControllerMethodInfo(
                        "deleteFile",
                        "Writes the given UTF-8 encoded content into the selected file; overwritting the old content if any. Creates the file if not present.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "path",
                                "The path of the file.",
                                JsControllerMethodInfo.DataType.text
                            )
                        },
                        new JsControllerMethodReturnValue(
                            @"'true' if it worked correctly, otherwise the error message.",
                            JsControllerMethodInfo.DataType.text
                        )
                    ),
                    new JsControllerMethodInfo(
                        "fileExists",
                        "Checks if the given file exists.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "path",
                                "The path of the file.",
                                JsControllerMethodInfo.DataType.text
                            )
                        },
                        new JsControllerMethodReturnValue(
                            @"true if the file exists, otherwise false.",
                            JsControllerMethodInfo.DataType.boolean
                        )
                    ),
                    new JsControllerMethodInfo(
                        "directoryExists",
                        "Checks if the given directory exists.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "path",
                                "The path of the directory.",
                                JsControllerMethodInfo.DataType.text
                            )
                        },
                        new JsControllerMethodReturnValue(
                            @"true if the directory exists, otherwise false.",
                            JsControllerMethodInfo.DataType.boolean
                        )
                    ),
                    new JsControllerMethodInfo(
                        "getAbsolutePath",
                        "Takes a relative path and returns an absolute path.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "path",
                                "The relative path. Can contain special folders like %desktop% or %configfile_dir%, which is the directory holding the used config file.",
                                JsControllerMethodInfo.DataType.text
                            )
                        },
                        new JsControllerMethodReturnValue(
                            @"The absolute path or if an error occurs 'false'.",
                            JsControllerMethodInfo.DataType.text
                        )
                    ),
                    new JsControllerMethodInfo(
                        "getConfigfileInfos",
                        "Get the infos from a confg file.",
                        new List<JsControllerMethodParameter>() {
                            new JsControllerMethodParameter(
                                "filepath",
                                "The path of the config file.",
                                JsControllerMethodInfo.DataType.text
                            )
                        },
                        new JsControllerMethodReturnValue(
                            @"The infos as a dictionary. Will only have an 'error' field with the error message if one occured while getting the infos.",
                            JsControllerMethodInfo.DataType.dictionary
                        )
                    )


                    
                };
            }
        }


        public string getDirectoryContent(string path) {
            path = path.Replace("/", "\\");

            if (!Common.IsValidLocalPath(path))
                return "false";

            path = Path.GetFullPath(path);

            try {

                var ret = new List<Dictionary<string, string>>();

                var dirs = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);                

                foreach (var dir in dirs) {
                    var fileInfos = new Dictionary<string, string>();
                    fileInfos.Add("path", dir.Replace("\\", "/"));
                    fileInfos.Add("isFile", "false");
                    ret.Add(fileInfos);
                }

                var files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
                foreach (var filepath in files) {
                    var fileInfos = new Dictionary<string, string>();
                    fileInfos.Add("path", filepath.Replace("\\", "/"));
                    fileInfos.Add("isFile", "true");
                    try {
                        var fi = new FileInfo(filepath);
                        fileInfos.Add("size", fi.Length + "");
                    } catch (Exception ex) {
                        Logger.Log("Error while getting fileinfo (" + filepath + ") :" + ex.Message, Logger.LogLevel.error);
                        fileInfos.Add("error", "Can not read file size");
                    }
                    ret.Add(fileInfos);
                }
                
                return Newtonsoft.Json.Linq.JArray.FromObject(ret).ToString(Newtonsoft.Json.Formatting.None);
            } catch (Exception ex) {
                Logger.Log("Error while getting directory content (" + path + ") :" + ex.Message, Logger.LogLevel.error);
                return "false";
            }            
                
        }

        public string getDrives() {
            var ret = new List<Dictionary<string, string>>();

            DriveInfo[] drives = null;

            try {
                drives = DriveInfo.GetDrives();
            } catch (Exception) {
                return "false";
            }
            
            foreach (var di in drives) {
                var dict = new Dictionary<string, string>();

                dict.Add("name", di.Name);
                dict.Add("label", di.VolumeLabel);
                dict.Add("total_size", di.TotalSize + "");
                dict.Add("available_space", di.AvailableFreeSpace + "");
                dict.Add("path", di.RootDirectory.FullName);

                ret.Add(dict);
            }            

            return Newtonsoft.Json.Linq.JArray.FromObject(ret).ToString(Newtonsoft.Json.Formatting.None);

        }

        public string getFileContent(string filepath) {
            try {
                return File.ReadAllText(filepath);
            } catch (Exception ex) {
                Logger.Log("Error while reading file (" + filepath + ") :" + ex.Message, Logger.LogLevel.error);
                return "";
            }
        }
        
        public Dictionary<string,string> getConfigfileInfos(string filepath) {            

            char[] startChars = null;
            // ensure file starts in a sane way (to prevent loading of unnecessary files)
            try {                    
                int charCount = 10;
                using (var stream = File.OpenRead(filepath))
                using (var reader = new StreamReader(stream, Encoding.UTF8)) {
                    char[] buffer = new char[charCount];
                    int n = reader.ReadBlock(buffer, 0, charCount);

                    startChars = new char[n];

                    Array.Copy(buffer, startChars, n);
                }
            } catch (FileNotFoundException ex) {
                Logger.Log("Error while attempting to read config file: " + ex.Message, Logger.LogLevel.info);
                return new Dictionary<string, string>() { { "error", "File not found" } };
            } catch (UnauthorizedAccessException ex) {
                Logger.Log("Error while attempting to read config file: " + ex.Message, Logger.LogLevel.info);
                return new Dictionary<string, string>() { { "error", "Not allowed to read file" } };
            } catch (Exception ex) {
                Logger.Log("Error while attempting to read config file: " + ex.Message, Logger.LogLevel.info);
                return new Dictionary<string, string>() { { "error", "Failed to read file" } };
            }
           
            // get the infos
            string infoLine = null;
            try {	        
		        infoLine = File.ReadLines(filepath).First();                
	        } catch (Exception ex) {
		        Logger.Log("Error while attempting to read config file: " + ex.Message, Logger.LogLevel.info);
                return new Dictionary<string, string>() { { "error", "Failed to read file" } };
	        }

            // parse them
            var infos = Arguments.ParseInfos(infoLine);
            if (infos == null)
                return new Dictionary<string, string>();

            if (infos.ContainsKey("error")) {
                infos.Remove("error");
                Logger.Log("Removed field 'error' from config file info while getting it via 'getConfigfileInfos' in the FilesystemController");
            }

            return infos;            
        }

        public string appendToFile(string path, string contents) {
            try {
                File.AppendAllText(path, contents);
            } catch (Exception ex) {
                return ex.Message;
            }
            return "true";
        }

        public string writeToFile(string path, string contents) {
            try {
                File.WriteAllText(path, contents);
            } catch (Exception ex) {
                return ex.Message;
            }
            return "true";
        }

        public string deleteFile(string path) {
            try {
                File.Delete(path);
            } catch (Exception ex) {
                return ex.Message;
            }
            return "true";
        }

        public bool fileExists(string path) {
            try {
                return File.Exists(path);
            } catch (Exception) {
                return false;
            }            
        }

        public bool directoryExists(string path) {
            try {
                return Directory.Exists(path);
            } catch (Exception) {
                return false;
            }
        }

        public string getAbsolutePath(string path) { 
            try {
                if(path.Contains("%configfile_dir%")) {
                    if(Arguments.ConfigfilePath == null)
                        return "false";
                    path = path.Replace("%configfile_dir%", Arguments.ConfigfilePath);
                }                
                return Path.GetFullPath(path);
	        } catch (Exception ex) {
                Logger.Log("Failed to get absolute for " + ex.Message, Logger.LogLevel.error);
                return "false";
	        }            
        }
        
    }
}
