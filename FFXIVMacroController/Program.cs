using Microsoft.AspNetCore.StaticFiles;
using Drk.AspNetCore.MinimalApiKit;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using FFXIVMacroController.Pigeonhole;
using FFXIVMacroController.Seer;
using FFXIVMacroController.Grunt;
using FFXIVMacroController.Helper;
using FFXIVMacroController.Model;
using System.Text.Json;
using FFXIVMacroController.Quotidian.Enums;
using System.Reflection.Emit;


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

CancellationTokenSource cancellationTokenSource = null;

BmpPigeonhole.Initialize(AppContext.BaseDirectory + @"\Grunt.ApiTest.json");

//BmpSeer.Instance.GameStarted += EventHelper.SendTest;

BmpSeer.Instance.SetupFirewall("FFXIVMacroController");

app.MapPost("/Init", async () =>
{
    BmpSeer.Instance.Start();

    BmpGrunt.Instance.Start();

    var json = File.ReadAllText("config.json");

    var sampleData = EventHelper.ConvertJsonToList(json);

    var keyList = Enum.GetValues(typeof(Keys)).Cast<Keys>().Select(n => new
    {
        label = Enum.GetName(n),
        value = (int)n
    }).Distinct();


    var typeList = Enum.GetValues(typeof(Types)).Cast<Types>().Select(n => new
    {
        label = Enum.GetName(n),
        value = n
    }).Distinct();


    await Task.Delay(1000);

    var game = BmpSeer.Instance.Games.Values.FirstOrDefault();

    var resultObj = new
    {
        rootData = sampleData,
        keyOptions = keyList,
        typeOptions = typeList,
        gamePath = game?.GamePath
    };

    return JsonSerializer.Serialize(resultObj);
});

app.MapPost("/Start", async (CategoryModel model) =>
{
    if (!BmpSeer.Instance.Started)
    {
        BmpSeer.Instance.Start();

        BmpGrunt.Instance.Start();

        await Task.Delay(1000);
    }

    var game = BmpSeer.Instance.Games.Values.FirstOrDefault();

    if (game == null)
    {
        return "";
    }

    if (model.macroList.Count == 0)
    {
        return "";
    }

    cancellationTokenSource = new CancellationTokenSource();
    CancellationToken cancellationToken = cancellationTokenSource.Token;
    await EventHelper.SendInput_Token(game, model.macroList, cancellationToken);

    bool isStarted = !BmpGrunt.Instance.Started || !BmpSeer.Instance.Started;

    return isStarted ? "1" : "0";
});

app.MapPost("/Stop", async () =>
{
    BmpGrunt.Instance.Stop();

    BmpSeer.Instance.Stop();

    if (cancellationTokenSource != null)
    {
        cancellationTokenSource.Cancel();
        return true;
    }

    return false;
});

app.MapPost("/Save", async (MacroRootModel model) =>
{
    bool isSuccess = false;
    
    try
    {
        string json = JsonSerializer.Serialize(model);
        File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"\config.json", json);
        isSuccess = true;
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
    
    return isSuccess;
});

app.MapPost("/LocateMouse", async () =>
{
    if (!BmpSeer.Instance.Started)
    {
        BmpSeer.Instance.Start();

        BmpGrunt.Instance.Start();

        await Task.Delay(1000);
    }

    var game = BmpSeer.Instance.Games.Values.FirstOrDefault();

    var (x, y) = MouseHelper.LocateMouse(game.Process.MainWindowHandle);

    var resultObj = new
    {
       coordinateX = x,
       coordinateY = y
    };

    return JsonSerializer.Serialize(resultObj);
});


app.RunAsDesktopTool();