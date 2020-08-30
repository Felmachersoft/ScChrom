using CefSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom.Filter {
    /// <summary>    
    /// StreamResponseFilter - copies all data from IResponseFilter.Filter    
    /// Mostly a copy of the PassThruResponseFilter.cs file from Cef#
    /// </summary>
    public class StreamResponseFilter : IResponseFilter {
        private Stream _responseStream;

        public StreamResponseFilter(Stream stream) {
            _responseStream = stream;
        }

        bool IResponseFilter.InitFilter() {
            // Will only be called a single time.
            // The filter will not be installed if this method returns false.
            return true;
        }

        FilterStatus IResponseFilter.Filter(Stream dataIn, out long dataInRead, Stream dataOut, out long dataOutWritten) {
            if (dataIn == null) {
                dataInRead = 0;
                dataOutWritten = 0;

                return FilterStatus.Done;
            }
            
            // Calculate how much data we can read, in some instances dataIn.Length is
            // greater than dataOut.Length
            dataInRead = Math.Min(dataIn.Length, dataOut.Length);
            dataOutWritten = dataInRead;

            var readBytes = new byte[dataInRead];
            dataIn.Read(readBytes, 0, readBytes.Length);
            dataOut.Write(readBytes, 0, readBytes.Length);

            // Write buffer to the memory stream
            _responseStream.Write(readBytes, 0, readBytes.Length);

            // If we read less than the total amount avaliable then we need
            // return FilterStatus.NeedMoreData so we can then write the rest
            if (dataInRead < dataIn.Length) {
                return FilterStatus.NeedMoreData;
            }

            return FilterStatus.Done;
        }

        void IDisposable.Dispose() {
            _responseStream = null;
        }
    }
}
