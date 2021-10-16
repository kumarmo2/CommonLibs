using System;
using System.Linq;
using System.Threading.Tasks;
using Core.CommonLibs.RabbitMq;

namespace Core.ConsolePracticeApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var manager = new RabbitMqManager();

            var tasks = Enumerable.Range(0, 10).Select(async i =>
            {
                await using (var channel = await manager.GetChannel())
                {
                    Console.WriteLine($"Got channel for func: {i}, processing now.");
                    await Task.Delay(3000);
                    Console.WriteLine($"Func: {i} completed");

                };
            });

            await Task.WhenAll(tasks);
        }
    }
}
