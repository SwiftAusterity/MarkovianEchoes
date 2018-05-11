using Cottontail.Cache;
using Cottontail.FileSystem;
using Cottontail.FileSystem.Logging;
using Echoes.Data.Contextual;
using Echoes.Data.Entity;
using Echoes.DataStructure.Entity;
using Echoes.DataStructure.System;
using Echoes.Web.Models;
using Microsoft.AspNetCore.Authorization;
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
        private StoredDataFileAccessor _data;
        private FileLogger _logger;

        public ExistenceController(StoredDataFileAccessor storedData, StoredDataCache storedDataCache, FileLogger logger)
        {
            _dataCache = storedDataCache;
            _data = storedData;
            _logger = logger;
        }

        [HttpGet("", Name = "Existence_Index")]
        public IActionResult Index()
        {
            var viewModel = new ExistenceModel();
            viewModel.KnownPlaces = _dataCache.GetAll<IPlace>();

            return View(viewModel);
        }

        [HttpGet("{place}", Name = "Existence_Place")]
        public IActionResult Place(string place)
        {
            var viewModel = new ExistenceModel();
            viewModel.KnownPlaces = _dataCache.GetAll<IPlace>();

            var self = GetPersonaValues();
            var persona = GetPersona(self.Item1);
            viewModel.CurrentPlace = GetPlace(place);

            if (persona == null)
                viewModel.Errors = "You must choose a persona first.";
            else
            {
                if (!persona.Position.Equals(viewModel.CurrentPlace) || !viewModel.CurrentPlace.PersonaInventory.Contains(persona))
                    viewModel.CurrentPlace.MoveInto(persona);

                viewModel.NewToYou = persona.AkashicRecord.Where(record => record.Timestamp >= self.Item3);
                viewModel.CurrentPersona = persona;
            }

            return View("Place", viewModel);
        }

        [HttpPost("{place}", Name = "Existence_Place_Input")]
        public IActionResult Input(string place, string input)
        {
            var viewModel = new ExistenceModel();
            viewModel.KnownPlaces = _dataCache.GetAll<IPlace>();

            var self = GetPersonaValues();
            var persona = GetPersona(self.Item1);
            viewModel.CurrentPlace = GetPlace(place);

            if (persona == null)
                viewModel.Errors = "You must choose a persona first.";
            else
            {
                if (!persona.Position.Equals(viewModel.CurrentPlace))
                    viewModel.CurrentPlace.MoveInto(persona);

                viewModel.NewToYou = persona.AkashicRecord.Where(record => record.Timestamp >= self.Item3);
                viewModel.CurrentPersona = persona;

                try
                {
                    viewModel.CurrentPlace.WriteTo(input, persona, self.Item2);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex);
                    viewModel.Errors = ex.Message;
                }
            }

            return View("Place", viewModel);
        }

        [HttpGet(@"{place}/{type}/{name}", Name ="Existence_Info_Popup")]
        public ActionResult GetEntityInfo(string place, string type, string name)
        {
            var vModel = new EntityInfoModel();
            IEntity entity = null;

            switch(type.ToLower())
            {
                case "thing":
                    entity = _dataCache.GetByName<IThing>(name);
                    break;
                case "persona":
                    entity = _dataCache.GetByName<IPersona>(name);
                    break;
                case "place":
                    entity = _dataCache.GetByName<IPlace>(name);
                    break;
            }

            if (entity == null)
            {
                vModel.Errors = "Invalid parameters.";
            }
            else
            {
                vModel.Entity = entity;

                if (type.Equals("place", StringComparison.InvariantCultureIgnoreCase))
                {
                    var thisPlace = (IPlace)entity;

                    vModel.Things = thisPlace.GetThings().Select(thing => thing.Name);
                    vModel.Personas = thisPlace.GetPersonas().Select(thing => thing.Name);
                }

                vModel.Actions = entity.FullContext.Where(ctx => ctx.GetType() == typeof(Verb)).Select(ctx => ctx.Name);
                vModel.Adjectives = entity.FullContext.Where(ctx => ctx.GetType() == typeof(Descriptor)).Select(ctx => ctx.Name);
            }

            return View(@"~/Views/Existence/EntityInfo.cshtml", vModel);
        }

        public IPlace GetPlace(string placeName)
        {
            IPlace currentPlace = null;

            //Find a default place
            if (String.IsNullOrWhiteSpace(placeName))
                Response.Redirect("/existence");
            else
                currentPlace = _dataCache.GetByName<IPlace>(placeName);

            //Create one
            if (currentPlace == null)
            {
                currentPlace = DataAccess.DataFactory.Create<IPlace>();
                currentPlace.Name = placeName;
                currentPlace.SetAccessors(_data, _dataCache, _logger);
                currentPlace.Create();
            }

            return currentPlace;
        }

        public IPersona GetPersona(string personaName)
        {
            if (String.IsNullOrWhiteSpace(personaName))
                return null;

            var persona  = _dataCache.GetByName<IPersona>(personaName);

            //Make a new one
            if (persona == null)
            {
                persona = DataAccess.DataFactory.Create<IPersona>();
                persona.Name = personaName;
                persona.SetAccessors(_data, _dataCache, _logger);
                persona.Create();
            }

            return persona;
        }

        public Tuple<string, bool, DateTime> GetPersonaValues()
        {
            var personaName = string.Empty;
            var lastSeenDate = DateTime.MinValue;
            var acting = true;

            if (HttpContext.Request.Cookies.ContainsKey("persona"))
                personaName = HttpContext.Request.Cookies["persona"];

            if (HttpContext.Request.Cookies.ContainsKey("modality"))
                acting = !HttpContext.Request.Cookies["modality"].Equals("speak");

            if (HttpContext.Request.Cookies.ContainsKey("lastSeenDate"))
                DateTime.TryParse(HttpContext.Request.Cookies["lastSeenDate"], out lastSeenDate);


            return new Tuple<string, bool, DateTime>(personaName, acting, lastSeenDate);
        }
    }
}