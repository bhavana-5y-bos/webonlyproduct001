using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;
using System;
using System.IO;

namespace BOS.StarterCode.Helpers
{
    public class JavaScriptConvert
    {
        public static HtmlString SerializeObject(Object value)
        {
            using (var stringWriter = new StringWriter())
            using (var jsonWriter = new JsonTextWriter(stringWriter))
            {
                var serialize = new JsonSerializer();
                //We dont want Quotes around Object names
                jsonWriter.QuoteName = false;
                serialize.Serialize(jsonWriter, value);
                return new HtmlString(stringWriter.ToString());
            }
        }
    }
}
