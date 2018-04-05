using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public static class Program
{
    public static void Main()
    {
        var type = typeof(Message);
        Console.WriteLine("MessageChild has " + MessageHierarchy.Ancestors(type).Count() + " Ancestors");
        foreach (var parent in MessageHierarchy.Ancestors(type))
        {
            Console.WriteLine(parent);
        }

        Console.WriteLine($"{type} has {MessageHierarchy.Descendants(type).Count()} Descendants");
        foreach (var descendant in MessageHierarchy.Descendants(type))
        {
            Console.WriteLine(descendant);
        }
    }
}
