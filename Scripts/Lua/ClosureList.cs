using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace MoDueler.Lua {

    /// <summary>
    /// A subrsibtble list of lua functions that can be executed in bulk.
    /// </summary>
    public class ClosureList {

        private readonly List<Closure> closureList = new List<Closure>();

        public ClosureList() { }

        public static ClosureList operator +(ClosureList list, Closure function) {
            list.closureList.Add(function);
            return list;
        }

        public static ClosureList operator -(ClosureList list, Closure function) {
            list.closureList.Remove(function);
            return list;
        }

        public void Add(Closure closure) => closureList.Add(closure);
        public void Remove(Closure closure) => closureList.Remove(closure);

        /// <summary>
        /// Execute all the closures; all with same provided arguments.
        /// </summary>
        public void CallAll(params object[] args) => closureList.ForEach((f) => f.Call(args));


    }
}
