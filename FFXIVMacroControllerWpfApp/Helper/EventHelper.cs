using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFXIVMacroController.Pigeonhole;
using FFXIVMacroController.Quotidian.Enums;
using FFXIVMacroController.Quotidian.Structs;
using FFXIVMacroController.Seer;
using FFXIVMacroController.Grunt;
using FFXIVMacroControllerWpfApp.Model;
using FFXIVMacroController.Seer.Events;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;
using System.Text.RegularExpressions;
using System.Drawing;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace FFXIVMacroController.Helper
{
    public class EventHelper
    {
        /// <summary>
        /// 發送遊戲事件
        /// </summary>
        /// <param name="game"></param>
        /// <param name="jsonText"></param>
        public static async Task SendInput(Game game, List<MacroModel> macroList)
        {
            Console.WriteLine("Trying to doot on game pid " + game.Pid + ".");

            foreach (var item in macroList)
            {
                item.key = (Keys)item.keyNumber;

                Console.WriteLine($"Key: {item.key}");

                if (!BmpSeer.Instance.Started || !BmpGrunt.Instance.Started)
                {
                    Console.WriteLine($"已經暫停!");
                    break;
                }

                var s = TimeSpan.FromSeconds(item.sleep).TotalMilliseconds;
                item.sleep = Convert.ToInt32(s);

                switch (item.type)
                {
                    case Types.button:
                        if (game != null && !await game.SendKeyArray(item.key)) Console.WriteLine("Failed to call game pid " + game.Pid + " to input keys :(");
                        break;
                    case Types.text:

                        string[] lines = item.inputText.Split(
                            new string[] { "\r\n", "\r", "\n" },
                            StringSplitOptions.None
                        );

                        foreach (string line in lines)
                        {
                            await game.SendLyricLine(line);
                            await Task.Delay(item.sleep);
                        }

                        break;
                }

                await Task.Delay(item.sleep);
            }
        }

        public static async Task SendInput_Token(Game game, List<MacroModel> macroList, CancellationToken cancellationToken)
        {
            Console.WriteLine("Trying to doot on game pid " + game.Pid + ".");

            bool isStop = !BmpSeer.Instance.Started || !BmpGrunt.Instance.Started;

            foreach (var item in macroList)
            {
                item.key = (Keys)item.keyNumber;

                Console.WriteLine($"Key: {item.key}");
                var s = TimeSpan.FromSeconds(item.sleep).TotalMilliseconds;
                item.sleep = Convert.ToInt32(s);

                var delayTask = Task.Delay(item.sleep, cancellationToken);

                switch (item.type)
                {
                    case Types.button:
                        if (game != null && !await game.SendKeyArray(item.key))
                        {
                            Console.WriteLine("Failed to call game pid " + game.Pid + " to input keys :(");
                        }
                        break;
                    case Types.text:
                        string[] lines = item.inputText.Split(
                            new string[] { "\r\n", "\r", "\n" },
                            StringSplitOptions.None
                        );

                        foreach (string line in lines)
                        {
                            await Task.WhenAny(delayTask, game.SendLyricLine(line));

                            if (cancellationToken.IsCancellationRequested && isStop)
                            {
                                Console.WriteLine($"已經立即暫停!");
                                return; // Exit the method if cancellation is requested
                            }

                            await Task.Delay(item.sleep);
                        }

                        break;
                }

                await Task.WhenAny(delayTask);

                if (cancellationToken.IsCancellationRequested && isStop)
                {
                    Console.WriteLine($"已經立即暫停!");
                    return;
                }
            }
        }

        public static MacroRootModel ConvertJsonToList(string jsonText)
        {
            JsonDocument jsonDocument = JsonDocument.Parse(jsonText);
            MacroRootModel rootModel = new MacroRootModel();

            rootModel.rootID =  jsonDocument.RootElement.GetProperty("rootID").GetString();
            rootModel.categoryList = new List<CategoryModel>();
            var categoryList = jsonDocument.RootElement.GetProperty("categoryList");

            foreach (JsonElement item in categoryList.EnumerateArray())
            {
                CategoryModel categoryModel = new CategoryModel();

                categoryModel.id = item.GetProperty("id").GetString();
                categoryModel.name = item.GetProperty("name").GetString();
                categoryModel.category  = item.GetProperty("category").GetString();
                categoryModel.repeat = item.GetProperty("repeat").GetInt16();
                categoryModel.macroList = new List<MacroModel>();
                
                var macroList = item.GetProperty("macroList");

                foreach (var subItem in macroList.EnumerateArray())
                {
                    MacroModel model = new MacroModel();

                    string typeStr = subItem.GetProperty("type").GetRawText();

                    if (Enum.TryParse(typeStr, out Types type))
                    {
                        model.type = type;
                        model.typeNumber = (int)type;
                    }

                    //string commandStr = subItem.GetProperty("key").GetString();

                    //if (Enum.TryParse(commandStr, out Keys key))
                    //{
                    //    model.key = key;
                    //}

                    var keyNumber = subItem.GetProperty("keyNumber").GetUInt16();

                    model.keyNumber = keyNumber;

                    string inputText = subItem.GetProperty("inputText").GetString();

                    model.inputText = inputText;

                    int coordinateX = subItem.GetProperty("coordinateX").GetInt16();

                    model.coordinateX = coordinateX;

                    int coordinateY = subItem.GetProperty("coordinateY").GetInt16();

                    model.coordinateY = coordinateY;

                    model.sleep = subItem.GetProperty("sleep").GetInt16();

                    model.group = categoryModel.id;

                    categoryModel.macroList.Add(model);
                }

                rootModel.categoryList.Add(categoryModel);
            }

            return rootModel;
        }
    }
}
