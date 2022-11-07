using Godot;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Nodes {


    /// <summary>
    /// A proxy type for <see cref="Node"/>s that hides access to features that would allow deletion.
    /// <para>Offers generic support for all node types.</para>
    /// </summary>
    [MoonSharpUserData]
    public class NodeProxy<T> where T : Node {

        [MoonSharpHidden]
        protected readonly T RealNode = null;

        [MoonSharpHidden]
        public NodeProxy(T node) {
            RealNode = node;
        }

        public void PrintTree() => RealNode.PrintTreePretty();

        public void AddChild(Node node) => RealNode.AddChild(node);

        public void RemoveChild(Node node) => RealNode.RemoveChild(node);

        //TODO: Renable other methods for mainuplating children.

        /// <summary>
        /// Store any value as part of this node.
        /// </summary>
        public void SetMeta(string key, object value) => RealNode.SetMeta(key, value);

        /// <summary>
        /// Retreive values set using <see cref="SetMeta(string, object)"/>.
        /// <para>Returns null if no value was found.</para>
        /// </summary>
        public object GetMeta(string key) {
            try {
                return RealNode.GetMeta(key);
            }
            catch {
                GD.Print("Meta requested with key '" + key + "' but no value was found");
                return null;
            }
        }

        /// <summary>
        /// Removes a value set using <see cref="SetMeta(string, object)"/>
        /// </summary>
        /// <param name="key"></param>
        public void RemoveMeta(string key) {
            try {
                RealNode.RemoveMeta(key);
            }
            catch { }
        }

        public object this[string key] {
            set => SetMeta(key, value);
            get => GetMeta(key);
        }


        /// <summary>
        /// <see cref="Node.GetNode(NodePath)"/> but with implicit string to path conversion.
        /// </summary>
        public Node GetNode(string path) {
            return RealNode.GetNode((NodePath)path);
        }

        /// <summary>
        /// Retrieves a child using it's name instead by using <see cref="GetNode(string)"/>
        /// <para><see cref="Node.GetChild(int)"/> can still be used when indexing.</para>
        /// </summary>
        public Node GetChild(string path) {
            return RealNode.GetNode((NodePath)path);
        }

    }

    /// <summary>
    /// A version of <see cref="NodeProxy{T}"/> that makes public all the features that allow for node deletion.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [MoonSharpUserData]
    public class DeletableNodeProxy<T> : NodeProxy<T> where T : Node {

        [MoonSharpHidden]
        public DeletableNodeProxy(T node) : base(node) { }

        public void Free() => RealNode.Free();

        public void QueueFree() => RealNode.QueueFree();

        public void RemoveAndSkip() => RealNode.RemoveAndSkip();

        public void ReplaceBy(Node node, bool keepData = false) => RealNode.ReplaceBy(node, keepData);

        public Node Owner {
            get => RealNode.Owner;
            set => RealNode.Owner = value;
        }

        public string Name {
            get => RealNode.Name;
            set { RealNode.Name = value; }
        }

        public Node GetParent() => RealNode.GetParent();



    }


}
