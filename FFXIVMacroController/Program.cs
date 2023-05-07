using Microsoft.AspNetCore.StaticFiles;
using Drk.AspNetCore.MinimalApiKit;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using FFXIVMacroController.Pigeonhole;
using FFXIVMacroController.Seer;
using FFXIVMacroController.Grunt;
using FFXIVMacroController.Helper;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(options => {
    options.SerializerOptions.PropertyNamingPolicy = null;
});

var app = builder.Build();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".vue"] = "application/javascript";

app.UseFileServer(new FileServerOptions
{
    RequestPath = "",
    FileProvider = new Microsoft.Extensions.FileProviders
                    .ManifestEmbeddedFileProvider(typeof(Program).Assembly, "ui"),
    StaticFileOptions = {
        ContentTypeProvider = provider
    }
});

BmpPigeonhole.Initialize(AppContext.BaseDirectory + @"\Grunt.ApiTest.json");

BmpSeer.Instance.GameStarted += EventHelper.SendTest;

BmpSeer.Instance.SetupFirewall("FFXIVMacroController");

app.MapGet("/Init", async () =>
{
    return "";
});


app.MapPost("/Start", async () =>
{
    BmpSeer.Instance.Start();

    BmpGrunt.Instance.Start();

    return "";
});

app.MapPost("/Stop", async () =>
{
    BmpGrunt.Instance.Stop();

    BmpSeer.Instance.Stop();

    return "";
});


app.RunAsDesktopTool();


//using System;
//using System.Text.Json;
//using System.Threading.Tasks;
//using FFXIVMacroController.Pigeonhole;
//using FFXIVMacroController.Quotidian.Enums;
//using FFXIVMacroController.Quotidian.Structs;
//using FFXIVMacroController.Seer;
//using FFXIVMacroController.Seer.Events;
//using FFXIVMacroController.Model;

//namespace FFXIVMacroController.Grunt.ApiTest;

//internal class Program
//{
//    private static void Main()
//    {
//        BmpPigeonhole.Initialize(AppContext.BaseDirectory + @"\Grunt.ApiTest.json");

//        BmpSeer.Instance.GameStarted += SendTest;

//        BmpSeer.Instance.SetupFirewall("FFXIVMacroController");

//        Console.WriteLine("Hit enter to start Grunt");

//        Console.ReadLine();

//        while (true)
//        {
//            BmpSeer.Instance.Start();

//            BmpGrunt.Instance.Start();

//            Console.ReadLine();

//            BmpGrunt.Instance.Stop();

//            BmpSeer.Instance.Stop();

//            Console.WriteLine("Grunt stopped. Hit enter to start it again.");

//            Console.ReadLine();
//        }
//    }

//    private static void SendTest(GameStarted seerEvent)
//    {
//        var game = seerEvent.Game;

//        Console.WriteLine("Detected game pid " + game.Pid + ", sleep a thread for 3000ms to allow Seer to parse the dat files.");

//        string jsonText = File.ReadAllText("config.json");

//        var macroList = ConvertJsonToList(jsonText);

//        Task.Run(async () =>
//        {
//            await Task.Delay(3000);

//            Console.WriteLine("Trying to doot on game pid " + game.Pid + ".");

//            foreach (var item in macroList)
//            {
//                Console.WriteLine($"Key: {item.key}");

//                switch (item.type)
//                {
//                    case Types.button:
//                        if (game != null && !await game.SendKeyArray(item.key)) Console.WriteLine("Failed to call game pid " + game.Pid + " to input keys :(");
//                        break;
//                    case Types.mouse:
//                        break;
//                }

//                await Task.Delay(item.sleep);
//            }
//        });
//    }

//    private static List<MacroModel> ConvertJsonToList(string jsonText)
//    {
//        List<MacroModel> macroList = new List<MacroModel>();

//        JsonDocument jsonDocument = JsonDocument.Parse(jsonText);

//        foreach (JsonElement element in jsonDocument.RootElement.EnumerateArray())
//        {
//            MacroModel model = new MacroModel();

//            string typeStr = element.GetProperty("type").GetString();

//            if (Enum.TryParse(typeStr, out Types type))
//            {
//                model.type = type;
//            }

//            string commandStr = element.GetProperty("command").GetString();

//            if (Enum.TryParse(commandStr, out Keys key))
//            {
//                model.key = key;
//            }

//            model.sleep = element.GetProperty("sleep").GetInt16();
//            macroList.Add(model);
//        }

//        return macroList;
//    }
//}