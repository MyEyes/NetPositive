﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.ComponentModel;
using dnlib.DotNet;

namespace NetPositive.Scanner.Generic
{
    internal class GenericSignature
    {
        string RiskText = "";
        string Description = "";
        string Title = "";
        List<IGenericMarker> MarkerList = new();
        private List<string> Tags = new List<string>();
        public GenericSignature(JsonElement definition)
        {
            if (definition.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("Signature definition must be a JSON Object");
            JsonElement riskElement;
            if(!definition.TryGetProperty("risk",out riskElement) || riskElement.ValueKind != JsonValueKind.String)
                throw new ArgumentException("Risk text doesn't exist or isn't a string");
            this.RiskText = riskElement.GetString();

            JsonElement descriptionElement;
            if (!definition.TryGetProperty("desc", out descriptionElement) || descriptionElement.ValueKind != JsonValueKind.String)
                throw new ArgumentException("Risk text doesn't exist or isn't a string");
            this.Description = descriptionElement.GetString();

            JsonElement markerElement;
            if (!definition.TryGetProperty("marker", out markerElement))
                throw new ArgumentException("No marker element found");
            if(markerElement.ValueKind == JsonValueKind.String)
            {
                this.MarkerList.Add(MarkerParser.MarkerFromString(markerElement.GetString()));
            }
            else if(markerElement.ValueKind == JsonValueKind.Array)
            {
                for(int i = 0; i<markerElement.GetArrayLength(); i++)
                {
                    var curr = markerElement[i];
                    if (curr.ValueKind != JsonValueKind.String)
                        throw new ArgumentException("Marker Element isn't a string");
                    this.MarkerList.Add(MarkerParser.MarkerFromString(curr.GetString()));
                }
            }
            JsonElement titleElement;
            bool flag8 = !definition.TryGetProperty("title", out titleElement) || titleElement.ValueKind != JsonValueKind.String;
            if (flag8)
            {
                this.Title = this.Description;
            }
            else
            {
                this.Title = titleElement.GetString();
            }
            JsonElement tagsElement;
            bool flag9 = definition.TryGetProperty("tags", out tagsElement) && tagsElement.ValueKind == JsonValueKind.Array;
            if (flag9)
            {
                foreach (JsonElement tag in tagsElement.EnumerateArray())
                {
                    this.Tags.Add(tag.GetString());
                }
            }
        }

        public bool Scan(IResultContainer results, IMethod method)
        {
            for(int i=0; i<MarkerList.Count; i++)
            {
                if (!MarkerList[i].Check(method))
                {
                    return false;
                }
            }

            results.AddResult(new ScanResult
            {
                Risk = this.RiskText,
                Description = this.Description,
                Method = method,
                Title = this.Title,
                Tags = this.Tags
            });
            return true;
        }
    }
}
