using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Lua.Proxies {
    public static class DelegateProxies {

        /// <summary>
        /// Automated convertor to C# actions from a lua function.
        /// </summary>
        public static void RegisterActionConverter() {
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.ClrFunction, typeof(Action),
                (value) => {
                    var function = value.Callback;
                    return (Action)(() => function.Invoke(null, new List<DynValue>()));
                }
            );
        }

        public class ActionProxy {

            public readonly Action RealAction;

            [MoonSharpHidden]
            public ActionProxy(Action realAction) {
                RealAction = realAction;
            }


        }
    }
}
