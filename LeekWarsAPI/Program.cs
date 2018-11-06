using System;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LeekWarsAPI
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            Api api = new Api();
            
            var jsonFile = File.ReadAllText("config.json");
            JObject json = JObject.Parse(jsonFile);

            string Login;
            string Pass;
            
            try
            {
                api.Url = json.Root["bot"]["url"].ToString();
                
                Login = json.Root["bot"]["login"].ToString();
                Pass = json.Root["bot"]["pass"].ToString();
            }
            catch (Exception)
            {
                throw new SystemException("LEEKWARS BOT: failed to load or parse config file");
            }

            bool isConnect = await api.Connect(Login, Pass);

            if (!isConnect)
            {
                throw new SystemException("LEEKWARS BOT: cannot connect to your account");
            }
            
            bool updatesuccessfull = await api.Update();

            await api.RegisterTounamentFarmer();
            await api.RegisterTounamentLeeks();

            Console.WriteLine("\n\n");
            
            await api.GetGardenStats();
            
            Console.WriteLine("\n\n=======");
            Console.WriteLine("Your leeks: ");
            for (int i = 0; i < api.player.Leeks.Count; i++)
            {
                Console.WriteLine("[" + i.ToString() + "]: " + api.player.Leeks[i].Name
                                  + " | level: " + api.player.Leeks[i].Level.ToString() 
                                  + " | talent: " + api.player.Leeks[i].Talent);
            }

            int leekId = -1;
            while (leekId < 0 || leekId > api.player.Leeks.Count)
            {
                Console.Write("Leek number: ");
                leekId = Readint();
            }
            
            Console.WriteLine("\n=======");
            int numberFight = -1;
            while (numberFight < 0 || numberFight >= api.garden.Fight)
            {
                Console.Write("How many fight ? ");
                numberFight = Readint();
            }
            Console.WriteLine("\n\n=======\n\n");

            while (numberFight > 0)
            {
                await api.FightLeek(leekId);
                --numberFight;
                Api.Cooldown();
                Console.WriteLine("\n\n");
            }
            
            Console.WriteLine("\n\n");
            bool disconnect = await api.Disconnect();
        }

        public static int Readint()
        {
            string input = "";
            while ((input = Console.ReadLine()) == "");

            try
            {
                return int.Parse(input);
            }
            catch (Exception)
            {
                return Readint();
            }
        }
    }
}