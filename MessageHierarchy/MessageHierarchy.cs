using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public static class MessageHierarchy
{
    static MessageHierarchy()
    {
        // Get the all of the types in the Assembly that are derived from Message
        // TODO: Change to GetLoadableTypes rather than direct GetTypes()
        // TODO: Change to get all assemblies (filtering out assemblies that will not have message types)
        var _messageTypes = typeof(MessageHierarchy).Assembly.GetTypes().Where(typeof(Message).IsAssignableFrom);
        TypeTree.BuildTree(_messageTypes);
    }

    public static IEnumerable<Type> Ancestors(Type type)
    {
        return TypeTree.Ancestors(type);
    }

    public static IEnumerable<Type> Descendants(Type type)
    {
        return TypeTree.Descendants(type);
    }
}

internal class TypeTree
{
    private static TypeTreeNode _root;
    private static Dictionary<Type, TypeTreeNode> _typeToNode = new Dictionary<Type, TypeTreeNode>();
    internal static void BuildTree(IEnumerable<Type> types)
    {
        // One time pass through types to create initial node list.
        foreach (var type in types)
        {
            _typeToNode[type] = new TypeTreeNode(type);
        }
        // One more time build tree (adding child to parent).
        foreach (var type in _typeToNode.Keys)
        {
            if (_typeToNode.ContainsKey(type.BaseType))
            {
                // Found the parent so add child to parent.
                _typeToNode[type.BaseType].AddChild(_typeToNode[type]);
            }
            else
            {
                // If the base type is not found then it must be root.
                _root = _typeToNode[type];
            }
        }
    }
    internal static IEnumerable<Type> Ancestors(Type type)
    {
        TypeTreeNode typeNode = _typeToNode[type];
        while (typeNode != null)
        {
            yield return typeNode.Type;
            typeNode = typeNode.Parent;
        }
    }

    internal static IEnumerable<Type> Descendants(Type type)
    {
        // non recursive depth first search
        // Initialize a stack of typeNodes to visit with the type passed in.
        Stack<TypeTreeNode> _nodesToVisit = new Stack<TypeTreeNode>(new[] { _typeToNode[type] });
        while (_nodesToVisit.Count != 0)
        {
            var typeNode = _nodesToVisit.Pop();
            yield return typeNode.Type;
            foreach (var node in typeNode.Children)
            {
                _nodesToVisit.Push(node);
            }
        }
    }
}

internal class TypeTreeNode
{
    public Type Type { get; private set; }
    public TypeTreeNode Parent { get; private set; }
    public List<TypeTreeNode> Children { get; private set; }

    public TypeTreeNode(Type type)
    {
        Type = type;
        Children = new List<TypeTreeNode>();
    }
    internal void AddChild(TypeTreeNode typeTreeNode)
    {
        typeTreeNode.Parent = this;
        Children.Add(typeTreeNode);
    }
}
