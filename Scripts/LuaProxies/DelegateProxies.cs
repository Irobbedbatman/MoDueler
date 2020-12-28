using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.LuaProxies {
    public static class DelegateProxies {

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
            public ActionProxy(Action realShader) {
                RealAction = realShader;
            }


        }
    }
}
