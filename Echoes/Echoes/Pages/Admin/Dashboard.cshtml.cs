﻿using Cottontail.Cache;
using Cottontail.FileSystem;
using Echoes.DataStructure.Entity;
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
        private StoredDataFileAccessor _data;

        public DashboardModel(StoredDataFileAccessor storedData, StoredDataCache storedDataCache)
        {
            _dataCache = storedDataCache;
            _data = storedData;
        }

        public void OnGet()
        {
            PersonaCount = _dataCache.GetAll<IPersona>().Count();
            ThingCount = _dataCache.GetAll<IThing>().Count();
            PlaceCount = _dataCache.GetAll<IPlace>().Count();

        }
    }
}