using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Duel {
    public class FlowOutputEventArgs : EventArgs {

        /// <summary>
        /// The player that should read the messages.
        /// <para><c>null</c> if all players should read the messages.</para>
        /// </summary>
        public readonly SimplifiedPlayer Target;

        /// <summary>
        /// A set of instructions for the the player's system to use.
        /// <para>The first value should be a function name.</para>
        /// </summary>
        public readonly string[] Messages;

        public FlowOutputEventArgs(SimplifiedPlayer target, params string[] messages) {
            Target = target;
            Messages = messages;
        }

    }
}
