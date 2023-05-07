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

namespace FFXIVMacroController.Helper
{
    public class EventHelper
    {
        public static void SendTest(GameStarted seerEvent)
        {
            var game = seerEvent.Game;

            Console.WriteLine("Detected game pid " + game.Pid + ", sleep a thread for 3000ms to allow Seer to parse the dat files.");

            string jsonText = File.ReadAllText("config.json");

            var macroList = ConvertJsonToList(jsonText);

            Task.Run(async () =>
            {
                await Task.Delay(3000);

                Console.WriteLine("Trying to doot on game pid " + game.Pid + ".");

                foreach (var item in macroList)
                {
                    Console.WriteLine($"Key: {item.key}");

                    switch (item.type)
                    {
                        case Types.button:
                            if (game != null && !await game.SendKeyArray(item.key)) Console.WriteLine("Failed to call game pid " + game.Pid + " to input keys :(");
                            break;
                        case Types.mouse:
                            break;
                    }

                    await Task.Delay(item.sleep);
                }
            });
        }

        private static List<MacroModel> ConvertJsonToList(string jsonText)
        {
            List<MacroModel> macroList = new List<MacroModel>();

            JsonDocument jsonDocument = JsonDocument.Parse(jsonText);

            foreach (JsonElement element in jsonDocument.RootElement.EnumerateArray())
            {
                MacroModel model = new MacroModel();

                string typeStr = element.GetProperty("type").GetString();

                if (Enum.TryParse(typeStr, out Types type))
                {
                    model.type = type;
                }

                string commandStr = element.GetProperty("command").GetString();

                if (Enum.TryParse(commandStr, out Keys key))
                {
                    model.key = key;
                }

                model.sleep = element.GetProperty("sleep").GetInt16();
                macroList.Add(model);
            }

            return macroList;
        }
    }
}
