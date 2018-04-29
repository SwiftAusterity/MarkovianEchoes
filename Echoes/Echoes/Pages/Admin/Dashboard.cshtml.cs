using Cottontail.Cache;
using Cottontail.FileSystem;
using Echoes.Data.Entity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;

namespace Echoes.Web.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        public int PersonaCount { get; set; }
        public int ThingCount { get; set; }
        public int PlaceCount { get; set;  }

        private StoredDataCache _dataCache;
        private StoredData _data;

        public DashboardModel(StoredData storedData, StoredDataCache storedDataCache)
        {
            _dataCache = storedDataCache;
            _data = storedData;
        }

        public void OnGet()
        {
            PersonaCount = _dataCache.GetAll<Persona>().Count();
            ThingCount = _dataCache.GetAll<Thing>().Count();
            PlaceCount = _dataCache.GetAll<Place>().Count();

        }
    }
}