﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFPackager.Interfaces;
using SFPackager.Models;

namespace SFPackager.Services
{
    public class VersionHandler
    {
        private readonly IHandleFiles _blobService;

        public VersionHandler(IHandleFiles blobService)
        {
            _blobService = blobService;
        }

        public async Task<GlobalVersion> LoadVersionAsync(VersionNumber version)
        {
            var result = await _blobService
                .GetFileAsStringAsync(version.FileName)
                .ConfigureAwait(false);

            if (!result.IsSuccessful)
                return null;

            var versionHashMap = JsonConvert.DeserializeObject<GlobalVersion>(result.ResponseContent);
            return versionHashMap;
        }

        public async Task SaveVersionAsync(GlobalVersion globalVersionHashMap, VersionNumber version)
        {
            var payload = JsonConvert.SerializeObject(globalVersionHashMap);
            var result = await _blobService
                .SaveFileAsync(version.FileName, payload)
                .ConfigureAwait(false);
        }

        public VersionNumber GetCurrentVersionFromApplications(List<ServiceFabricApplication> applications)
        {
            if (!applications.Any())
                return VersionNumber.Default();

            var parsedVersions = applications
                .Select(x => x.TypeVersion)
                .Select(VersionNumber.Parse)
                .ToList();

            var highestVersion = parsedVersions.Max(x => x.RollingNumber);
            return parsedVersions.First(x => x.RollingNumber == highestVersion);
        }
    }
}