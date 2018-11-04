using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Formatters;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace LeekWarsAPI
{
    public class Api
    {

        #region Attribute

        public string Url = "https://leekwars.com/api/";
        
        private static HttpClient Client;
        private HttpClientHandler _handler;
        
        private CookieContainer _cookiesContainer = new CookieContainer();
        protected List<Cookie> Cookies = null;
        
        protected Dictionary<string, string> Data;
        
        protected Farmer player;

        protected Garden garden;

        #endregion

        #region init

        public Api()
        {
            Init();
        }

        private void Init()
        {
            player = new Farmer();
            _handler = new HttpClientHandler();
            _handler.CookieContainer = _cookiesContainer;
            Client = new HttpClient(_handler);
            Cookies = new List<Cookie>();
            Data = new Dictionary<string, string>();
            garden = new Garden();
        }        

        #endregion

        #region Connection

        public async Task<bool> Connect(string login, string password)
        {
            Data["login"] = login;
            Data["password"] = password;
            try
            {
                Uri url = new Uri(Url + "farmer/login-token/" + login + "/" + password);
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var authenticationResponse = await Client.GetStringAsync(url);

                var responseCookies = _cookiesContainer.GetCookies(url).Cast<Cookie>();

                JObject json = JObject.Parse(authenticationResponse);
                if (json.Root["success"].ToString() == "True")
                {
                    Cookies = new List<Cookie>();
                    foreach (var cookie in (IEnumerable<Cookie>) responseCookies.GetEnumerator())
                    {
                        Cookies.Add(cookie);
                    }

                    UpdateCookie();
                    player.Id = int.Parse(json.Root["farmer"]["id"].ToString());
                    player.Talent = int.Parse(json.Root["farmer"]["talent"].ToString());
                    player.Login = login;
                    player.Leeks = new List<Leek>();
                    var leeks = json.Root["farmer"]["leeks"].First;
                    bool isEnd = false;
                    while (!isEnd)
                    {
                        try
                        {
                            Leek leek = new Leek();
                            leek.Id = int.Parse(leeks.First["id"].ToString());
                            leek.Name = leeks.First["name"].ToString();
                            leek.Level = int.Parse(leeks.First["level"].ToString());
                            leek.Talent = int.Parse(leeks.First["talent"].ToString());
                            player.Leeks.Add(leek);
                            leeks = leeks.Next;
                        }
                        catch (Exception e)
                        {
                            isEnd = true;
                        }
                    }
                    Console.WriteLine("[API]: connected to " + player.Login);
                    return true;
                }
                else
                {
                    Console.WriteLine("[API]: FAILED connected to " + player.Login);
                    return false;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("[API]: FAILED connected to " + player.Login);
                return false;
            }
        }

        public async Task<bool> Reconnect()
        {
            player = new Farmer();
            _handler = new HttpClientHandler();
            _handler.CookieContainer = _cookiesContainer;
            Client = new HttpClient(_handler);
            Cookies = new List<Cookie>();
            return await Connect(Data["login"], Data["password"]);
        }

        public async Task<bool> Disconnect()
        {
            try
            {
                Uri url = new Uri(Url + "farmer/disconnect/" + Data["token"]);
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var authenticationResponse = await Client.GetStringAsync(url);
                JObject json = JObject.Parse(authenticationResponse);
                if (json.Root["success"].ToString() == "True")
                {
                    Data.Remove("token");
                    Data.Remove("PHPSESSID");
                    player = null;
                    Cookies = null;
                    Console.WriteLine("[API]: disconnected");
                    return true;
                }
                else
                {
                    Console.WriteLine("[API]: FAILED disconnected");
                    return false;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("[API]: FAILED disconnected");
                return false;
            }
        }

        public async Task<bool> Update()
        {
            try
            {
                Uri url = new Uri(Url + "farmer/update/" + Data["token"]);
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var authenticationResponse = await Client.GetStringAsync(url);
                JObject json = JObject.Parse(authenticationResponse);
                if (json.Root["success"].ToString() == "True")
                {
                    Console.WriteLine("[API]: session update for " + player.Login);
                    return true;
                }
                else
                {
                    Console.WriteLine("[API]: FAILED session update for " + player.Login);
                    return false;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("[API]: FAILED session update for " + player.Login);
                return false;
            }
        }

        protected void UpdateCookie()
        {
            foreach (var c in Cookies)
            {
                Data[c.Name] = c.Value;
            }
        }       

        #endregion

        #region Tournament

        public async Task<bool> RegisterTounamentFarmer()
        {
            try
            {
                Uri url = new Uri(Url + "farmer/register-tournament/" + Data["token"]);
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var authenticationResponse = await Client.GetStringAsync(url);
                JObject json = JObject.Parse(authenticationResponse);
                if (json.Root["success"].ToString() == "True")
                {
                    Console.WriteLine("[FARMER][TOURNAMENT]: " + player.Login + " registered");
                    return true;
                }
                else
                {
                    Console.WriteLine("[FARMER][TOURNAMENT]: FAILED " + player.Login + " registered");
                    return false;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("[FARMER][TOURNAMENT]: FAILED " + player.Login + " registered");
                return false;
            } 
        }

        public async Task RegisterTounamentLeeks()
        {
            for (int i = 0; i < player.Leeks.Count; i++)
            {
                await RegisterTounamentLeek(i);
            }
        }
        
        private async Task<bool> RegisterTounamentLeek(int id)
        {
            try
            {
                Uri url = new Uri(Url + "leek/register-tournament/" + player.Leeks[id].Id.ToString() + "/" + Data["token"]);
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var authenticationResponse = await Client.GetStringAsync(url);
                JObject json = JObject.Parse(authenticationResponse);
                if (json.Root["success"].ToString() == "True")
                {
                    Console.WriteLine("[LEEK][TOURNAMENT]: " + player.Leeks[id].Name + " registered");
                    return true;
                }
                else
                {
                    Console.WriteLine("[LEEK][TOURNAMENT]: FAILED " + player.Leeks[id].Name + " registered");
                    return false;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("[LEEK][TOURNAMENT]: FAILED " + player.Leeks[id].Name + " registered");
                return false;
            }
        }

        #endregion

        #region garden

        public async Task<bool> GetGardenStats()
        {
            Uri url = new Uri(Url + "garden/get/" + Data["token"]);
            try
            {
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var authenticationResponse = await Client.GetStringAsync(url);
                JObject json = JObject.Parse(authenticationResponse);
                if (json.Root["success"].ToString() == "True")
                {
                    garden = new Garden
                    {
                        Fight = int.Parse(json.Root["garden"]["fights"].ToString()),
                        Opponents = new List<Leek>()
                    };
                    Console.WriteLine("[GARDEN][STATS]: Success | " + garden.Fight.ToString() + " remaining fights");
                    return true;
                }
                else
                {
                    Console.WriteLine("[GARDEN][STATS]: FAILED");
                    return false;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("[GARDEN][STATS]: FAILED");
                return false;
            }
        }

        public async Task<bool> GardenGetOpponents(int leekId)
        {
            Uri url = new Uri(Url + "garden/get-leek-opponents/" + player.Leeks[leekId].Id.ToString() + "/" + Data["token"]);
            try
            {
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var authenticationResponse = await Client.GetStringAsync(url);
                JObject json = JObject.Parse(authenticationResponse);
                if (json.Root["success"].ToString() == "True")
                {
                    var leeks = json.Root["opponents"].First;
                    bool isEnd = false;
                    garden.Opponents = new List<Leek>();
                    while (!isEnd)
                    {
                        try
                        {
                            Leek leek = new Leek();
                            leek.Id = int.Parse(leeks["id"].ToString());
                            leek.Name = leeks["name"].ToString();
                            leek.Level = int.Parse(leeks["level"].ToString());
                            leek.Talent = int.Parse(leeks["talent"].ToString());
                            garden.Opponents.Add(leek);
                            leeks = leeks.Next;
                        }
                        catch (Exception e)
                        {
                            isEnd = true;
                        }
                    }
                    
                    Console.WriteLine("[GARDEN][OPPONENTS]: Success | Found: " + garden.Opponents.Count.ToString() + " opponents for " + player.Leeks[leekId].Name);
                    foreach (var opponent in garden.Opponents)
                    {
                        Console.WriteLine("[GARDEN][OPPONENTS][LIST]: " + opponent.Name + " | level: " + opponent.Level + " | talent: " + opponent.Talent);
                    }
                    return true;
                }
                else
                {
                    Console.WriteLine("[GARDEN][OPPONENTS]: FAILED");
                    return false;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("[GARDEN][OPPONENTS]: FAILED");
                return false;
            }
        }

        #endregion

        

        
        
    }
}