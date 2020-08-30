using CefSharp;
using ScChrom.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ScChrom.Filter {
    /// <summary>
    /// Buffers all data of a response and manipulates it, before passing it through.
    /// Mostly build upon the ExperimentalStreamResponseFilter.cs Cef# example filter.
    /// </summary>
    public class ContentManipulationFilter: IResponseFilter {
        private List<byte> _dataOutBuffer = new List<byte>();
        private string _url = null;
        private string _manipulationScript = null;
        private string _attachedText = null;
        private string _parameterName = null;
        private bool _allDataRead = false;

        public ContentManipulationFilter(string url, string manipulationScript = null, string attachedText = null, string parameterName = null) {
            _url = url;
            _manipulationScript = manipulationScript;
            _attachedText = attachedText;
            _parameterName = parameterName;
        }

        bool IResponseFilter.InitFilter() {
            return true;
        }

        FilterStatus IResponseFilter.Filter(Stream dataIn, out long dataInRead, Stream dataOut, out long dataOutWritten) {

            // if dataIn is null, all data has been read and 
            if (dataIn == null) {

                // first time having the complete response => handle manipulation
                if (!_allDataRead) {
                    Logger.Log("Resp. Manip.: All data read of response for " + _url, Logger.LogLevel.debug);
                    Logger.Log("Resp. Manip.: Starting manipulation of response for " + _url, Logger.LogLevel.debug);

                    string dataString = null;
                    try {                    
                        dataString = Encoding.UTF8.GetString(_dataOutBuffer.ToArray());
                        if (_manipulationScript != null)
                            dataString = executeScript(_url, dataString, _manipulationScript);
                        if (_attachedText != null)
                            dataString = appendString(_url, dataString, _attachedText);
                    } catch (Exception ex) {
                        Logger.Log("Resp. Manip.: Error while manipulating response for " + _url + ": " + ex.Message);
                    }

                    _dataOutBuffer = new List<byte>(Encoding.UTF8.GetBytes(dataString));

                    Logger.Log("Resp. Manip.: Finished manipulation of response for " + _url, Logger.LogLevel.debug);
                }
                // don't manipulate again
                _allDataRead = true;

                dataInRead = 0;
                dataOutWritten = 0;

                var maxWrite = Math.Min(_dataOutBuffer.Count, dataOut.Length);

                // Write the maximum portion that fits in dataOut.
                if (maxWrite > 0) {
                    dataOut.Write(_dataOutBuffer.ToArray(), 0, (int)maxWrite);
                    dataOutWritten += maxWrite;
                }

                // If dataOutBuffer is bigger than dataOut then we'll write the
                // data on the second pass
                if (maxWrite < _dataOutBuffer.Count) {
                    // Need to write more bytes than will fit in the output buffer. 
                    // Remove the bytes that were written already
                    _dataOutBuffer.RemoveRange(0, (int)(maxWrite - 1));
                    Logger.Log("Resp. Manip.: Writting out manipulated response for " + _url, Logger.LogLevel.debug);
                    return FilterStatus.NeedMoreData;
                }

                // All data was written, so we clear the buffer and return FilterStatus.Done
                _dataOutBuffer.Clear();

                Logger.Log("Resp. Manip.: Finished writting out manipulated response for " + _url, Logger.LogLevel.debug);
                return FilterStatus.Done;
            }

            // We're going to read all of dataIn
            dataInRead = dataIn.Length;

            var dataInBuffer = new byte[(int)dataIn.Length];
            dataIn.Read(dataInBuffer, 0, dataInBuffer.Length);

            // Add all the bytes to the dataOutBuffer
            _dataOutBuffer.AddRange(dataInBuffer);

            dataOutWritten = 0;

            // We haven't got all of our dataIn yet, so we keep buffering so that when it's finished
            // we can process the buffer, replace some words etc and then write it all out. 
            Logger.Log("Resp. Manip.: Read part data of response for " + _url, Logger.LogLevel.debug);
            return FilterStatus.NeedMoreData;
        }

       

        private string appendString(string url, string dataString, string attachedText) {
            
            Logger.Log("Resp. Manip.: Attached text to response of " + url + " via " + _parameterName, Logger.LogLevel.debug);

            return dataString + attachedText;            
        }

        private string executeScript(string url, string dataString, string script) {
            
            Logger.Log("Resp. Manip.: Manipulating response of " + url + " via script of " + _parameterName, Logger.LogLevel.debug);

            var key = new Jint.Key("response");
            JSEngine.Instance.Engine.SetValue(ref key, dataString);
            var urlkey = new Jint.Key("url");
            JSEngine.Instance.Engine.SetValue(ref urlkey, url);
            string result = JSEngine.Instance.ExecuteResult(script, _parameterName);
            if (result == null || result == "undefined")
                result = "";
            return result;
        }

        void IDisposable.Dispose() {
            // nothing to do
        }
    }
}
