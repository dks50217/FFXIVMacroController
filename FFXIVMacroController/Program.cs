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
using Newtonsoft.Json.Linq;

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
        value = n
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
    var game = BmpSeer.Instance.Games.Values.FirstOrDefault();

    if (game == null)
    {
        return "";
    }

    if (model.macroList.Count == 0)
    {
        return "";
    }

    await EventHelper.SendInput(game, model.macroList);

    return "";
});

app.MapPost("/Stop", async () =>
{
    BmpGrunt.Instance.Stop();

    BmpSeer.Instance.Stop();

    return "";
});


app.RunAsDesktopTool();