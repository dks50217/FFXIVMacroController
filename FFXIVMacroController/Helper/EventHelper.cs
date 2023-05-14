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
using FFXIVMacroController.Model;
using FFXIVMacroController.Seer.Events;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;
using static FFXIVMacroController.Helper.ClickOnPointTool;
using System.Text.RegularExpressions;
using System.Drawing;

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

                var s = TimeSpan.FromSeconds(item.sleep).TotalMilliseconds;
                item.sleep = Convert.ToInt32(s);

                switch (item.type)
                {
                    case Types.button:
                        if (game != null && !await game.SendKeyArray(item.key)) Console.WriteLine("Failed to call game pid " + game.Pid + " to input keys :(");
                        break;
                    case Types.mouse:
                        ClickOnPointTool.ClickOnPoint(game.Process.MainWindowHandle, item.coordinateX, item.coordinateY);
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


        public static MacroRootModel ConvertJsonToList(string jsonText)
        {
            JsonDocument jsonDocument = JsonDocument.Parse(jsonText);
            MacroRootModel rootModel = new MacroRootModel();

            rootModel.rootID =  jsonDocument.RootElement.GetProperty("rootID").GetInt16();
            rootModel.categoryList = new List<CategoryModel>();
            var categoryList = jsonDocument.RootElement.GetProperty("categoryList");

            foreach (JsonElement item in categoryList.EnumerateArray())
            {
                CategoryModel categoryModel = new CategoryModel();

                categoryModel.id = item.GetProperty("id").GetInt16();
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
                    categoryModel.macroList.Add(model);
                }

                rootModel.categoryList.Add(categoryModel);
            }

            return rootModel;
        }
    }
}
