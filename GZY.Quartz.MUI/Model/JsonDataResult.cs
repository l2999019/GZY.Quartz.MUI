using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace GZY.Quartz.MUI.Model
{
    public class JsonDataResult : JsonResult
    {
        public JsonDataResult(object values,object option):base(values,
            option)
        {


        }

        public override void ExecuteResult(ActionContext context)
        {
            var services = context.HttpContext.RequestServices;
            var executor = services.GetRequiredService<IActionResultExecutor<JsonResult>>();
            var typename = executor.GetType().FullName;
            if (typename.Equals("Microsoft.AspNetCore.Mvc.Infrastructure.SystemTextJsonResultExecutor"))
            {
                this.SerializerSettings = new System.Text.Json.JsonSerializerOptions()
                {
                    PropertyNamingPolicy = null,
                    WriteIndented = true,
                    Converters = { new DateTimeJsonConverter() },
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(UnicodeRanges.All)
                 };

            }
            if (typename.Equals("Microsoft.AspNetCore.Mvc.NewtonsoftJson.NewtonsoftJsonResultExecutor"))
            {
                this.SerializerSettings = new Newtonsoft.Json.JsonSerializerSettings()
                {
                    DateFormatString = "yyyy-MM-dd HH:mm:ss",
                    ContractResolver = null,
                    DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local,
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize
                };
            }
            base.ExecuteResult(context);
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            var services = context.HttpContext.RequestServices;
            var executor = services.GetRequiredService<IActionResultExecutor<JsonResult>>();
            var typename = executor.GetType().FullName;
            if (typename.Equals("Microsoft.AspNetCore.Mvc.Infrastructure.SystemTextJsonResultExecutor"))
            {
                this.SerializerSettings = new System.Text.Json.JsonSerializerOptions()
                {
                    PropertyNamingPolicy = null,
                    WriteIndented = true,
                    Converters = { new DateTimeJsonConverter() },
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(UnicodeRanges.All)
                };
            }
            if (typename.Equals("Microsoft.AspNetCore.Mvc.NewtonsoftJson.NewtonsoftJsonResultExecutor"))
            {
                this.SerializerSettings = new Newtonsoft.Json.JsonSerializerSettings()
                {
                    DateFormatString = "yyyy-MM-dd HH:mm:ss",
                    ContractResolver = null,
                    DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local,
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize
                };
            }
            return base.ExecuteResultAsync(context);
        }
        public JsonDataResult(object values) : base(values)
        {
           
        }

    }

    public class DateTimeJsonConverter : JsonConverter<DateTime>
    {
        private readonly string _dateFormatString;
        public DateTimeJsonConverter()
        {
            _dateFormatString = "yyyy-MM-dd HH:mm:ss";
        }

        public DateTimeJsonConverter(string dateFormatString)
        {
            _dateFormatString = dateFormatString;
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_dateFormatString));
        }
    }
}
