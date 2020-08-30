using CefSharp;
using CefSharp.Handler;
using ScChrom.Filter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScChrom.CustomResourceRequestHandlers {
    public class OutputResponse_Manipulate: ResourceRequestHandler {
        private string _appendedText = null;
        private string _manipulateScript = null;
        private string _parameterName = null;
        private bool _ignoreRequest = false;
        private bool _useOnlyOnce = false;

        public bool UseOnlyOnce {
            get { return _useOnlyOnce; }
            set {
                _useOnlyOnce = value;
                if(value)
                    _ignoreRequest = false;                
            }
        }

        public OutputResponse_Manipulate(string manipulateScript, string appendText, string parameterName, bool useOnlyOnce = false) : base(){
            _appendedText = appendText;
            _manipulateScript = manipulateScript;
            _parameterName = parameterName;
            UseOnlyOnce = useOnlyOnce;
        }
        
        protected override IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response) {
            // ignore redirects and/or errors
            if (response.StatusCode != 200 || _ignoreRequest)
                return base.GetResourceResponseFilter(chromiumWebBrowser, browser, frame, request, response);
            
            if (UseOnlyOnce)
                _ignoreRequest = true;            

            return new ContentManipulationFilter(request.Url,_manipulateScript, _appendedText, _parameterName);

        }
    }
}
