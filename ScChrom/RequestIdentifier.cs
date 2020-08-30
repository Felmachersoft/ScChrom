using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScChrom {
    /// <summary>
    /// This class encloses all necessary information to identify a request.
    /// </summary>
    public class RequestIdentifier :IEquatable<RequestIdentifier> {

        /// <summary>
        /// The exact address of the request. 
        /// If neither this nor AddressPattern are set, the requests address will be ignored.
        /// </summary>
        public string ExactAddress { get; set; }

        /// <summary>
        /// The address pattern of the request. 
        /// ? can be used for single char wildcard, * as multichar wildcard.
        /// If neither this nor ExactAddress are set, the requests address will be ignored.
        /// </summary>
        public string AddressPattern { get; set; }

        /// <summary>
        /// The HTTP method (like GET, POST, PUT, DELETE). Null if ignored.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// If null ignored.
        /// </summary>
        public bool? IsDownload{ get; set; }

        /// <summary>
        /// If null ignored.
        /// </summary>
        public bool? IsNavigation { get; set; }

        public bool MatchesEverything {
            get {
                return AddressPattern == "*";                    
            }
        }

        public RequestIdentifier(string input, string method = null, bool? isDownload = null) {
            Method = method;
            IsDownload = isDownload;

            if (string.IsNullOrWhiteSpace(input))
                input = "*";
            
            if (input.StartsWith("\"") && input.EndsWith("\"")) {
                ExactAddress = input.Substring(1, input.Length - 2);
            } else { 
                AddressPattern = input;
            }
        }

        /// <summary>
        /// Returns true if this matches the given values.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="isDownload"></param>
        /// <param name="isNavigation"></param>
        /// <returns></returns>
        public bool Match(string url = null, string method = null, bool? isDownload = null, bool? isNavigation = null) {

            if (MatchesEverything)
                return true;

            if (IsNavigation.HasValue){
                if (!isNavigation.HasValue)
                    return false;
                if (IsNavigation.Value != isNavigation.Value)
                    return false;            
            }
            if (IsDownload.HasValue) {
                if (!isDownload.HasValue)
                    return false;
                if (IsDownload.Value != isDownload.Value)
                    return false;
            }
            if (Method != null && Method != method)
                return false;

            if (ExactAddress != null && ExactAddress != url)
                return false;

            if (AddressPattern != null) {
                if (!Tools.Common.MatchText(url, AddressPattern))
                    return false;
            }

            return true;
        }        

        public bool Equals(RequestIdentifier other) {
            if (this.Method != other.Method ||
                this.IsDownload != other.IsDownload ||
                this.IsNavigation != other.IsNavigation) {
                    return false;
            }

            if (this.ExactAddress != null) 
                return this.ExactAddress == other.ExactAddress;
            return this.AddressPattern == other.AddressPattern;
        }
    }
}
