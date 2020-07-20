using System;
using BedrockServerConfigurator.Library.Commands;
using BedrockServerConfigurator.Library.Entities;
using BedrockServerConfigurator.Library.Location;

namespace BedrockServerConfigurator.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var first = PublicCoordinate.GetPublicCoordinate(7, 4, 3);
            var second = PublicCoordinate.GetPublicCoordinate(17, 6, 2);

            var distance = PublicCoordinate.Distance(first, second);
            Console.WriteLine(distance);

            var one = new Entity("***REMOVED***");
            var two = new Entity("***REMOVED***");

            var rp = new CommandBuilder().Teleport(one, two);
            Console.WriteLine(rp);

            Console.WriteLine(first);
        }
    }
}
