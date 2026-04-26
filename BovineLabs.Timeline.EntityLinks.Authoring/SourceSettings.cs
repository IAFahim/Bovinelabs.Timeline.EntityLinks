using System;
using System.Collections.Generic;
using BovineLabs.Core.Keys;
using BovineLabs.Core.Settings;
using UnityEngine;

namespace BovineLabs.Timeline.EntityLinks.Authoring
{
    [SettingsGroup("Source")]
    public class SourceSettings : KSettingsBase<SourceSettings, byte>
    {
        [SerializeField] 
        private SourceSchema[] sourceSchemas = Array.Empty<SourceSchema>();
        
        public IReadOnlyList<SourceSchema> SourceSchemas => this.sourceSchemas;

        public override IEnumerable<NameValue<byte>> Keys
        {
            get
            {
                foreach (var schema in this.sourceSchemas)
                {
                    if (schema == null) continue;
                    yield return new NameValue<byte>(schema.name, schema.Id);
                }
            }
        }
    }
}
