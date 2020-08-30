using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom.BrowserJs {
    /// <summary>
    /// A common interface for all .net objects that can be called inside
    /// the browsers JavaScript context via the gloabl ScChrom object.
    /// 
    /// <para>Keep in mind:</para>
    /// <para>1. Objects must provide either none explicit or a parameterless constructor.</para>
    /// <para>2. Only methods are supported; properties will NOT be accessable.</para>
    /// 
    /// If a static public property named <b>MethodInfos</b> that returns a List of 
    /// <see cref="JsController.JsControllerMethodInfo"/> is avilable, it will be used instead of 
    /// creating generic info about it.
    /// 
    /// </summary>
    interface IBrowserContextCallable {      
    }
    
}
