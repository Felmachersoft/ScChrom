namespace ScChrom.BrowserJs {
    /// <summary>
    /// This is the base class which is used for the base object in the 
    /// browsers javascript context.
    /// </summary>
    public class ScChromBaseJsController  {
        
        
        public void log(object content = null) {
            if(content != null)
                MainController.Instance.WriteOut(content.ToString());
        }
        
        public void err(object content = null) {
            if (content != null)
                MainController.Instance.WriteErrorOut(content.ToString());
        }

        public void openLink(string url) {
            Tools.Common.OpenLinkInDefaultBrowser(url);
        }

        public bool matchText(string text, string pattern) {
            if (text == null || pattern == null)
                return false;
            return Tools.Common.MatchText(text, pattern);
        }
       
    }
}
