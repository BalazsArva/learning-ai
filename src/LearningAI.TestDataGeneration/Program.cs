using LearningAI.TestDataGeneration.Apis.Knowledgebase;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Compact;

try
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo
        .Console(formatter: new RenderedCompactJsonFormatter())
        .CreateLogger();

    var host = new HostBuilder()
        .ConfigureLogging(logging =>
        {
            logging.AddSerilog(Log.Logger);
        })
        .ConfigureServices((context, services) =>
        {
            services.AddSingleton<KnowledgebaseTestDataGenerator>();
            services.AddSingleton(srvProvider =>
            {
                return Refit.RestService.For<IKnowledgebaseApi>("http://localhost:5011");
            });
        })
        .Build();

    var dataGenerator = host.Services.GetRequiredService<KnowledgebaseTestDataGenerator>();

    await dataGenerator.GenerateKnowledgebaseData();
}
catch (Exception e)
{
    Console.WriteLine($"Application failed to execute. {e.Message}");
    throw;
}