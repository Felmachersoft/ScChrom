using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Enums;
using System.Drawing;
using ScChrom.Tools;

namespace ScChrom.Handler {
    public class DragHandler: IDragHandler {

        public Region draggableRegion = new Region(new Rectangle(0,0,0,0));
        public event Action<Region> RegionsChanged;
        
        public bool OnDragEnter(IWebBrowser chromiumWebBrowser, IBrowser browser, IDragData dragData, DragOperationsMask mask) {           
            return false;
        }

        /// <summary>
        /// Called whenever the regions marked with the css style "-webkit-app-region: drag/no-drag;" moved (after load and scroll for example)
        /// </summary>
        /// <param name="chromiumWebBrowser"></param>
        /// <param name="browser"></param>
        /// <param name="frame"></param>
        /// <param name="regions"></param>
        public void OnDraggableRegionsChanged(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IList<DraggableRegion> regions) {
            
            // update the draggeable region
            draggableRegion = null;
            if (regions != null && regions.Count > 0) {
                foreach (var region in regions) {
                    Logger.Log("Draggable region found: " + region.X + " - " + region.Y + " - " + region.Width + " - " + region.Height, Logger.LogLevel.debug);
                                        
                    var rect = new Rectangle(region.X, region.Y, region.Width, region.Height);

                    if (draggableRegion == null) {
                        draggableRegion = new Region(rect);
                    } else {
                        if (region.Draggable) {
                            draggableRegion.Union(rect);
                        } else {                            
                            // In the scenario where we have an outer region, that is draggable and it has
                            // an inner region that's not, we must exclude the non draggable.
                            // Not all scenarios are covered in this example.
                            draggableRegion.Exclude(rect);
                        }
                    }
                }
            }

            // inform about change
            if(RegionsChanged != null)
                RegionsChanged.Invoke(draggableRegion);
        }

        // help the GC
        public void Dispose() {
            RegionsChanged = null;
        }

    }
}
