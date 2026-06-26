using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonOptimizerApp
{
    // ۱. کلاس اصلی برای آیتم‌های ورودی شما
    public class AppContentItem
    {
        [JsonPropertyName("Content")]
        public string Content { get; set; }

        [JsonPropertyName("translationFa")]
        public string TranslationFa { get; set; }

        [JsonPropertyName("translationAr")]
        public string TranslationAr { get; set; }

        // این فیلد در JSON نهایی چاپ نمی‌شود
        [JsonIgnore]
        public string TempId { get; set; }

        // جادوی حفظ سایر فیلدهای تنظیمات UI بدون نیاز به تعریف آن‌ها
        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalData { get; set; }
    }

    // ۲. کلاسی برای خروجی نهایی اپلیکیشن
    public class FinalOutput
    {
        [JsonPropertyName("Data")]
        public List<AppContentItem> Data { get; set; }

        [JsonPropertyName("Interactives")]
        public List<string> Interactives { get; set; }
    }

    // ۳. کلاس‌های مربوط به ارتباط با مدل (Gemini)
    public class TextPayload
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class GeminiResponse
    {
        [JsonPropertyName("Data")]
        public List<TranslationData> Data { get; set; }

        [JsonPropertyName("Interactives")]
        public List<string> Interactives { get; set; }
    }

    public class TranslationData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("translationFa")]
        public string TranslationFa { get; set; }

        [JsonPropertyName("translationAr")]
        public string TranslationAr { get; set; }
    }
    // کلاس‌های مربوط به خروجی کلمات کلیدی طبق پرامپت شما
    public class InteractiveItem
    {
        [JsonPropertyName("exactText")] public string ExactText { get; set; }
        [JsonPropertyName("translationFa")] public string TranslationFa { get; set; }
        [JsonPropertyName("translationAr")] public string TranslationAr { get; set; }
        [JsonPropertyName("pronounceFa")] public string PronounceFa { get; set; }
        [JsonPropertyName("pronounceAr")] public string PronounceAr { get; set; }
        [JsonPropertyName("explanationFa")] public string ExplanationFa { get; set; }
        [JsonPropertyName("explanationAr")] public string ExplanationAr { get; set; }
        [JsonPropertyName("cefrLevel")] public string CefrLevel { get; set; }
    }

    // کلاسی برای خواندن امن پاسخ هوش مصنوعی (بعد از اصلاح ساختار)
    public class AiResponseWrapper
    {
        [JsonPropertyName("Data")] public JsonArray Data { get; set; }
        [JsonPropertyName("Interactives")] public List<InteractiveItem> Interactives { get; set; }
    }
}