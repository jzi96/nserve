using Microsoft.Extensions.FileProviders;
using System.CommandLine;

namespace nserve;

public class Program
{
    public static Task<int> Main(string[] args)
    {
        // https://learn.microsoft.com/en-us/dotnet/standard/commandline/define-commands#define-a-root-command
        var portOption = new Option<int>(new string[] { "--port", "-p" }, "The port to run on")
        {
            IsRequired = false
        };
        portOption.SetDefaultValue(5001);
        var securePortOption = new Option<int?>(new string[] { "--sport", "-sp" }, "The port to run on ssh")
        {
            IsRequired = false
        };
        securePortOption.SetDefaultValue(null);
        var folderArgument = new Argument<string>(name: "folder", description: "folder to host");
        folderArgument.SetDefaultValue(() => AppDomain.CurrentDomain.BaseDirectory);

        var rootCommand = new RootCommand("run");
        rootCommand.AddOption(portOption);
        rootCommand.AddArgument(folderArgument);
        rootCommand.AddOption(securePortOption);
        rootCommand.SetHandler((portOption, folderArgument, securePortOption) =>
        {
            RunServer(args, folderArgument, portOption, securePortOption);
        }, portOption, folderArgument, securePortOption);


        return rootCommand.InvokeAsync(args);
    }

    private static void RunServer(string[] args, string appFolder, int port, int? securePort)
    {
        var fp = new PhysicalFileProvider(appFolder, Microsoft.Extensions.FileProviders.Physical.ExclusionFilters.DotPrefixed);
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ContentRootPath = appFolder, //oder webrp
            WebRootPath = appFolder
        });

        // Add services to the container.
        builder.Services.AddAuthorization();
        builder.Services.AddDirectoryBrowser();
        builder.WebHost.ConfigureKestrel((context, serverOptions) =>
        {
            serverOptions.ListenAnyIP(port);
            if(securePort.HasValue)
                serverOptions.ListenAnyIP(securePort.Value, opt =>
                {
                    opt.UseHttps();
                });
        });

        var app = builder.Build();
        app.UseDefaultFiles();
        
        app.UseStaticFiles();
        app.UseDirectoryBrowser();
        app.Run();
    }
}
