using System;
using System.Linq;
using System.Collections.Generic;

using RiotSharp;
using RiotSharp.Misc;
using RiotSharp.Endpoints.SummonerEndpoint;
using RiotSharp.Endpoints.SpectatorEndpoint;
using RiotSharp.Endpoints.StaticDataEndpoint.Champion;
using RiotSharp.Endpoints.StaticDataEndpoint.SummonerSpell;

namespace LoL_Summoner_Spells
{
    class GameApi
    {
        private const string uriChampion = "http://opgg-static.akamaized.net/images/lol/champion/";
        private const string uriSpell = "http://ddragon.leagueoflegends.com/cdn/6.24.1/img/spell/";

        private readonly string summonerName;
        private readonly string currentVersion;
        private readonly string region;

        private readonly ChampionListStatic championList;
        private readonly SummonerSpellListStatic spellList;
        private readonly Summoner summonerObject;
        private readonly RiotApi api;

        /// <summary>
        /// Constructor for the GameApi class.
        /// </summary>
        public GameApi(string _summonerName, string _region, string KEY)
        {
            api = RiotApi.GetDevelopmentInstance(KEY);
            summonerName = _summonerName;
            region = _region;

            try
            {
                currentVersion = api.DataDragon.Versions.GetAllAsync().Result.First();
                summonerObject = api.Summoner.GetSummonerByNameAsync((Region)Enum.Parse(typeof(Region), region), _summonerName).Result;
                championList = api.DataDragon.Champions.GetAllAsync(currentVersion).Result;
                spellList = api.DataDragon.SummonerSpells.GetAllAsync(currentVersion).Result;
            }
            catch (RiotSharpException) { }
        }

        /// <summary>
        /// Get the EnemyTeam from a current game.
        /// </summary>
        /// <returns><see cref="IEnumerable{T}" /></returns>
        public IEnumerable<CurrentGameParticipant> GetEnemyTeam()
        {
            List<CurrentGameParticipant> currentMatch =
                api.Spectator.GetCurrentGameAsync((Region)Enum.Parse(typeof(Region), region), summonerObject.Id).Result.Participants;

            long teamId = currentMatch.First(p => p.SummonerName == summonerName).TeamId;
            return currentMatch.Where(p => p.TeamId != teamId);
        }


        /// <summary>
        /// Get the uri image of every champion from a current game.
        /// </summary>
        /// <returns><see cref="List{T}" /></returns>
        public List<string> GetChampionsUri(IEnumerable<CurrentGameParticipant> participants)
        {
            List<string> championsUri = new List<string>();

            foreach (var participant in participants)
            {
                championsUri
                    .Add(uriChampion + championList.Champions
                    .Where(p => p.Value.Id == participant.ChampionId)
                    .First().Value.Image.Full);
            }
            return championsUri;
        }


        /// <summary>
        /// Get the uri image of every spell from a current game.
        /// </summary>
        /// <returns><see cref="List{T}" /></returns>
        public List<string> GetSpellUriList(IEnumerable<CurrentGameParticipant> participants)
        {
            List<string> spellUri = new List<string>();

            foreach (var participant in participants)
            {
                spellUri
                    .Add(uriSpell + spellList.SummonerSpells
                    .Where(p => p.Value.Id == participant.SummonerSpell1)
                    .First().Value.Image.Full);

                spellUri
                    .Add(uriSpell + spellList.SummonerSpells
                    .Where(p => p.Value.Id == participant.SummonerSpell2)
                    .First().Value.Image.Full);
            }

            return spellUri;
        }


        /// <summary>
        /// Get the CD cooldown of every spell in a current game.
        /// </summary>
        /// <returns><see cref="List{T}" /></returns>
        public List<int> GetSpellCD(IEnumerable<CurrentGameParticipant> participants)
        {
            List<int> cooldown = new List<int>();

            foreach (var participant in participants)
            {

                if (participant.SummonerSpell1 == 12)
                {
                    cooldown.Add(420);
                    cooldown
                            .Add(Int32.Parse(spellList.SummonerSpells
                            .Where(p => p.Value.Id == participant.SummonerSpell2)
                            .First().Value.CooldownBurn));
                }

                else if (participant.SummonerSpell2 == 12)
                {
                    cooldown
                          .Add(Int32.Parse(spellList.SummonerSpells
                          .Where(p => p.Value.Id == participant.SummonerSpell1)
                          .First().Value.CooldownBurn));
                    cooldown.Add(420);
                }

                else
                {
                    cooldown
                        .Add(Int32.Parse(spellList.SummonerSpells
                        .Where(p => p.Value.Id == participant.SummonerSpell1)
                        .First().Value.CooldownBurn));

                    cooldown
                        .Add(Int32.Parse(spellList.SummonerSpells
                        .Where(p => p.Value.Id == participant.SummonerSpell2)
                        .First().Value.CooldownBurn));
                }
            }

            return cooldown;
        }
    }
}
