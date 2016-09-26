﻿using System.Collections.Generic;
using System.Linq;
using MapzenGo.Helpers;
using MapzenGo.Models.Factories;
using MapzenGo.Models.Settings;
using UnityEngine;

namespace MapzenGo.Models
{
    public class PlacesFactory : Factory
    {
        [SerializeField]
        private GameObject _labelPrefab;
        public override string XmlTag { get { return "places"; } }
        [SerializeField]
        protected PlacesFactorySettings FactorySettings;
        public override void Start()
        {
            base.Start();
            Query = (geo) => geo["geometry"]["type"].str == "Point";
        }

        public override void Create(Tile tile)
        {
            if (!(tile.Data.HasField(XmlTag) && tile.Data[XmlTag].HasField("features")))
                return;

            foreach (var entity in tile.Data[XmlTag]["features"].list.Where(x => Query(x)).SelectMany(geo => Create(tile, geo)))
            {
                if (entity != null)
                {
                    entity.transform.SetParent(tile.transform, false);
                    entity.transform.localScale = Vector3.one * 3/tile.transform.lossyScale.x;
                }
            }
        }

        protected override IEnumerable<MonoBehaviour> Create(Tile tile, JSONObject geo)
        {
            var kind = geo["properties"]["kind"].str.ConvertToPlaceType();
            var typeSettings = FactorySettings.GetSettingsFor<PlaceSettings>(kind);

            var go = Instantiate(_labelPrefab);
            var water = go.AddComponent<Place>();
            if (geo["properties"].HasField("name"))
                go.GetComponentInChildren<TextMesh>().text = geo["properties"]["name"].str;

            var c = geo["geometry"]["coordinates"];
            var dotMerc = GM.LatLonToMeters(c[1].f, c[0].f);
            var localMercPos = dotMerc - tile.Rect.Center;
            go.transform.position = localMercPos.ToVector3();

            SetProperties(geo, water, typeSettings);

            yield return water;
        }

        private static void SetProperties(JSONObject geo, Place place, PlaceSettings typeSettings)
        {
            place.Id = geo["properties"]["id"].ToString();
            if (geo["properties"].HasField("name"))
                place.Name = geo["properties"]["name"].str;
            place.Type = geo["type"].str;
            place.Kind = geo["properties"]["kind"].str;
            place.name = "place";
        }
    }
}
