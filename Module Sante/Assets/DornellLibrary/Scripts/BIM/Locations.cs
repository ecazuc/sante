using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dornell.BIM {
    public class Locations {
        public static readonly string LocationTag = "Location";

        private Dictionary<string, Transform> locations;

        public bool Init() {
            locations = new Dictionary<string, Transform>();

            try {
                foreach (GameObject location in GameObject.FindGameObjectsWithTag(LocationTag)) {
                    int count = 0;
#if NETSTANDARD2_1
                    while (!locations.TryAdd((count > 0) ? $"{location.name}_{count}" : location.name, location.transform)) {
                        ++count;
                    }
#else
                    string location_name = location.name;
                    while (locations.ContainsKey(location_name)) {
                        location_name = $"{location.name}_{++count}";
                    }
                    locations.Add(location_name, location.transform);
#endif
                }
            } catch (UnityException) {
                Debug.LogError($"Couldn't get locations from tag {LocationTag}. Maybe it's missing in project tags ?");
                return false;
            }

            return true;
        }

        public IEnumerable<KeyValuePair<string, Transform>> GetLocationsContaining(string text) {
            text = text.Trim();
            if (text.Length == 0)
                return locations;
#if NETSTANDARD2_1
            return locations.Where(location => location.Key.Contains(text, StringComparison.CurrentCultureIgnoreCase));
#else
            return locations.Where(location => location.Key.Contains(text));
#endif
        }

        public Transform GetLocation(string name) {
#if NETSTANDARD2_1
            return locations.First(location => location.Key.Contains(name, StringComparison.CurrentCultureIgnoreCase)).Value;
#else
            return locations.First(location => location.Key.Contains(name)).Value;
#endif
        }

    }

}