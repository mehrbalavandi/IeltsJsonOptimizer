using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web; // برای رفع مشکل یونیکد حروف فارسی و عربی
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IeltsJsonOptimizer
{
    public partial class Form1 : Form
    {
        // شمارنده برای تولید شناسه‌های ثابت و قابل پیش‌بینی
        private int _nodeCounter = 1;

        // نگهداری JSON اصلی در حافظه فرم تا زمان ادغام
        private JsonNode _originalJsonNode;

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// متد بازگشتی برای جستجوی عمیق JSON و پیدا کردن هر آرایه Spans
        /// </summary>
        private void ExtractPayloadsRecursive(JsonNode node, JsonArray payloadForGemini)
        {
            if (node is JsonObject obj)
            {
                // بررسی می‌کنیم که آیا این شیء دارای کلید Spans هست و آیا متن مصرف‌نشده‌ای دارد یا خیر
                if (obj.ContainsKey("Spans") && obj["Spans"] is JsonArray spansArray)
                {
                    bool hasValidTextSpan = spansArray.OfType<JsonObject>().Any(s =>
                        s.ContainsKey("Type") && s["Type"]?.ToString() == "text" && !s.ContainsKey("_isConsumed")
                    );

                    if (hasValidTextSpan)
                    {
                        string tempId = $"item_{_nodeCounter}";
                        _nodeCounter++;

                        // شناسه موقت را به همان شیءای می‌دهیم که شامل Spans است
                        obj["_tempId"] = tempId;

                        // استخراج متن تمام Spansهای زیرمجموعه
                        string allContents = GetTextFromSpans(spansArray);

                        if (!string.IsNullOrWhiteSpace(allContents))
                        {
                            var lightItem = new JsonObject
                            {
                                ["id"] = tempId,
                                ["allContents"] = allContents.Trim()
                            };
                            payloadForGemini.Add(lightItem);
                        }
                        else
                        {
                            obj.Remove("_tempId");
                        }
                    }
                }

                // ادامه جستجو در عمق سایر فیلدهای این آبجکت برای پیدا کردن Spansهای تو در تو
                foreach (var kvp in obj.ToList())
                {
                    ExtractPayloadsRecursive(kvp.Value, payloadForGemini);
                }
            }
            else if (node is JsonArray arr)
            {
                foreach (var item in arr)
                {
                    ExtractPayloadsRecursive(item, payloadForGemini);
                }
            }
        }

        /// <summary>
        /// استخراج تمام متن‌ها از آرایه Spans و چسباندن مستقیم آن‌ها
        /// </summary>
        private string GetTextFromSpans(JsonArray spans)
        {
            List<string> spanTexts = new List<string>();
            foreach (var sNode in spans.OfType<JsonObject>())
            {
                if (sNode.ContainsKey("Type") && sNode["Type"]?.ToString() == "text" && sNode.ContainsKey("Content") && !sNode.ContainsKey("_isConsumed"))
                {
                    spanTexts.Add(sNode["Content"].ToString());

                    // علامت‌گذاری به عنوان مصرف‌شده برای جلوگیری از تکرار
                    sNode["_isConsumed"] = true;
                }
            }
            return string.Join("", spanTexts);
        }

        /// <summary>
        /// ادغام ترجمه‌ها با ساختار اصلی JSON
        /// </summary>
        private void MergeTranslationsRecursive(JsonNode node, AiResponseWrapper aiData)
        {
            if (node is JsonObject obj)
            {
                // پاکسازی فلگ مصرف‌شدگی
                if (obj.ContainsKey("_isConsumed"))
                {
                    obj.Remove("_isConsumed");
                }

                // اگر این شیء همان شیءای باشد که قبلاً حاوی Spans بود و شناسه موقت گرفت
                if (obj.TryGetPropertyValue("_tempId", out JsonNode idNode))
                {
                    string id = idNode.ToString();

                    // اصلاح اصلی: استفاده از x.id به جای x["id"]
                    var translatedItem = aiData.Data.FirstOrDefault(x => x.id == id);

                    if (translatedItem != null)
                    {
                        // اصلاح دسترسی به پراپرتی‌ها با دات‌نوتیشن
                        if (!string.IsNullOrEmpty(translatedItem.translationFa))
                            obj["translationFa"] = translatedItem.translationFa;

                        if (!string.IsNullOrEmpty(translatedItem.translationAr))
                            obj["translationAr"] = translatedItem.translationAr;
                    }

                    // پاکسازی شناسه موقت
                    obj.Remove("_tempId");
                }

                foreach (var kvp in obj.ToList())
                {
                    MergeTranslationsRecursive(kvp.Value, aiData);
                }
            }
            else if (node is JsonArray arr)
            {
                foreach (var item in arr)
                {
                    MergeTranslationsRecursive(item, aiData);
                }
            }
        }

        // --- دکمه‌های رابط کاربری --- //

        private void btnGeneratePayload_Click(object sender, EventArgs e)
        {
            try
            {
                // تنظیمات برای خروجی تمیز و حل مشکل یونیکد فارسی/عربی
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                string inputJson = rtbInput.Text;

                // ریست کردن شمارنده در هر بار تولید Payload
                _nodeCounter = 1;

                // پارس کردن فایل اصلی
                _originalJsonNode = JsonNode.Parse(inputJson);
                JsonArray originalArray = _originalJsonNode.AsArray();

                JsonArray payloadForGemini = new JsonArray();
                ExtractPayloadsRecursive(originalArray, payloadForGemini);

                if (payloadForGemini.Count == 0)
                {
                    MessageBox.Show("هیچ آرایه Spans دارای متن در ورودی یافت نشد!");
                    return;
                }

                rtbJsonToSend.Text = payloadForGemini.ToJsonString(options);
                MessageBox.Show("Payload با موفقیت بر اساس آرایه Spans تولید شد.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در تولید ساختار: " + ex.Message);
            }
        }

        private void btnMergeAiResponse_Click(object sender, EventArgs e)
        {
            try
            {
                string aiResponseText = rtbAiResponse.Text;

                if (string.IsNullOrWhiteSpace(aiResponseText))
                {
                    MessageBox.Show("لطفاً پاسخ هوش مصنوعی را در کادر مربوطه Paste کنید.");
                    return;
                }

                if (_originalJsonNode == null)
                {
                    MessageBox.Show("فایل اصلی لود نشده است. لطفاً ابتدا مرحله تولید Payload را انجام دهید.");
                    return;
                }

                // آماده‌سازی پاسخ هوش مصنوعی برای پارس شدن
                string parsableJson = "{\"Data\":" + aiResponseText + "}";
                var aiData = JsonSerializer.Deserialize<AiResponseWrapper>(parsableJson);

                if (aiData?.Data != null)
                {
                    // ادغام داده‌ها در شیء اصلی حافظه
                    MergeTranslationsRecursive(_originalJsonNode, aiData);

                    // اعمال تنظیمات برای حل مشکل یونیکد
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };

                    // تولید خروجی آرایه اصلی
                    string finalArrayJson = _originalJsonNode.ToJsonString(options);

                    // فرمت‌دهی سفارشی خروجی نهایی (چسباندن Interactives بدون آکولاد والد)
                    string customFormatOutput = finalArrayJson;
                    if (aiData.Interactives != null)
                    {
                        string interactivesJson = JsonSerializer.Serialize(aiData.Interactives, options);
                        int lastBracketIndex = finalArrayJson.LastIndexOf(']');

                        if (lastBracketIndex >= 0)
                        {
                            customFormatOutput = finalArrayJson.Substring(0, lastBracketIndex)
                                               + "],\n\"Interactives\": \n"
                                               + interactivesJson;
                        }
                    }

                    rtbOutput.Text = customFormatOutput;
                    MessageBox.Show("ادغام در سطح Spans با موفقیت انجام شد!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ادغام اطلاعات (شاید فرمت JSON دریافتی معتبر نیست): " + ex.Message);
            }
        }
    }

    // کلاس‌های کمکی برای پارس کردن پاسخ مدل (در صورتی که در فایل دیگری نیستند)
    public class AiResponseWrapper
    {
        public List<TranslationItem> Data { get; set; }
        public List<object> Interactives { get; set; }
    }

    public class TranslationItem
    {
        public string id { get; set; }
        public string translationFa { get; set; }
        public string translationAr { get; set; }
    }
}