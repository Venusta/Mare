﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MareSynchronos.API;
using MareSynchronos.Utils;

namespace MareSynchronos.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CharacterData
    {
        [JsonProperty]
        public List<FileReplacement> AllReplacements =>
            FileReplacements.Where(f => f.HasFileReplacement)
            .Concat(FileReplacements.SelectMany(f => f.Associated)).Where(f => f.HasFileReplacement)
            .Concat(FileReplacements.SelectMany(f => f.Associated).SelectMany(f => f.Associated)).Where(f => f.HasFileReplacement)
            .Distinct().OrderBy(f => f.GamePaths[0])
            .ToList();

        [JsonProperty]
        public string CacheHash { get; set; } = string.Empty;

        public List<FileReplacement> FileReplacements { get; set; } = new();

        [JsonProperty]
        public string GlamourerString { get; set; } = string.Empty;

        public bool IsReady => FileReplacements.All(f => f.Computed);

        [JsonProperty]
        public int JobId { get; set; } = 0;

        public string ManipulationString { get; set; } = string.Empty;

        public void AddAssociatedResource(FileReplacement resource, FileReplacement? mdlParent, FileReplacement? mtrlParent)
        {
            try
            {
                if (mdlParent == null)
                {
                    resource.IsInUse = true;
                    FileReplacements.Add(resource);
                    return;
                }

                var mdlReplacements = FileReplacements.Where(f => f == mdlParent && mtrlParent == null);
                foreach (var mdlReplacement in mdlReplacements)
                {
                    mdlReplacement.AddAssociated(resource);
                }

                var mtrlReplacements = FileReplacements.Where(f => f == mdlParent).SelectMany(a => a.Associated).Where(f => f == mtrlParent);
                foreach (var mtrlReplacement in mtrlReplacements)
                {
                    mtrlReplacement.AddAssociated(resource);
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex.Message);
            }
        }

        public void Invalidate(List<FileReplacement>? fileReplacements = null)
        {
            try
            {
                var fileReplacement = fileReplacements ?? FileReplacements.ToList();
                foreach (var item in fileReplacement)
                {
                    item.IsInUse = false;
                    Invalidate(item.Associated);
                    if (FileReplacements.Contains(item))
                    {
                        FileReplacements.Remove(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex.Message);
            }
        }

        public CharacterCacheDto ToCharacterCacheDto()
        {
            return new CharacterCacheDto()
            {
                FileReplacements = AllReplacements.Select(f => f.ToFileReplacementDto()).ToList(),
                GlamourerData = GlamourerString,
                Hash = CacheHash,
                JobId = JobId,
                ManipulationData = ManipulationString
            };
        }
        public override string ToString()
        {
            StringBuilder stringBuilder = new();
            foreach (var fileReplacement in FileReplacements.OrderBy(a => a.GamePaths[0]))
            {
                stringBuilder.AppendLine(fileReplacement.ToString());
            }
            return stringBuilder.ToString();
        }
    }
}