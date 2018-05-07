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
        private readonly StoredDataFileAccessor DataStore;
        private readonly FileLogger Logger;

        public HotBackup(StoredDataFileAccessor dataStore, StoredDataCache dataCache, FileLogger logger)
        {
            DataStore = dataStore;
            DataCache = dataCache;
            Logger = logger;
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

            Logger.WriteToLog("World restored from data fallback.", LogChannels.Backup);

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
                Logger.WriteToLog("World backup to current INITIATED.", LogChannels.Backup);

                //DataStore.ArchiveFull();

                //Get all the entities (which should be a ton of stuff)
                //var entities = DataCache.GetAll();

                //Dont save players to the hot section, there's another place for them
                //foreach (var entity in entities)
                //    DataStore.WriteEntity(entity);

                Logger.WriteToLog("Live world written to current.", LogChannels.Backup);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
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

            Logger.WriteToLog("World restored from current live INITIATED.", LogChannels.Backup);

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

                    var args = new object[] { DataStore, DataCache, Logger };

                    foreach (var file in entityFilesDirectory.EnumerateFiles())
                    {
                        var newEntity = (IEntity)DataStore.ReadEntity(file, type, args);
                        newEntity.SetAccessors(DataStore, DataCache, Logger);
                        entitiesToLoad.Add(newEntity);
                    }
                }

                foreach (var entity in entitiesToLoad.Where(ent => ent.GetType() == typeof(Place)).OrderBy(ent => ent.Created))
                    entity.SpawnNewInWorld();

                //Shove them all into the live system first
                foreach (var entity in entitiesToLoad.Where(ent => ent.GetType() != typeof(Place)).OrderBy(ent => ent.Created))
                {
                    if (entity.Position != null)
                        entity.SpawnNewInWorld(entity.Position);
                    else
                        entity.SpawnNewInWorld();
                }

                //Check we found actual data
                if (!entitiesToLoad.Any(ent => ent.GetType() == typeof(Place)))
                    throw new Exception("No places found, failover.");

                Logger.WriteToLog("World restored from current live.", LogChannels.Backup);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }

            return false;
        }
    }
}
