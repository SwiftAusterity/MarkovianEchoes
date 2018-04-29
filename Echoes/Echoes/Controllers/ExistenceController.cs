﻿using Cottontail.Cache;
using Cottontail.FileSystem;
using Echoes.Data.Entity;
using Echoes.DataStructure.Entity;
using Echoes.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Echoes.Web.Controllers
{
    [AllowAnonymous]
    [Route("[controller]")]
    public class ExistenceController : Controller
    {
        private StoredDataCache _dataCache;
        private StoredData _data;
        private IHostingEnvironment _env;

        public ExistenceController(IHostingEnvironment env, StoredData storedData, StoredDataCache storedDataCache)
        {
            _env = env;
            _dataCache = storedDataCache;
            _data = storedData;
        }

        [HttpGet("", Name = "Existence_Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("{place}", Name = "Existence_Place")]
        public IActionResult Place(string place)
        {
            var viewModel = new ExistenceModel();
            var self = GetPersonaValues();
            var persona = GetPersona(self.Item1);
            viewModel.CurrentPlace = GetPlace(place);

            if (persona == null)
                viewModel.Errors = "You must choose a persona first.";
            else
            {
                viewModel.NewToYou = persona.AkashicRecord.Where(record => record.Timestamp >= self.Item2);
                viewModel.CurrentPersona = persona;
            }

            return View("Place", viewModel);
        }

        [HttpPost("{place}", Name = "Existence_Place_Input")]
        public IActionResult Input(string place, string input)
        {
            var viewModel = new ExistenceModel();
            var self = GetPersonaValues();
            var persona = GetPersona(self.Item1);
            viewModel.CurrentPlace = GetPlace(place);

            if (persona == null)
                viewModel.Errors = "You must choose a persona first.";
            else
            {
                viewModel.CurrentPlace.WriteTo(input, persona);
                viewModel.NewToYou = persona.AkashicRecord.Where(record => record.Timestamp >= self.Item2);
                viewModel.CurrentPersona = persona;
            }

            return View("Place", viewModel);
        }

        public IPlace GetPlace(string placeName)
        {
            var places = _dataCache.GetAll<Place>();
            IPlace currentPlace = null;

            //Find a default place
            if (String.IsNullOrWhiteSpace(placeName))
            {
                currentPlace = places.FirstOrDefault();

                //We have no places at all?
                if(currentPlace == null)
                    placeName = "The_Warren";
            }
            else
                currentPlace = places.FirstOrDefault(pl => pl.Name.Equals(placeName, StringComparison.InvariantCultureIgnoreCase));

            //Create one
            if (currentPlace == null)
            {
                currentPlace = new Place(_data, _dataCache)
                {
                    Name = placeName
                };

                currentPlace.Create();
            }

            return currentPlace;
        }

        public IPersona GetPersona(string personaName)
        {
            if (String.IsNullOrWhiteSpace(personaName))
                return null;

            var persona  = _dataCache.GetAll<Persona>().FirstOrDefault(per => per.Name.Equals(personaName, StringComparison.InvariantCultureIgnoreCase));

            //Make a new one
            if(persona == null)
            {
                persona = new Persona(_data, _dataCache)
                {
                    Name = personaName
                }; 
                persona.Create();
            }

            return persona;
        }

        public Tuple<string, DateTime> GetPersonaValues()
        {
            var personaName = string.Empty;
            var lastSeenDate = DateTime.Now;

            if (HttpContext.Request.Cookies.ContainsKey("persona"))
                personaName = HttpContext.Request.Cookies["persona"];

            if (HttpContext.Request.Cookies.ContainsKey("lastSeenDate"))
                DateTime.TryParse(HttpContext.Request.Cookies["lastSeenDate"], out lastSeenDate);

            return new Tuple<string, DateTime>(personaName, lastSeenDate);
        }
    }
}