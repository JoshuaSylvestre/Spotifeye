using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;
using UnityEngine;

    public class Program : MonoBehaviour
    {
        private static string client_id = "ac4f84de91614c86a251c3aae0e9f944";
        private static string client_secret = "67a2f73aa8f8415a9e4ab51587afe44b";
        public Dictionary<string, double> info;


        public void search(string input)
        {
        if (input == null || input == "")
            Debug.Log("uh oh");
            //string[] strs = input.Split(';');
            //info = getTrack(strs[0], strs[1]);
            //Debug.Log("finished search");
        }


        public Dictionary<string, double> getTrack(string artist_name, string track_name)
        {
            HttpClient client = new HttpClient();

            string token = getToken();

            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + token);

            var response = client.GetAsync("https://api.spotify.com/v1/search?q=artist:\"" + artist_name + "\"%20track:\"" + track_name + "\"&type=track").Result;
            string responseString = null, previewUrl = null, songID = null;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content;

                responseString = responseContent.ReadAsStringAsync().Result;

                JObject parsedResponse = JObject.Parse(responseString);
                songID = (string)parsedResponse["tracks"]["items"][0]["id"];

                JArray items = (JArray)parsedResponse["tracks"]["items"];
                int numItems = items.Count;

                for (int i = 0; i < numItems; i++)
                {
                    if ((string)items[i]["preview_url"] != null)
                    {
                        previewUrl = (string)items[i]["preview_url"];
                        break;
                    }
                }
            }

            using (var webclient = new WebClient())
            {
                webclient.DownloadFile(previewUrl, "hd.mp3");
            }

            return getAudioFeatures(token, songID);
        }

        public string getToken()
        {
            HttpClient client = new HttpClient();

            var values = new Dictionary<string, string>
          {
             { "grant_type", "client_credentials" },
             { "client_id", client_id },
             { "client_secret", client_secret }
          };

            var content = new FormUrlEncodedContent(values);

            var response = client.PostAsync("https://accounts.spotify.com/api/token", content).Result;
            string responseString = null;
            string token = null;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content;

                responseString = responseContent.ReadAsStringAsync().Result;

                JObject parsedResponse = JObject.Parse(responseString);
                token = (string)parsedResponse["access_token"];
            }

            return token;
        }

        public Dictionary<string, double> getAudioFeatures(string token, string songID)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + token);

            var response = client.GetAsync("https://api.spotify.com/v1/audio-features/" + songID).Result;
            string responseString = null;
            var stats = new Dictionary<string, double> { { "", 0.0 } };

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content;
                responseString = responseContent.ReadAsStringAsync().Result;

                JObject parsedResponse = JObject.Parse(responseString);

                stats = new Dictionary<string, double>
            {
               { "energy", (double)parsedResponse["energy"] },
               { "danceability", (double)parsedResponse["danceability"] },
               { "loudness", (double)parsedResponse["loudness"] },
               { "tempo", (double)parsedResponse["tempo"] }
            };
            }

            return stats;
        }
    }