using Cottontail.Cache;
using Cottontail.FileSystem;
using Cottontail.FileSystem.Logging;
using Cottontail.Structure;
using Echoes.Data.Entity;
using Echoes.Data.System;
using Echoes.DataStructure.Entity;
using Echoes.DataStructure.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Echoes.Backup
{
    /// <summary>
    /// The engine behind the system that constantly writes out live data so we can reboot into the prior state if needs be
    /// BaseDirectory should end with a trailing slash
    /// </summary>
    public class HotBackup
    {
        private readonly StoredDataCache DataCache;
        private readonly StoredData DataStore;

        public HotBackup(StoredData dataStore, StoredDataCache dataCache)
        {
            DataStore = dataStore;
            DataCache = dataCache;
        }

        /// <summary>
        /// Something went wrong with restoring the live backup, this loads all persistence singeltons from the database (rooms, paths, spawns)
        /// </summary>
        /// <returns>success state</returns>
        public bool NewWorldFallback()
        {
            //Only load in stuff that is static and spawns as singleton
            PreLoadAll<IPlace>(typeof(Place));
            PreLoadAll<IThing>(typeof(Thing));
            PreLoadAll<IPersona>(typeof(Persona));

            LoggingUtility.Log(DataStore.RootDirectory, "World restored from data fallback.", LogChannels.Backup);

            return true;
        }

        /// <summary>
        /// Dumps everything of a single type into the cache from the database for BackingData
        /// </summary>
        /// <typeparam name="T">the type to get and store</typeparam>
        /// <returns>success status</returns>
        public bool PreLoadAll<T>(Type baseClass) where T : IData
        {
            foreach (IData thing in DataCache.GetAll<T>())
            {
                var entityThing = Activator.CreateInstance(baseClass, new object[] { (T)thing }) as IEntity;

                entityThing.UpsertToLiveWorldCache();
            }

            return true;
        }


        /// <summary>
        /// Writes the current live world content (entities, positions, etc) to the Current backup; archives whatever was already considered current
        /// </summary>
        /// <returns>Success state</returns>
        public bool WriteLiveBackup()
        {
            try
            {
                LoggingUtility.Log(DataStore.RootDirectory, "World backup to current INITIATED.", LogChannels.Backup);

                DataStore.ArchiveFull();

                //Get all the entities (which should be a ton of stuff)
                var entities = DataCache.GetAll();

                //Dont save players to the hot section, there's another place for them
                foreach (var entity in entities)
                    DataStore.WriteEntity(entity);

                LoggingUtility.Log(DataStore.RootDirectory, "Live world written to current.", LogChannels.Backup);

                return true;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(DataStore.RootDirectory, ex);
            }

            return false;
        }

        /// <summary>
        /// Restores live entity backup from Current
        /// </summary>
        /// <returns>Success state</returns>
        public bool RestoreLiveBackup()
        {
            var currentBackupDirectory = DataStore.BaseDirectory + DataStore.CurrentDirectoryName;

            //No backup directory? No live data.
            if (!Directory.Exists(currentBackupDirectory))
                return false;

            LoggingUtility.Log(DataStore.RootDirectory, "World restored from current live INITIATED.", LogChannels.Backup);

            try
            {
                //dont load players here
                var entitiesToLoad = new List<IEntity>();
                var implimentedTypes = typeof(EntityPartial).Assembly.GetTypes().Where(ty => ty.GetInterfaces().Contains(typeof(IEntity))
                                                                                                && ty.IsClass
                                                                                                && !ty.IsAbstract);

                foreach (var type in implimentedTypes)
                {
                    if (!Directory.Exists(currentBackupDirectory + type.Name))
                        continue;

                    var entityFilesDirectory = new DirectoryInfo(currentBackupDirectory + type.Name);

                    var args = new object[] { DataStore, DataCache };

                    foreach (var file in entityFilesDirectory.EnumerateFiles())
                    {
                        var newEntity = (IEntity)DataStore.ReadEntity(file, type, args);
                        newEntity.SetAccessors(DataStore, DataCache);
                        entitiesToLoad.Add(newEntity);
                    }
                }

                //Shove them all into the live system first
                foreach (var entity in entitiesToLoad.OrderBy(ent => ent.Created))
                    entity.UpsertToLiveWorldCache();

                //Check we found actual data
                if (!entitiesToLoad.Any(ent => ent.GetType() == typeof(Place)))
                    throw new Exception("No places found, failover.");

                //We have the containers contents and the birthmarks from the deserial
                //I don't know how we can even begin to do this type agnostically since the collections are held on type specific objects without some super ugly reflection
                foreach (Place entity in entitiesToLoad.Where(ent => ent.GetType() == typeof(Place)))
                {
                    foreach (var obj in entity.GetContents().Where(ent => ent.GetType().GetInterfaces().Contains(typeof(IThing))))
                    {
                        var fullObj = DataCache.Get<IThing>(new BackingDataCacheKey(typeof(IThing), obj.ID));
                        entity.MoveFrom(obj);
                        entity.MoveInto(fullObj);
                    }

                    foreach (var obj in entity.GetContents().Where(ent => ent.GetType().GetInterfaces().Contains(typeof(IPersona))))
                    {
                        var fullObj = DataCache.Get<IPersona>(new BackingDataCacheKey(typeof(IPersona), obj.ID));
                        entity.MoveFrom(obj);
                        entity.MoveInto(fullObj);
                    }
                }

                LoggingUtility.Log(DataStore.RootDirectory, "World restored from current live.", LogChannels.Backup);
                return true;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(DataStore.RootDirectory, ex);
            }

            return false;
        }
    }
}
