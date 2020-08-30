using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom.JsController {
    /// <summary>
    /// A common interface for all .net objects that can be called inside
    /// the Jint JavaScript context.
    /// 
    /// <para>Keep in mind:</para>
    /// <para>1. Objects must provide either none explicit or a parameterless constructor.</para>
    /// <para>2. Only methods are supported; properties will NOT be accessable.</para>
    /// <para>3. Methodnames should start lower cased, to match their names in the browser context.</para>
    /// 
    /// If a static public property named <b>MethodInfos</b> that returns a List of 
    /// <see cref="JsController.JsControllerMethodInfo"/> is avilable, it will be used instead of 
    /// creating generic info about it.
    /// 
    /// </summary>
    public interface IJintContextCallable {
    }
}
